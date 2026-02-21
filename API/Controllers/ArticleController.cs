using DataAccess.Models;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace HappyHeadlines.Controllers;

[ApiController]
[Route("api/article")]
public class ArticleController(IArticleService articleService) : ControllerBase
{
    [HttpGet]
    [Route("get/{region}")]
    public async Task<List<Article>> GetArticlesFromRegion([FromRoute]  string region)
    {
        var response = await articleService.GetArticlesFromRegion(region);
        return response;
    }
}