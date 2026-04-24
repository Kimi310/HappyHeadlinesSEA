using Microsoft.EntityFrameworkCore.Storage;

namespace CommentCache.Services;

using CommentCache.Configuration;
using CommentCache.Models;
using StackExchange.Redis;

public class ArticleTracker
{
    private readonly IDatabase _db;
    private readonly CommentCacheConfiguration _config;
    private readonly ILogger<ArticleTracker> _logger;
    private const string ArticleTrackingKey = "articles:recent:tracking";
    private const int MaxTrackedArticles = 30;

    public ArticleTracker(
        IConnectionMultiplexer redis,
        CommentCacheConfiguration config,
        ILogger<ArticleTracker> logger)
    {
        _db = redis.GetDatabase();
        _config = config;
        _logger = logger;
    }

    public async Task TrackArticleAsync(ArticlePublishedEvent article)
    {
        try
        {
            var score = new DateTimeOffset(article.PublishedAtUtc).ToUnixTimeSeconds();
            await _db.SortedSetAddAsync(ArticleTrackingKey, article.Id.ToString(), score);
            
            _logger.LogDebug("Tracked article {ArticleId} published at {PublishedAt}", 
                article.Id, article.PublishedAtUtc);

            var count = await _db.SortedSetLengthAsync(ArticleTrackingKey);
            if (count > MaxTrackedArticles)
            {
                var toRemove = count - MaxTrackedArticles;
                var removed = await _db.SortedSetRemoveRangeByRankAsync(
                    ArticleTrackingKey, 
                    0, 
                    toRemove - 1);
                
                _logger.LogInformation(
                    "Removed {Count} oldest article(s) from tracking (keeping 30 most recent)",
                    removed);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking article {ArticleId}", article.Id);
        }
    }

    public async Task<List<Guid>> GetTrackedArticleIdsAsync()
    {
        try
        {
            var articleIds = await _db.SortedSetRangeByRankAsync(
                ArticleTrackingKey, 
                0, 
                -1, 
                Order.Descending);
            
            return articleIds
                .Select(id => Guid.TryParse((string?)id, out var guid) ? guid : Guid.Empty)
                .Where(id => id != Guid.Empty)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tracked article IDs");
            return new List<Guid>();
        }
    }

    public async Task<ArticleTrackingStats> GetStatsAsync()
    {
        try
        {
            var count = await _db.SortedSetLengthAsync(ArticleTrackingKey);
            
            return new ArticleTrackingStats
            {
                TrackedArticleCount = (int)count,
                MaxTrackedArticles = MaxTrackedArticles
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tracking stats");
            return new ArticleTrackingStats
            {
                TrackedArticleCount = 0,
                MaxTrackedArticles = MaxTrackedArticles
            };
        }
    }

    public async Task ClearAllAsync()
    {
        try
        {
            await _db.KeyDeleteAsync(ArticleTrackingKey);
            _logger.LogInformation("Cleared all tracked articles");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing tracked articles");
        }
    }
}

public class ArticleTrackingStats
{
    public int TrackedArticleCount { get; set; }
    public int MaxTrackedArticles { get; set; }
}
