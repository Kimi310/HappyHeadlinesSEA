using System.Text;
using System.Text.Json;
using System.Diagnostics;
using DataAccess.Models;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using PublisherService.Options;
using RabbitMQ.Client;

namespace PublisherService.Messaging;

public sealed class RabbitMqArticleEventPublisher : IArticleEventPublisher, IDisposable
{
    private static readonly ActivitySource ActivitySource = new("PublisherService.Messaging");
    private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqArticleEventPublisher> _logger;
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMqArticleEventPublisher(
        IOptions<RabbitMqOptions> options,
        ILogger<RabbitMqArticleEventPublisher> logger)
    {
        _options = options.Value;
        _logger = logger;

        var factory = new ConnectionFactory
        {
            HostName = _options.Host,
            Port = _options.Port,
            UserName = _options.Username,
            Password = _options.Password
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.ExchangeDeclare(_options.Exchange, ExchangeType.Direct, durable: true, autoDelete: false);
        _channel.QueueDeclare(_options.Queue, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(_options.Queue, _options.Exchange, _options.RoutingKey);
    }

    public Task PublishArticleAsync(Article article, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var articleId = article.Id == Guid.Empty ? Guid.NewGuid() : article.Id;
        var payload = new ArticlePublishedEvent
        {
            Id = articleId,
            Title = article.Title,
            Content = article.Content,
            Continent = article.Continent,
            IsGlobal = article.IsGlobal,
            PublishedAtUtc = DateTime.UtcNow
        };

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload));
        var properties = _channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.Headers = new Dictionary<string, object>();

        using var activity = ActivitySource.StartActivity("article publish", ActivityKind.Producer);
        activity?.SetTag("messaging.system", "rabbitmq");
        activity?.SetTag("messaging.destination.name", _options.Queue);
        activity?.SetTag("messaging.rabbitmq.exchange", _options.Exchange);
        activity?.SetTag("messaging.operation", "publish");
        activity?.SetTag("messaging.message.id", payload.Id.ToString());

        var propagationContext = new PropagationContext(activity?.Context ?? Activity.Current?.Context ?? default, Baggage.Current);
        Propagator.Inject(propagationContext, properties, static (props, key, value) =>
        {
            props.Headers ??= new Dictionary<string, object>();
            props.Headers[key] = Encoding.UTF8.GetBytes(value);
        });

        _channel.BasicPublish(
            exchange: _options.Exchange,
            routingKey: _options.RoutingKey,
            basicProperties: properties,
            body: body);

        _logger.LogInformation("Published article event {ArticleId} to queue {Queue}", payload.Id, _options.Queue);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel.Dispose();
        _connection.Dispose();
    }
}

