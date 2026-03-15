using ArticleService.DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace ArticleService.DataAccess.Repositories;

public class ArticleRepository : IArticleRepository
{
    private readonly IArticleDbContextFactory _factory;

    public ArticleRepository(IArticleDbContextFactory factory)
    {
        _factory = factory;
    }

    public async Task<Article> UpdateAsync(Article article)
    {
        var context = _factory.Create(article.Continent, article.IsGlobal);
        context.Articles.Update(article);
        await context.SaveChangesAsync();
        return article;
    }
    
    public async Task<Article> CreateAsync(Article article)
    {
        var context = _factory.Create(article.Continent, article.IsGlobal);
        context.Articles.Add(article);
        await context.SaveChangesAsync();
        return article;
    }

    public async Task DeleteAsync(Article article)
    {
        var context = _factory.Create(article.Continent, article.IsGlobal);
        context.Articles.Remove(article);
        await context.SaveChangesAsync();
    }

    public async Task<List<Article>> GetFromRegionAsync(string region)
    {
        var isGlobal = IsGlobalRegion(region);
        var context = _factory.Create(region, isGlobal);
        return await context.Articles.ToListAsync();
    }

    public async Task<List<Article>> GetFromRegionSinceAsync(string region, DateTime sinceUtc, CancellationToken cancellationToken = default)
    {
        var isGlobal = IsGlobalRegion(region);
        var context = _factory.Create(region, isGlobal);
        return await context.Articles
            .Where(article => article.PublishedAtUtc >= sinceUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<Article> GetByIdAsync(Guid id, string region)
    {
        var isGlobal = IsGlobalRegion(region);
        var context = _factory.Create(region, isGlobal);
        return await context.Articles.FirstOrDefaultAsync(a => a.Id == id);
    }

    private static bool IsGlobalRegion(string region)
    {
        return string.Equals(region, "Global", StringComparison.OrdinalIgnoreCase);
    }
}