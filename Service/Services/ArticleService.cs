using DataAccess.Interfaces;
using DataAccess.Models;
using Services.Interfaces;

namespace Services.Services;

public class ArticleService(IArticleRepository articleRepository) : IArticleService
{
    public async Task<List<Article>> GetArticlesFromRegion(string region)
    {
        return await articleRepository.GetFromRegionAsync(region);
    }

    public async Task<Article> AddArticle(Article article)
    {
        return await articleRepository.CreateAsync(article);
    }
    
    public async Task RemoveArticle(Guid id, string region)
    {
        var article = await articleRepository.GetByIdAsync(id, region);
        await articleRepository.DeleteAsync(article);
    }
}