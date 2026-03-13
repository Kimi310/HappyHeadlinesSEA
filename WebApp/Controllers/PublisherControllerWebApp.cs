using DataAccess.Models;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;


[ApiController]
[Route("[controller]")]
public class PublisherControllerWebApp : ControllerBase
{
    private readonly HttpClient _httpClient;

    public PublisherControllerWebApp(IHttpClientFactory factory)
    {
        _httpClient = factory.CreateClient("publisherservice");
    }
    
    [HttpPost]
    [Route("addArticle")]
    public async Task<IActionResult> AddArticle([FromBody] Article article)
    {
        await _httpClient.PostAsJsonAsync("/Publisher/publishArticle",article);
        return Ok();
    }
    
}