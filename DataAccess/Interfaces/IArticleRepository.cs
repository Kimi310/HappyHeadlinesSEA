using DataAccess.Models;

namespace DataAccess.Interfaces;

public interface IArticleRepository
{
    public Task<List<Article>> GetFromRegionAsync(string region);
}