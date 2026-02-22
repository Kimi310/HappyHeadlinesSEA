

using DataAccess.Models;

namespace Services.Interfaces;

public interface IArticleService
{
    public Task<List<Article>> GetArticlesFromRegion(string region);
    
    public Task<Article> AddArticle(Article article);

    public Task RemoveArticle(Guid id, string region);
    public Task<Article> UpdateArticle(Article article);
}