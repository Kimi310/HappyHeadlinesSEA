using DataAccess.Models;

namespace DataAccess.Interfaces;

public interface IArticleRepository
{
    public Task<List<Article>> GetFromRegionAsync(string region);
    public Task<Article> CreateAsync(Article article);
    public Task DeleteAsync(Article article);
    public Task<Article> GetByIdAsync(Guid id, string region);
}