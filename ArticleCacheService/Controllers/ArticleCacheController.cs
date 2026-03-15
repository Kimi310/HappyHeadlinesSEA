using ArticleCacheService.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ArticleCacheService.Controllers;

[ApiController]
[Route("api/cache")]
public class ArticleCacheController(IArticleCacheService articleCacheService) : ControllerBase
{
    [HttpGet("articles/{region}")]
    public async Task<IActionResult> GetFromRegion([FromRoute] string region, CancellationToken cancellationToken)
    {
        var result = await articleCacheService.GetRegionArticlesAsync(region, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("region/{region}")]
    public async Task<IActionResult> InvalidateRegion([FromRoute] string region, CancellationToken cancellationToken)
    {
        await articleCacheService.InvalidateRegionAsync(region, cancellationToken);
        return NoContent();
    }

    [HttpPost("warmup")]
    public async Task<IActionResult> WarmUp(CancellationToken cancellationToken)
    {
        await articleCacheService.WarmRecentArticlesAsync(cancellationToken);
        return Ok();
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats(CancellationToken cancellationToken)
    {
        var stats = await articleCacheService.GetStatsAsync(cancellationToken);
        return Ok(stats);
    }
}

