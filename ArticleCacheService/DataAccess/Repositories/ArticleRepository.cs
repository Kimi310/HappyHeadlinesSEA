using ArticleCacheService.DataAccess.Interfaces;
using ArticleCacheService.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace ArticleCacheService.DataAccess.Repositories;

public class ArticleRepository(IArticleDbContextFactory factory) : IArticleRepository
{
    public async Task<List<Article>> GetFromRegionAsync(string region, CancellationToken cancellationToken = default)
    {
        var isGlobal = IsGlobalRegion(region);
        var context = factory.Create(region, isGlobal);
        return await context.Articles.ToListAsync(cancellationToken);
    }

    public async Task<List<Article>> GetFromRegionSinceAsync(string region, DateTime sinceUtc, CancellationToken cancellationToken = default)
    {
        var isGlobal = IsGlobalRegion(region);
        var context = factory.Create(region, isGlobal);
        return await context.Articles
            .Where(article => article.PublishedAtUtc >= sinceUtc)
            .ToListAsync(cancellationToken);
    }

    private static bool IsGlobalRegion(string region)
    {
        return string.Equals(region, "Global", StringComparison.OrdinalIgnoreCase);
    }
}

