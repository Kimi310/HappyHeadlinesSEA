namespace CommentCache.Configuration;

public class CommentCacheConfiguration
{
    public const string SectionName = "Cache";
    
    public string RedisConnectionString { get; set; } = "localhost:6379";
    public int MaxCachedArticles { get; set; } = 30;
    public int CacheTtlMinutes { get; set; } = 60;
    public string KeyPrefix { get; set; } = "comments:article:";
    public string LruTrackingKey { get; set; } = "comments:lru:tracking";
}