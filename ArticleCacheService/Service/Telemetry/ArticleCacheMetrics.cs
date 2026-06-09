using StackExchange.Redis;

namespace ArticleCacheService.Service.Telemetry;

public interface IArticleCacheMetricsStore
{
    Task RecordL1HitAsync(string region, CancellationToken cancellationToken = default);
    Task RecordL1MissAsync(string region, CancellationToken cancellationToken = default);
    Task RecordL2HitAsync(string region, CancellationToken cancellationToken = default);
    Task RecordL2MissAsync(string region, CancellationToken cancellationToken = default);
    Task RecordDbQueryAsync(string region, CancellationToken cancellationToken = default);
    Task RecordLatencyAsync(double milliseconds, CancellationToken cancellationToken = default);
    Task ResetAsync(CancellationToken cancellationToken = default);
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

    private const string LatencyKey = "article-cache:stats:latency";
    private const string DbQueriesKey = "article-cache:stats:dbqueries";

    private readonly IDatabase _database = redis.GetDatabase();

    public Task RecordL1HitAsync(string region, CancellationToken cancellationToken = default)
        => IncrementAsync("l1", "hit", region);

    public Task RecordL1MissAsync(string region, CancellationToken cancellationToken = default)
        => IncrementAsync("l1", "miss", region);

    public Task RecordL2HitAsync(string region, CancellationToken cancellationToken = default)
        => IncrementAsync("l2", "hit", region);

    public Task RecordL2MissAsync(string region, CancellationToken cancellationToken = default)
        => IncrementAsync("l2", "miss", region);

    public async Task RecordDbQueryAsync(string region, CancellationToken cancellationToken = default)
    {
        try
        {
            await _database.StringIncrementAsync(DbQueriesKey);
            await _database.StringIncrementAsync(RegionDbQueriesKey(region));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to increment db-query metric for region {Region}", region);
        }
    }

    public async Task RecordLatencyAsync(double milliseconds, CancellationToken cancellationToken = default)
    {
        try
        {
            await _database.HashIncrementAsync(LatencyKey, "totalMs", milliseconds);
            await _database.HashIncrementAsync(LatencyKey, "count", 1);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to record latency metric");
        }
    }

    public async Task ResetAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var keys = new List<RedisKey>
            {
                "article-cache:stats:layer:l1",
                "article-cache:stats:layer:l2",
                LatencyKey,
                DbQueriesKey
            };

            foreach (var region in Regions)
            {
                keys.Add($"article-cache:stats:region:{region}:layer:l1");
                keys.Add($"article-cache:stats:region:{region}:layer:l2");
                keys.Add(RegionDbQueriesKey(region));
            }

            await _database.KeyDeleteAsync(keys.ToArray());
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to reset cache metrics");
        }
    }

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
            var regionDbQueries = await ReadCounterAsync(RegionDbQueriesKey(region));
            regions.Add(new ArticleCacheRegionStats(
                region,
                l1Region.Hit,
                l1Region.Miss,
                l2Region.Hit,
                l2Region.Miss,
                regionDbQueries));
        }

        var dbQueries = await ReadCounterAsync(DbQueriesKey);
        var dbQueriesAvoided = l1.Hit + l2.Hit;
        var avgLatencyMs = await ReadAvgLatencyAsync();

        return new ArticleCacheStatsSnapshot(
            new Dictionary<string, ArticleCacheLayerStats>(StringComparer.OrdinalIgnoreCase)
            {
                ["l1"] = l1,
                ["l2"] = l2
            },
            regions,
            dbQueries,
            dbQueriesAvoided,
            avgLatencyMs);
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

    private async Task<long> ReadCounterAsync(RedisKey key)
    {
        var value = await _database.StringGetAsync(key);
        return value.HasValue && value.TryParse(out long parsed) ? parsed : 0;
    }

    private async Task<double> ReadAvgLatencyAsync()
    {
        var entries = await _database.HashGetAllAsync(LatencyKey);
        var dict = entries.ToDictionary(entry => entry.Name.ToString(), entry => (double)entry.Value);

        dict.TryGetValue("totalMs", out var totalMs);
        dict.TryGetValue("count", out var count);

        return count <= 0 ? 0 : totalMs / count;
    }

    private static string RegionDbQueriesKey(string region) => $"article-cache:stats:region:{region}:dbqueries";
}
