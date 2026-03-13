using DataAccess.Models;
using Microsoft.AspNetCore.Mvc;
using PublisherService.Messaging;

namespace PublisherService.Controllers;

[ApiController]
[Route("[controller]")]
public class PublisherController(IArticleEventPublisher articleEventPublisher) : ControllerBase
{
    [HttpPost]
    [Route("/publishArticle")]
    public async Task<IActionResult> PublishArticle([FromBody] Article article)
    {
        if (string.IsNullOrWhiteSpace(article.Title) || string.IsNullOrWhiteSpace(article.Content))
        {
            return BadRequest("Title and Content are required.");
        }

        if (!article.IsGlobal && string.IsNullOrWhiteSpace(article.Continent))
        {
            return BadRequest("Continent is required for non-global articles.");
        }

        if (article.Id == Guid.Empty)
        {
            article.Id = Guid.NewGuid();
        }

        await articleEventPublisher.PublishArticleAsync(article, HttpContext.RequestAborted);
        return Ok();
    }
}