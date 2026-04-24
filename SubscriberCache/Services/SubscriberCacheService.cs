using Newtonsoft.Json;
using StackExchange.Redis;
using SubscriberCache.Configuration;
using SubscriberCache.Models;
using IDatabase = Microsoft.EntityFrameworkCore.Storage.IDatabase;

namespace SubscriberCache.Services;

public class SubscriberCacheService: IDisposable
{
    private readonly IConnectionMultiplexer _redis;
    private readonly StackExchange.Redis.IDatabase _db;
    private readonly SubscriberCacheConfiguration _config;
    private readonly ILogger<SubscriberCacheService> _logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public SubscriberCacheService(
        IConnectionMultiplexer redis,
        SubscriberCacheConfiguration config,
        ILogger<SubscriberCacheService> logger)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _db = _redis.GetDatabase();
    }

    public async Task<List<Subscriber>?> TryGetSubscribersAsync(Guid articleId)
    {
        var cacheKey = GetCacheKey(articleId);

        try
        {
            var cachedData = await _db.StringGetAsync(cacheKey);

            if (cachedData.HasValue)
            {
                _logger.LogDebug("Cache HIT for article {ArticleId}", articleId);
                await UpdateLruTrackingAsync(articleId);
                return DeserializeComments(cachedData!);
            }

            _logger.LogDebug("Cache MISS for article {ArticleId}", articleId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting comments from cache for article {ArticleId}", articleId);
            return null;
        }
    }

    public async Task SetSubscribersAsync(List<Subscriber> subscribers, Guid subscriberId)
    {
        await _semaphore.WaitAsync();
        try
        {
            await EnforceCacheLimitAsync();

            var cacheKey = GetCacheKey(subscriberId);
            var serialized = SerializeComments(subscribers);
            
            var ttl = TimeSpan.FromMinutes(_config.CacheTtlMinutes);
            await _db.StringSetAsync(cacheKey,serialized, ttl);
            
            await UpdateLruTrackingAsync(subscriberId);
            
            _logger.LogDebug("Cached {Count} comments for article {subscriberId}", subscribers.Count, subscriberId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting comments in cache for article {subscriberId}", subscriberId);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task UpdateLruTrackingAsync(Guid subscriberId)
    {
        var score = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        await _db.SortedSetAddAsync(_config.LruTrackingKey, subscriberId.ToString(), score);
    }

    private async Task EnforceCacheLimitAsync()
    {
        var currentCount = await _db.SortedSetLengthAsync(_config.LruTrackingKey);
        
        if (currentCount >= _config.MaxCachedArticles)
        {
            var countToRemove = (int)currentCount - _config.MaxCachedArticles + 1;
            
            var lruEntries = await _db.SortedSetRangeByRankAsync(
                _config.LruTrackingKey, 
                0, 
                countToRemove - 1);

            foreach (var entry in lruEntries)
            {
                if (Guid.TryParse((string?)entry!, out var articleId))
                {
                    var cacheKey = GetCacheKey(articleId);
                    await _db.KeyDeleteAsync(cacheKey);
                    await _db.SortedSetRemoveAsync(_config.LruTrackingKey, entry);
                    
                    _logger.LogInformation("Evicted article {ArticleId} from cache (LRU)", articleId);
                }
            }
        }
    }

    public async Task InvalidateAsync(Guid articleId)
    {
        try
        {
            var cacheKey = GetCacheKey(articleId);
            await _db.KeyDeleteAsync(cacheKey);
            await _db.SortedSetRemoveAsync(_config.LruTrackingKey, articleId.ToString());
            
            _logger.LogInformation("Invalidated cache for article {ArticleId}", articleId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache for article {ArticleId}", articleId);
        }
    }

    public async Task AddOrUpdateCommentAsync(Subscriber subscriber)
    {
        try
        {
            var cachedData = await _db.StringGetAsync(subscriber.Id.ToString());

            if (cachedData.HasValue)
            {
                var subscribers = DeserializeComments(cachedData!);
                subscribers.RemoveAll(s => s.Id == subscriber.Id);
                subscribers.Add(subscriber);
                await SetSubscribersAsync(subscribers, new Guid());
                
                _logger.LogDebug("Updated comment {subscriberId} in cache", 
                    subscriber.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding/updating comment {subscriberId} in cache", subscriber.Id);
        }
    }

    public async Task RemoveCommentAsync(Guid subscriberId)
    {
        try
        {
            var cachedData = await _db.StringGetAsync(subscriberId.ToString());

            if (cachedData.HasValue)
            {
                var subscribers = DeserializeComments(cachedData!);
                var originalCount = subscribers.Count;
                subscribers.RemoveAll(c => c.Id == subscriberId);
                
                if (subscribers.Count < originalCount)
                {
                    await SetSubscribersAsync(subscribers, new Guid());
                    _logger.LogDebug("Removed comment {subscriberId} from cache", 
                        subscriberId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing comment {subscriberId} from cache", subscriberId);
        }
    }

    public async Task<CacheStats> GetStatsAsync()
    {
        try
        {
            var count = await _db.SortedSetLengthAsync(_config.LruTrackingKey);
            
            return new CacheStats
            {
                CachedArticleCount = (int)count,
                MaxCachedArticles = _config.MaxCachedArticles,
                CacheTtlMinutes = _config.CacheTtlMinutes
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache stats");
            return new CacheStats
            {
                CachedArticleCount = 0,
                MaxCachedArticles = _config.MaxCachedArticles,
                CacheTtlMinutes = _config.CacheTtlMinutes
            };
        }
    }

    private string GetCacheKey(Guid articleId) => $"{_config.KeyPrefix}{articleId}";

    private static string SerializeComments(List<Subscriber> subscribers) =>
        JsonConvert.SerializeObject(subscribers);

    private static List<Subscriber>? DeserializeComments(string json) =>
        JsonConvert.DeserializeObject<List<Subscriber>>(json) ?? new List<Subscriber>();

    public void Dispose()
    {
        _semaphore?.Dispose();
    }
}

public class CacheStats
{
    public int CachedArticleCount { get; set; }
    public int MaxCachedArticles { get; set; }
    public int CacheTtlMinutes { get; set; }
}
