

using DataAccess.Models;

namespace Services.Interfaces;

public interface IArticleService
{
    public Task<List<Article>> GetArticlesFromRegion(string region);
}