using Microsoft.AspNetCore.Mvc;
using Services.Dtos.ResponseDtos;
using Services.Interfaces;

namespace HappyHeadlines.Controllers;

[ApiController]
[Route("api/article")]
public class ArticleController(IArticleService articleService) : ControllerBase
{
    [HttpGet]
    public async Task<List<ArticleResponseDto>> GetArticles()
    {
        var response = await articleService.GetArticles();
        return response;
    }
}