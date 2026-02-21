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

}