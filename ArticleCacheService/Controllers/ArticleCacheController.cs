using ArticleCacheService.Service;
using ArticleCacheService.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ArticleCacheService.Controllers;

[ApiController]
[Route("api/cache")]
public class ArticleCacheController(IArticleCacheService articleCacheService, CacheRuntimeState runtimeState) : ControllerBase
{
    [HttpGet("articles/{region}")]
    public async Task<IActionResult> GetFromRegion([FromRoute] string region, CancellationToken cancellationToken)
    {
        var result = await articleCacheService.GetRegionArticlesAsync(region, cancellationToken);
        return Ok(result);
    }

    [HttpPost("mode")]
    public IActionResult SetMode([FromQuery] bool enabled)
    {
        runtimeState.Enabled = enabled;
        return Ok(new { cacheEnabled = runtimeState.Enabled });
    }

    [HttpPost("stats/reset")]
    public async Task<IActionResult> ResetStats(CancellationToken cancellationToken)
    {
        await articleCacheService.ResetStatsAsync(cancellationToken);
        return Ok(new { reset = true });
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

