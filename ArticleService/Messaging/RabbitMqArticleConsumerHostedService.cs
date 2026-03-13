using System.Text;
using System.Text.Json;
using System.Diagnostics;
using ArticleService.Options;
using ArticleService.Service.Interfaces;
using DataAccess.Models;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ArticleService.Messaging;

public sealed class RabbitMqArticleConsumerHostedService : BackgroundService
{
    private static readonly ActivitySource ActivitySource = new("ArticleService.Messaging");
    private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(3);
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqArticleConsumerHostedService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private IConnection? _connection;
    private IModel? _channel;

    public RabbitMqArticleConsumerHostedService(
        IOptions<RabbitMqOptions> options,
        ILogger<RabbitMqArticleConsumerHostedService> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _options = options.Value;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
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
                    "ArticleService connected to RabbitMQ {Host}:{Port}. Queue={Queue}",
                    _options.Host,
                    _options.Port,
                    _options.Queue);

                await base.StartAsync(cancellationToken);
                return;
            }
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(
                    ex,
                    "RabbitMQ is not ready yet for ArticleService ({Host}:{Port}). Retrying in {DelaySeconds}s",
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
        {
            throw new InvalidOperationException("RabbitMQ channel was not initialized");
        }

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (_, eventArgs) =>
        {
            var body = eventArgs.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var parentContext = Propagator.Extract(default, eventArgs.BasicProperties, ExtractTraceContextFromBasicProperties);
            Baggage.Current = parentContext.Baggage;

            using var activity = ActivitySource.StartActivity(
                "article consume",
                ActivityKind.Consumer,
                parentContext.ActivityContext);
            activity?.SetTag("messaging.system", "rabbitmq");
            activity?.SetTag("messaging.destination.name", _options.Queue);
            activity?.SetTag("messaging.rabbitmq.exchange", _options.Exchange);
            activity?.SetTag("messaging.operation", "process");

            try
            {
                var articleEvent = JsonSerializer.Deserialize<ArticlePublishedEvent>(message);
                if (articleEvent is null)
                {
                    _logger.LogWarning("Received empty article message payload");
                    _channel.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: false);
                    return;
                }

                var article = new Article
                {
                    Id = articleEvent.Id,
                    Title = articleEvent.Title,
                    Content = articleEvent.Content,
                    Continent = articleEvent.Continent,
                    IsGlobal = articleEvent.IsGlobal
                };

                activity?.SetTag("messaging.message.id", article.Id.ToString());
                activity?.SetTag("article.continent", article.Continent);
                activity?.SetTag("article.is_global", article.IsGlobal);

                if (!article.IsGlobal && string.IsNullOrWhiteSpace(article.Continent))
                {
                    _logger.LogWarning("Article {ArticleId} rejected because continent is missing", article.Id);
                    activity?.SetStatus(ActivityStatusCode.Error, "Continent is missing");
                    _channel.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: false);
                    return;
                }

                using var scope = _serviceScopeFactory.CreateScope();
                var articleService = scope.ServiceProvider.GetRequiredService<IArticleService>();
                await articleService.AddArticle(article);

                _channel.BasicAck(eventArgs.DeliveryTag, multiple: false);
                _logger.LogInformation("Article {ArticleId} persisted from queue", article.Id);
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                _logger.LogError(ex, "Failed to process queued article message");
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

        _logger.LogInformation("ArticleService RabbitMQ consumer stopped");
        return base.StopAsync(cancellationToken);
    }

    private static IEnumerable<string> ExtractTraceContextFromBasicProperties(IBasicProperties properties, string key)
    {
        if (properties.Headers is null || !properties.Headers.TryGetValue(key, out var value) || value is null)
        {
            return Array.Empty<string>();
        }

        return value switch
        {
            byte[] bytes => new[] { Encoding.UTF8.GetString(bytes) },
            string text => new[] { text },
            _ => Array.Empty<string>()
        };
    }
}

