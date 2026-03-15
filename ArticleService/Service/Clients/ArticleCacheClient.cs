using System.Net;
using System.Net.Http.Json;
using DataAccess.Models;

namespace ArticleService.Service.Clients;

public sealed class ArticleCacheClient(HttpClient httpClient, ILogger<ArticleCacheClient> logger) : IArticleCacheClient
{
    public async Task<List<Article>?> TryGetRegionArticlesAsync(string region, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.GetAsync($"/api/cache/articles/{Uri.EscapeDataString(region)}", cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<Article>>(cancellationToken: cancellationToken);
            }

            logger.LogWarning("Article cache request failed with status {StatusCode} for region {Region}", response.StatusCode, region);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Article cache request failed for region {Region}", region);
            return null;
        }
    }

    public async Task InvalidateRegionAsync(string region, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.DeleteAsync($"/api/cache/region/{Uri.EscapeDataString(region)}", cancellationToken);
            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.NoContent)
            {
                logger.LogWarning("Article cache invalidation returned status {StatusCode} for region {Region}", response.StatusCode, region);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Article cache invalidation failed for region {Region}", region);
        }
    }
}

