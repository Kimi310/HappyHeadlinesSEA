using DraftService.Models;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[ApiController]
[Route("[controller]")]
public class DraftControllerWebApp : ControllerBase
{
    private readonly HttpClient _httpClient;

    public DraftControllerWebApp(IHttpClientFactory factory)
    {
        _httpClient = factory.CreateClient("draftservice");
    }
    
    [HttpGet]
    [Route("getDrafts")]
    public async Task<ActionResult<List<Draft>>> GetDrafts()
    {
        var drafts = await _httpClient.GetFromJsonAsync<List<Draft>>("/Draft/getDrafts");
        return Ok(drafts);
    }

    [HttpPost]
    [Route("addDraft")]
    public async Task<IActionResult> AddDraft([FromBody] Draft draft)
    {
        await _httpClient.PostAsJsonAsync(
            "/Draft/addDraft",
            draft
        );

        return Ok();
    }
}