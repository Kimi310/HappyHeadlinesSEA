namespace SubscriberCache.Configuration;

public class SubscriberCacheConfiguration
{
    public const string SectionName = "Cache";
    
    public string RedisConnectionString { get; set; } = "localhost:6379";
    public int MaxCachedArticles { get; set; } = 30;
    public int CacheTtlMinutes { get; set; } = 60;
    public string KeyPrefix { get; set; } = "subscribers:subscriber:";
    public string LruTrackingKey { get; set; } = "subscriber:lru:tracking";
}