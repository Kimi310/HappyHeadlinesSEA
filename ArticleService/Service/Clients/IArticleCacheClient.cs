using DataAccess.Models;

namespace ArticleService.Service.Clients;

public interface IArticleCacheClient
{
    Task<List<Article>?> TryGetRegionArticlesAsync(string region, CancellationToken cancellationToken = default);
    Task InvalidateRegionAsync(string region, CancellationToken cancellationToken = default);
}

