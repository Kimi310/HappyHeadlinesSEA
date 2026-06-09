using ArticleCacheService.DataAccess.Models;
using ArticleCacheService.Service.Telemetry;

namespace ArticleCacheService.Service.Interfaces;

public interface IArticleCacheService
{
    Task<IReadOnlyList<Article>> GetRegionArticlesAsync(string region, CancellationToken cancellationToken = default);
    Task InvalidateRegionAsync(string region, CancellationToken cancellationToken = default);
    Task WarmRecentArticlesAsync(CancellationToken cancellationToken = default);
    Task<ArticleCacheStatsSnapshot> GetStatsAsync(CancellationToken cancellationToken = default);
    Task ResetStatsAsync(CancellationToken cancellationToken = default);
}

