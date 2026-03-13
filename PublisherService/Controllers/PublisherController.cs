using DataAccess.Models;
using Microsoft.AspNetCore.Mvc;

namespace PublisherService.Controllers;

[ApiController]
[Route("[controller]")]
public class PublisherController : ControllerBase
{
    [HttpPost]
    [Route("/publishArticle")]
    public async Task<IActionResult> PublishArticle([FromBody] Article article)
    {
        // Call mqtt for publishing articles
        return Ok();
    }
}