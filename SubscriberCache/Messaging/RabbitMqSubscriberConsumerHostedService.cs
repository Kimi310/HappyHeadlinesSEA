using SubscriberCache.Models;
using SubscriberCache.Options;

namespace SubscriberCache.Messaging;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;


public sealed class RabbitMqSubscriberConsumerHostedService : BackgroundService
{
    private static readonly ActivitySource ActivitySource = new("SubscriberCache.Messaging");
    private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(3);

    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqSubscriberConsumerHostedService> _logger;
    private IConnection? _connection;
    private IModel? _channel;

    public RabbitMqSubscriberConsumerHostedService(
        RabbitMqOptions options,
        ILogger<RabbitMqSubscriberConsumerHostedService> logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.Host,
            Port = _options.Port,
            UserName = _options.Username,
            Password = _options.Password,
            DispatchConsumersAsync = true
        };

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                _channel.ExchangeDeclare(_options.Exchange, ExchangeType.Direct, durable: true, autoDelete: false);
                _channel.QueueDeclare(_options.Queue, durable: true, exclusive: false, autoDelete: false);
                _channel.QueueBind(_options.Queue, _options.Exchange, _options.RoutingKey);
                _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                _logger.LogInformation(
                    "SubscriberCache connected to RabbitMQ {Host}:{Port}. Queue={Queue}, Exchange={Exchange}, RoutingKey={RoutingKey}",
                    _options.Host,
                    _options.Port,
                    _options.Queue,
                    _options.Exchange,
                    _options.RoutingKey);

                await base.StartAsync(cancellationToken);
                return;
            }
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(
                    ex,
                    "RabbitMQ is not ready yet for SubscriberCache ({Host}:{Port}). Retrying in {DelaySeconds}s",
                    _options.Host,
                    _options.Port,
                    RetryDelay.TotalSeconds);
                await Task.Delay(RetryDelay, cancellationToken);
            }
        }

        throw new OperationCanceledException(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_channel is null)
            throw new InvalidOperationException("RabbitMQ channel was not initialized");

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (_, eventArgs) =>
        {
            var body = eventArgs.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var parentContext = Propagator.Extract(default, eventArgs.BasicProperties, ExtractTraceContextFromBasicProperties);
            Baggage.Current = parentContext.Baggage;

            using var activity = ActivitySource.StartActivity(
                "subscriber.received",
                ActivityKind.Consumer,
                parentContext.ActivityContext);

            activity?.SetTag("messaging.system", "rabbitmq");
            activity?.SetTag("messaging.destination.name", _options.Queue);
            activity?.SetTag("messaging.rabbitmq.exchange", _options.Exchange);
            activity?.SetTag("messaging.operation", "subscriber_received");

            try
            {
                var subscriberEvent = JsonSerializer.Deserialize<Subscriber>(message);
                if (subscriberEvent is null)
                {
                    _logger.LogWarning("Received empty subscriber event payload");
                    _channel.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: false);
                    return;
                }

                activity?.SetTag("messaging.message.id", subscriberEvent.Id.ToString());
                activity?.SetTag("subscriber.email", subscriberEvent.Email);
                activity?.SetTag("subscriber.continent", subscriberEvent.Continent);
                activity?.SetTag("subscriber.subscribed_at", subscriberEvent.SubscribedAtUtc.ToString("o"));

                _channel.BasicAck(eventArgs.DeliveryTag, multiple: false);
                _logger.LogInformation(
                    "Subscriber {SubscriberId} ({Email}) received and cached (Subscribed: {SubscribedAt})",
                    subscriberEvent.Id,
                    subscriberEvent.Email,
                    subscriberEvent.SubscribedAtUtc);
            }
            catch (JsonException ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                _logger.LogError(ex, "Failed to deserialize subscriber event message");
                _channel.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: false);
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                _logger.LogError(ex, "Failed to process subscriber event message");
                _channel.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: false);
            }
        };

        _channel.BasicConsume(_options.Queue, autoAck: false, consumer);
        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _channel?.Close();
        _connection?.Close();
        _channel?.Dispose();
        _connection?.Dispose();

        _logger.LogInformation("SubscriberCache RabbitMQ consumer stopped");
        return base.StopAsync(cancellationToken);
    }

    private static IEnumerable<string> ExtractTraceContextFromBasicProperties(IBasicProperties properties, string key)
    {
        if (properties.Headers is null || !properties.Headers.TryGetValue(key, out var value) || value is null)
            return Array.Empty<string>();

        return value switch
        {
            byte[] bytes => new[] { Encoding.UTF8.GetString(bytes) },
            string text => new[] { text },
            _ => Array.Empty<string>()
        };
    }
}