using Polly;
using Polly.CircuitBreaker;

namespace CommentService.Resilience;

public class ProfanityCircuitBreaker
{
    private readonly AsyncCircuitBreakerPolicy<HttpResponseMessage> _policy;

    public ProfanityCircuitBreaker(ILogger<ProfanityCircuitBreaker> logger)
    {
        _policy = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .OrResult(response => (int)response.StatusCode >= 500)
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 3,
                durationOfBreak: TimeSpan.FromSeconds(15),
                onBreak: (outcome, breakDelay) =>
                    logger.LogWarning("Profanity circuit opened for {Seconds}s", breakDelay.TotalSeconds),
                onReset: () => logger.LogInformation("Profanity circuit closed"),
                onHalfOpen: () => logger.LogInformation("Profanity circuit half-open"));
    }

    public CircuitState State => _policy.CircuitState;

    public Task<HttpResponseMessage> ExecuteAsync(Func<Task<HttpResponseMessage>> action) =>
        _policy.ExecuteAsync(action);
}
