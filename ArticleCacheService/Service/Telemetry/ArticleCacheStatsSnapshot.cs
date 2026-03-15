namespace ArticleCacheService.Service.Telemetry;

public sealed record ArticleCacheStatsSnapshot(
    Dictionary<string, ArticleCacheLayerStats> Layers,
    List<ArticleCacheRegionStats> Regions);

public sealed record ArticleCacheLayerStats(long Hit, long Miss, double Ratio);

public sealed record ArticleCacheRegionStats(
    string Region,
    long L1Hit,
    long L1Miss,
    long L2Hit,
    long L2Miss);

