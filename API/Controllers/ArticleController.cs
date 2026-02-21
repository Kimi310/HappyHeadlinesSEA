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

    [HttpPost]
    [Route("add")]
    public async Task<Article> AddArticle([FromBody] Article article)
    {
        var response = await articleService.AddArticle(article);
        return response;
    }
    
    [HttpDelete]
    [Route("delete/{region}/{id}")]
    public async Task DeleteArticle([FromRoute] string region, [FromRoute] Guid id)
    {
        await articleService.RemoveArticle(id,region);
    }
}