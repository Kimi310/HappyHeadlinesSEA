using DataAccess.Models;

namespace ArticleService.DataAccess.Interfaces;

public interface IArticleRepository
{
    public Task<List<Article>> GetFromRegionAsync(string region);
    public Task<List<Article>> GetFromRegionSinceAsync(string region, DateTime sinceUtc, CancellationToken cancellationToken = default);
    public Task<Article> CreateAsync(Article article);
    public Task DeleteAsync(Article article);
    public Task<Article> GetByIdAsync(Guid id, string region);
    public Task<Article> UpdateAsync(Article article);
}