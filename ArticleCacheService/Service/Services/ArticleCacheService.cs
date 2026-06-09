using System.Diagnostics;
using System.Text.Json;
using ArticleCacheService.DataAccess.Interfaces;
using ArticleCacheService.DataAccess.Models;
using ArticleCacheService.Options;
using ArticleCacheService.Service.Interfaces;
using ArticleCacheService.Service.Telemetry;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace ArticleCacheService.Service.Services;

public sealed class ArticleCacheService : IArticleCacheService
{
    private static readonly Dictionary<string, string> RegionAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Global"] = "Global",
        ["Europe"] = "Europe",
        ["Asia"] = "Asia",
        ["Africa"] = "Africa",
        ["NorthAmerica"] = "NorthAmerica",
        ["SouthAmerica"] = "SouthAmerica",
        ["Australia"] = "Australia",
        ["Antarctica"] = "Antarctica"
    };

    private static readonly string[] Regions =
    {
        "Global",
        "Europe",
        "Asia",
        "Africa",
        "NorthAmerica",
        "SouthAmerica",
        "Australia",
        "Antarctica"
    };

    private readonly IMemoryCache _memoryCache;
    private readonly IDistributedCache _distributedCache;
    private readonly IArticleRepository _articleRepository;
    private readonly IArticleCacheMetricsStore _metricsStore;
    private readonly CacheRuntimeState _runtimeState;
    private readonly ArticleCacheOptions _cacheOptions;
    private readonly ILogger<ArticleCacheService> _logger;

    public ArticleCacheService(
        IMemoryCache memoryCache,
        IDistributedCache distributedCache,
        IArticleRepository articleRepository,
        IArticleCacheMetricsStore metricsStore,
        CacheRuntimeState runtimeState,
        IOptions<ArticleCacheOptions> cacheOptions,
        ILogger<ArticleCacheService> logger)
    {
        _memoryCache = memoryCache;
        _distributedCache = distributedCache;
        _articleRepository = articleRepository;
        _metricsStore = metricsStore;
        _runtimeState = runtimeState;
        _cacheOptions = cacheOptions.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Article>> GetRegionArticlesAsync(string region, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var completed = false;
        try
        {
            var result = await GetRegionArticlesCoreAsync(region, cancellationToken);
            completed = true;
            return result;
        }
        finally
        {
            stopwatch.Stop();
            if (completed)
            {
                await _metricsStore.RecordLatencyAsync(stopwatch.Elapsed.TotalMilliseconds, cancellationToken);
            }
        }
    }

    private async Task<IReadOnlyList<Article>> GetRegionArticlesCoreAsync(string region, CancellationToken cancellationToken)
    {
        var normalizedRegion = NormalizeRegion(region);
        var cacheKey = BuildCacheKey(normalizedRegion);
        var recentCutoffUtc = DateTime.UtcNow.AddDays(-Math.Max(_cacheOptions.PreloadDays, 1));

        if (!_runtimeState.Enabled)
        {
            await _metricsStore.RecordDbQueryAsync(normalizedRegion, cancellationToken);
            return await _articleRepository.GetFromRegionSinceAsync(normalizedRegion, recentCutoffUtc, cancellationToken);
        }

        if (_memoryCache.TryGetValue<IReadOnlyList<Article>>(cacheKey, out var l1Articles) && l1Articles is not null)
        {
            await _metricsStore.RecordL1HitAsync(normalizedRegion, cancellationToken);
            return l1Articles;
        }

        await _metricsStore.RecordL1MissAsync(normalizedRegion, cancellationToken);

        try
        {
            var payload = await _distributedCache.GetStringAsync(cacheKey, cancellationToken);
            if (!string.IsNullOrWhiteSpace(payload))
            {
                var l2Articles = JsonSerializer.Deserialize<List<Article>>(payload);
                if (l2Articles is not null)
                {
                    await _metricsStore.RecordL2HitAsync(normalizedRegion, cancellationToken);
                    SetL1(cacheKey, l2Articles);
                    return l2Articles;
                }
            }

            await _metricsStore.RecordL2MissAsync(normalizedRegion, cancellationToken);
        }
        catch (Exception ex)
        {
            await _metricsStore.RecordL2MissAsync(normalizedRegion, cancellationToken);
            _logger.LogWarning(ex, "Failed to read L2 cache for region {Region}", normalizedRegion);
        }

        await _metricsStore.RecordDbQueryAsync(normalizedRegion, cancellationToken);
        var freshArticles = await _articleRepository.GetFromRegionSinceAsync(normalizedRegion, recentCutoffUtc, cancellationToken);
        await SetBothLayersAsync(cacheKey, freshArticles, cancellationToken);
        return freshArticles;
    }

    public async Task<ArticleCacheStatsSnapshot> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        var snapshot = await _metricsStore.GetSnapshotAsync(cancellationToken);
        return snapshot with { CacheEnabled = _runtimeState.Enabled };
    }

    public Task ResetStatsAsync(CancellationToken cancellationToken = default)
    {
        return _metricsStore.ResetAsync(cancellationToken);
    }

    public async Task InvalidateRegionAsync(string region, CancellationToken cancellationToken = default)
    {
        var normalizedRegion = NormalizeRegion(region);
        var cacheKey = BuildCacheKey(normalizedRegion);

        _memoryCache.Remove(cacheKey);

        try
        {
            await _distributedCache.RemoveAsync(cacheKey, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to invalidate L2 cache for region {Region}", normalizedRegion);
        }
    }

    public async Task WarmRecentArticlesAsync(CancellationToken cancellationToken = default)
    {
        var recentCutoffUtc = DateTime.UtcNow.AddDays(-Math.Max(_cacheOptions.PreloadDays, 1));

        foreach (var region in Regions)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var articles = await _articleRepository.GetFromRegionSinceAsync(region, recentCutoffUtc, cancellationToken);
            await SetBothLayersAsync(BuildCacheKey(region), articles, cancellationToken);
        }
    }

    private void SetL1(string cacheKey, IReadOnlyList<Article> articles)
    {
        _memoryCache.Set(cacheKey, articles, TimeSpan.FromMinutes(Math.Max(_cacheOptions.L1ExpirationMinutes, 1)));
    }

    private async Task SetBothLayersAsync(string cacheKey, IReadOnlyList<Article> articles, CancellationToken cancellationToken)
    {
        SetL1(cacheKey, articles);

        try
        {
            var payload = JsonSerializer.Serialize(articles);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(Math.Max(_cacheOptions.L2ExpirationMinutes, 1))
            };
            await _distributedCache.SetStringAsync(cacheKey, payload, options, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to write L2 cache for key {CacheKey}", cacheKey);
        }
    }

    private static string BuildCacheKey(string region) => $"article-cache:region:{region.ToLowerInvariant()}";

    private static string NormalizeRegion(string region)
    {
        var normalized = region?.Trim() ?? string.Empty;
        if (RegionAliases.TryGetValue(normalized, out var canonical))
        {
            return canonical;
        }

        throw new ArgumentException($"Unsupported region '{region}'", nameof(region));
    }
}

