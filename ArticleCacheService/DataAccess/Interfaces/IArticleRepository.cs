using ArticleCacheService.DataAccess.Models;

namespace ArticleCacheService.DataAccess.Interfaces;

public interface IArticleRepository
{
    Task<List<Article>> GetFromRegionAsync(string region, CancellationToken cancellationToken = default);
    Task<List<Article>> GetFromRegionSinceAsync(string region, DateTime sinceUtc, CancellationToken cancellationToken = default);
}

