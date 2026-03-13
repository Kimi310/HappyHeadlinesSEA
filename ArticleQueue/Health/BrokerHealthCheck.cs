using ArticleQueue.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ArticleQueue.Health;

public sealed class BrokerHealthCheck : IHealthCheck
{
    private readonly BrokerRuntimeState _state;

    public BrokerHealthCheck(BrokerRuntimeState state)
    {
        _state = state;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_state.IsRunning
            ? HealthCheckResult.Healthy("RabbitMQ topology service is running")
            : HealthCheckResult.Unhealthy("RabbitMQ topology service is not running"));
    }
}

