using DataAccess.Interfaces;
using DataAccess.Models;

namespace DataAccess.Repositories;

public class ArticleRepository : IArticlerepository
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
}