using System.Diagnostics;
using System.Text;
using System.Text.Json;
using ArticleService.Options;
using DataAccess.Models;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using RabbitMQ.Client;

namespace ArticleService.Messaging;

public class RabbitMqArticleEventSubscribe : IArticleEventSubscriber, IDisposable
{
    private static readonly ActivitySource ActivitySource = new("PublisherService.Messaging");
    private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqArticleEventSubscribe> _logger;
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMqArticleEventSubscribe(
        IOptions<RabbitMqOptions> options,
        ILogger<RabbitMqArticleEventSubscribe> logger)
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
    
    public Task<Article> SubscribeArticleAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Use SubscribeArticlesAsync for batch processing");
    }

    public async Task<IEnumerable<Article>> SubscribeArticlesAsync(int batchSize = 100, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var articles = new List<Article>();
        var processedCount = 0;

        _logger.LogInformation("Starting to consume up to {BatchSize} messages from queue {Queue}", batchSize, _options.Queue);

        while (processedCount < batchSize && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                // Get a single message from the queue
                var result = _channel.BasicGet(_options.Queue, autoAck: false);

                if (result == null)
                {
                    _logger.LogInformation("No more messages available in queue {Queue}. Retrieved {Count} articles", _options.Queue, articles.Count);
                    break;
                }

                try
                {
                    // Extract trace context from message headers for distributed tracing
                    var parentContext = Propagator.Extract(default, result.BasicProperties, ExtractTraceContextFromBasicProperties);
                    Baggage.Current = parentContext.Baggage;

                    // Start activity for this message processing
                    using var activity = ActivitySource.StartActivity("ProcessArticleMessage", ActivityKind.Consumer, parentContext.ActivityContext);
                    activity?.SetTag("messaging.system", "rabbitmq");
                    activity?.SetTag("messaging.destination", _options.Queue);
                    activity?.SetTag("messaging.operation", "receive");
                    activity?.SetTag("messaging.batch_size", batchSize);

                    // Deserialize the message body to Article
                    var messageBody = Encoding.UTF8.GetString(result.Body.ToArray());

                    var article = JsonSerializer.Deserialize<Article>(messageBody);

                    if (article == null)
                    {
                        _logger.LogWarning("Failed to deserialize message to Article. Delivery tag: {DeliveryTag}", result.DeliveryTag);
                        _channel.BasicNack(result.DeliveryTag, false, false); // Reject and don't requeue
                        processedCount++;
                        continue;
                    }

                    // Add to collection
                    articles.Add(article);

                    // Acknowledge the message
                    _channel.BasicAck(result.DeliveryTag, false);
                    processedCount++;

                    _logger.LogDebug("Processed article {ArticleId} ({ProcessedCount}/{BatchSize})", 
                        article.Id, processedCount, batchSize);
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Error deserializing message from queue {Queue}. Delivery tag: {DeliveryTag}", 
                        _options.Queue, result.DeliveryTag);
                    _channel.BasicNack(result.DeliveryTag, false, false); // Reject and don't requeue
                    processedCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message from queue {Queue}. Delivery tag: {DeliveryTag}", 
                        _options.Queue, result.DeliveryTag);
                    _channel.BasicNack(result.DeliveryTag, false, true); // Reject and requeue for retry
                    processedCount++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during batch consumption from queue {Queue}", _options.Queue);
                break;
            }

            // Small delay to prevent tight loop
            await Task.Delay(10, cancellationToken);
        }

        _logger.LogInformation("Batch consumption completed. Retrieved {ArticleCount} articles from queue {Queue}", 
            articles.Count, _options.Queue);

        return articles;
    }

    private IEnumerable<string> ExtractTraceContextFromBasicProperties(IBasicProperties props, string key)
    {
        if (props.Headers != null && props.Headers.TryGetValue(key, out var value))
        {
            var bytes = value as byte[];
            return new[] { Encoding.UTF8.GetString(bytes) };
        }
        return Enumerable.Empty<string>();
    }
    
    public void Dispose()
    {
        _channel.Dispose();
        _connection.Dispose();
    }
}