using ArticleCacheService.Options;
using ArticleCacheService.Service.Interfaces;
using Microsoft.Extensions.Options;

namespace ArticleCacheService.Service.Services;

public sealed class ArticleCachePreloadHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ArticleCacheOptions _cacheOptions;
    private readonly ILogger<ArticleCachePreloadHostedService> _logger;

    public ArticleCachePreloadHostedService(
        IServiceScopeFactory serviceScopeFactory,
        IOptions<ArticleCacheOptions> cacheOptions,
        ILogger<ArticleCachePreloadHostedService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _cacheOptions = cacheOptions.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await WarmUpAsync(stoppingToken);

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(Math.Max(_cacheOptions.PreloadIntervalMinutes, 1)));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await WarmUpAsync(stoppingToken);
        }
    }

    private async Task WarmUpAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var cacheService = scope.ServiceProvider.GetRequiredService<IArticleCacheService>();
            await cacheService.WarmRecentArticlesAsync(cancellationToken);
            _logger.LogInformation("Cache warmup completed for last {Days} days", Math.Max(_cacheOptions.PreloadDays, 1));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Normal shutdown path.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cache warmup failed");
        }
    }
}

