using DraftService.Models;
using DraftService.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DraftService.Controllers;

[ApiController]
[Route("[controller]")]
public class DraftController(IDraftService draftService) : ControllerBase
{
    [HttpGet]
    [Route("getDrafts")]
    public async Task<List<Draft>> GetDrafts()
    {
        return await draftService.GetDrafts();
    }

    [HttpPost]
    [Route("addDraft")]
    public async Task<IActionResult> AddDraft([FromBody] Draft draft)
    {
        await draftService.AddDraft(draft);
        return Ok();
    }
}