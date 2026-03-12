using DraftService.Models;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[ApiController]
[Route("[controller]")]
public class DraftControllerWebApp (HttpClient httpClient) : ControllerBase
{
    [HttpGet]
    [Route("getDrafts")]
    public async Task<ActionResult<List<Draft>>> GetDrafts()
    {
        var drafts = await httpClient.GetFromJsonAsync<List<Draft>>("http://api-draft:5201/Draft/getDrafts");
        return Ok(drafts);
    }

    [HttpPost]
    [Route("addDraft")]
    public async Task<IActionResult> AddDraft([FromBody] Draft draft)
    {
        await httpClient.PostAsJsonAsync(
            "http://api-draft:5201/Draft/addDraft",
            draft
        );

        return Ok();
    }
}