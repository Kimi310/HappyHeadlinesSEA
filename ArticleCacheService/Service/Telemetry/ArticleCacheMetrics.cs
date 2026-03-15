using StackExchange.Redis;

namespace ArticleCacheService.Service.Telemetry;

public interface IArticleCacheMetricsStore
{
    Task RecordL1HitAsync(string region, CancellationToken cancellationToken = default);
    Task RecordL1MissAsync(string region, CancellationToken cancellationToken = default);
    Task RecordL2HitAsync(string region, CancellationToken cancellationToken = default);
    Task RecordL2MissAsync(string region, CancellationToken cancellationToken = default);
    Task<ArticleCacheStatsSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default);
}

public sealed class RedisArticleCacheMetricsStore(IConnectionMultiplexer redis, ILogger<RedisArticleCacheMetricsStore> logger)
    : IArticleCacheMetricsStore
{
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

    private readonly IDatabase _database = redis.GetDatabase();

    public Task RecordL1HitAsync(string region, CancellationToken cancellationToken = default)
        => IncrementAsync("l1", "hit", region);

    public Task RecordL1MissAsync(string region, CancellationToken cancellationToken = default)
        => IncrementAsync("l1", "miss", region);

    public Task RecordL2HitAsync(string region, CancellationToken cancellationToken = default)
        => IncrementAsync("l2", "hit", region);

    public Task RecordL2MissAsync(string region, CancellationToken cancellationToken = default)
        => IncrementAsync("l2", "miss", region);

    public async Task<ArticleCacheStatsSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default)
    {
        var l1 = await ReadLayerAsync("l1");
        var l2 = await ReadLayerAsync("l2");

        var regions = new List<ArticleCacheRegionStats>();
        foreach (var region in Regions)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var l1Region = await ReadLayerAsync("l1", region);
            var l2Region = await ReadLayerAsync("l2", region);
            regions.Add(new ArticleCacheRegionStats(
                region,
                l1Region.Hit,
                l1Region.Miss,
                l2Region.Hit,
                l2Region.Miss));
        }

        return new ArticleCacheStatsSnapshot(
            new Dictionary<string, ArticleCacheLayerStats>(StringComparer.OrdinalIgnoreCase)
            {
                ["l1"] = l1,
                ["l2"] = l2
            },
            regions);
    }

    private async Task IncrementAsync(string layer, string result, string region)
    {
        try
        {
            await _database.HashIncrementAsync($"article-cache:stats:layer:{layer}", result, 1);
            await _database.HashIncrementAsync($"article-cache:stats:region:{region}:layer:{layer}", result, 1);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to increment cache metric for layer {Layer}, result {Result}, region {Region}", layer, result, region);
        }
    }

    private async Task<ArticleCacheLayerStats> ReadLayerAsync(string layer, string? region = null)
    {
        var key = region is null
            ? $"article-cache:stats:layer:{layer}"
            : $"article-cache:stats:region:{region}:layer:{layer}";

        var entries = await _database.HashGetAllAsync(key);
        var dict = entries.ToDictionary(entry => entry.Name.ToString(), entry => (long)entry.Value);

        dict.TryGetValue("hit", out var hit);
        dict.TryGetValue("miss", out var miss);

        var total = hit + miss;
        var ratio = total == 0 ? 0 : (double)hit / total;
        return new ArticleCacheLayerStats(hit, miss, ratio);
    }
}

