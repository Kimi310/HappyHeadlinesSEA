using ArticleQueue.Options;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace ArticleQueue.Services;

public sealed class QueueTopologyHostedService : IHostedService
{
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(3);
    private readonly ILogger<QueueTopologyHostedService> _logger;
    private readonly RabbitMqOptions _options;
    private readonly BrokerRuntimeState _state;
    private IConnection? _connection;
    private IModel? _channel;

    public QueueTopologyHostedService(
        ILogger<QueueTopologyHostedService> logger,
        IOptions<RabbitMqOptions> options,
        BrokerRuntimeState state)
    {
        _logger = logger;
        _options = options.Value;
        _state = state;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.Host,
            Port = _options.Port,
            UserName = _options.Username,
            Password = _options.Password
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

                _state.MarkRunning();

                _logger.LogInformation(
                    "ArticleQueue declared topology on RabbitMQ {Host}:{Port}. Exchange={Exchange}, Queue={Queue}, RoutingKey={RoutingKey}",
                    _options.Host,
                    _options.Port,
                    _options.Exchange,
                    _options.Queue,
                    _options.RoutingKey);

                return;
            }
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(
                    ex,
                    "RabbitMQ is not ready yet for ArticleQueue ({Host}:{Port}). Retrying in {DelaySeconds}s",
                    _options.Host,
                    _options.Port,
                    RetryDelay.TotalSeconds);
                await Task.Delay(RetryDelay, cancellationToken);
            }
        }

        throw new OperationCanceledException(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _channel?.Close();
        _connection?.Close();
        _state.MarkStopped();
        _logger.LogInformation("ArticleQueue topology service stopped");

        await Task.CompletedTask;
    }
}

