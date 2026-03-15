namespace CommentCache.Services;

using CommentCache.Configuration;
using CommentCache.Models;
using Newtonsoft.Json;
using StackExchange.Redis;

public class CommentCacheService : IDisposable
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;
    private readonly CommentCacheConfiguration _config;
    private readonly ILogger<CommentCacheService> _logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public CommentCacheService(
        IConnectionMultiplexer redis,
        CommentCacheConfiguration config,
        ILogger<CommentCacheService> logger)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _db = _redis.GetDatabase();
    }

    public async Task<List<Comment>?> TryGetCommentsAsync(Guid articleId)
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

    public async Task SetCommentsAsync(Guid articleId, List<Comment> comments)
    {
        await _semaphore.WaitAsync();
        try
        {
            await EnforceCacheLimitAsync();

            var cacheKey = GetCacheKey(articleId);
            var serialized = SerializeComments(comments);
            
            var ttl = TimeSpan.FromMinutes(_config.CacheTtlMinutes);
            await _db.StringSetAsync(cacheKey, serialized, ttl);
            
            await UpdateLruTrackingAsync(articleId);
            
            _logger.LogDebug("Cached {Count} comments for article {ArticleId}", comments.Count, articleId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting comments in cache for article {ArticleId}", articleId);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task UpdateLruTrackingAsync(Guid articleId)
    {
        var score = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        await _db.SortedSetAddAsync(_config.LruTrackingKey, articleId.ToString(), score);
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

    public async Task AddOrUpdateCommentAsync(Comment comment)
    {
        try
        {
            var cacheKey = GetCacheKey(comment.ArticleId);
            var cachedData = await _db.StringGetAsync(cacheKey);

            if (cachedData.HasValue)
            {
                var comments = DeserializeComments(cachedData!);
                comments.RemoveAll(c => c.Id == comment.Id);
                comments.Add(comment);
                await SetCommentsAsync(comment.ArticleId, comments);
                
                _logger.LogDebug("Updated comment {CommentId} in cache for article {ArticleId}", 
                    comment.Id, comment.ArticleId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding/updating comment {CommentId} in cache", comment.Id);
        }
    }

    public async Task RemoveCommentAsync(Guid articleId, Guid commentId)
    {
        try
        {
            var cacheKey = GetCacheKey(articleId);
            var cachedData = await _db.StringGetAsync(cacheKey);

            if (cachedData.HasValue)
            {
                var comments = DeserializeComments(cachedData!);
                var originalCount = comments.Count;
                comments.RemoveAll(c => c.Id == commentId);
                
                if (comments.Count < originalCount)
                {
                    await SetCommentsAsync(articleId, comments);
                    _logger.LogDebug("Removed comment {CommentId} from cache for article {ArticleId}", 
                        commentId, articleId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing comment {CommentId} from cache", commentId);
        }
    }

    public async Task ClearAllAsync()
    {
        try
        {
            var lruEntries = await _db.SortedSetRangeByRankAsync(_config.LruTrackingKey, 0, -1);
            
            foreach (var entry in lruEntries)
            {
                if (Guid.TryParse((string?)entry!, out var articleId))
                {
                    var cacheKey = GetCacheKey(articleId);
                    await _db.KeyDeleteAsync(cacheKey);
                }
            }
            
            await _db.KeyDeleteAsync(_config.LruTrackingKey);
            _logger.LogInformation("Cleared all cached comments");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cache");
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

    private static string SerializeComments(List<Comment> comments) =>
        JsonConvert.SerializeObject(comments);

    private static List<Comment> DeserializeComments(string json) =>
        JsonConvert.DeserializeObject<List<Comment>>(json) ?? new List<Comment>();

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
