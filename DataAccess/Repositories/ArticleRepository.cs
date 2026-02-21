using DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories;

public class ArticleRepository : IArticleRepository
{
    private readonly IArticleDbContextFactory _factory;

    public ArticleRepository(IArticleDbContextFactory factory)
    {
        _factory = factory;
    }

    public async Task CreateAsync(Article article)
    {
        var context = _factory.Create(article.Continent, article.IsGlobal);
        context.Articles.Add(article);
        await context.SaveChangesAsync();
    }

    public async Task<List<Article>> GetFromRegionAsync(string region)
    {
        var isGlobal = false;
        if (region == "Global")
        {
            isGlobal = true;
        }
        
        var context = _factory.Create(region, isGlobal);
        return await context.Articles.ToListAsync();
    }
}