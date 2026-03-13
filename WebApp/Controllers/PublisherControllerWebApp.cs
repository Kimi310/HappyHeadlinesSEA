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
        var response = await _httpClient.PostAsJsonAsync("/publishArticle", article);
        if (response.IsSuccessStatusCode)
        {
            return Ok();
        }

        var body = await response.Content.ReadAsStringAsync();
        return StatusCode((int)response.StatusCode, body);
    }
    
}