using ArticleService.DataAccess.Interfaces;
using ArticleService.Service.Clients;
using ArticleService.Service.Interfaces;
using DataAccess.Models;

namespace ArticleService.Service.Services;

public class ArticleService(IArticleRepository articleRepository, IArticleCacheClient articleCacheClient) : IArticleService
{
    public async Task<List<Article>> GetArticlesFromRegion(string region)
    {
        var cachedArticles = await articleCacheClient.TryGetRegionArticlesAsync(region);
        if (cachedArticles is not null)
        {
            return cachedArticles;
        }

        return await articleRepository.GetFromRegionAsync(region);
    }

    public async Task<Article> AddArticle(Article article)
    {
        if (article.PublishedAtUtc == default)
        {
            article.PublishedAtUtc = DateTime.UtcNow;
        }

        var createdArticle = await articleRepository.CreateAsync(article);
        await articleCacheClient.InvalidateRegionAsync(GetRegionForArticle(createdArticle));
        return createdArticle;
    }
    
    public async Task RemoveArticle(Guid id, string region)
    {
        var article = await articleRepository.GetByIdAsync(id, region);
        await articleRepository.DeleteAsync(article);
        await articleCacheClient.InvalidateRegionAsync(region);
    }

    public async Task<Article> UpdateArticle(Article article)
    {
        var articleToUpdate = await articleRepository.GetByIdAsync(article.Id, article.Continent);
        UpdateArticleValues(articleToUpdate, article);
        var updatedArticle = await articleRepository.UpdateAsync(articleToUpdate);
        await articleCacheClient.InvalidateRegionAsync(GetRegionForArticle(updatedArticle));
        return updatedArticle;
    }

    private static void UpdateArticleValues(Article dbArticle, Article newArticle)
    {
        dbArticle.Title = newArticle.Title;
        dbArticle.Content = newArticle.Content;
    }

    private static string GetRegionForArticle(Article article)
    {
        return article.IsGlobal ? "Global" : article.Continent;
    }
}