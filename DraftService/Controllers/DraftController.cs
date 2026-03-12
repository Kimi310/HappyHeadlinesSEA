using DraftService.Models;
using DraftService.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DraftService.Controllers;

[ApiController]
[Route("[controller]")]
public class DraftController(IDraftService draftService, ILogger<DraftController> logger) : ControllerBase
{

    [HttpGet]
    [Route("getDrafts")]
    public async Task<ActionResult<List<Draft>>> GetDrafts()
    {
        logger.LogInformation("GET /draft/getDrafts request received");

        try
        {
            var drafts = await draftService.GetDrafts();

            logger.LogInformation("Returning {DraftCount} drafts", drafts.Count);

            return Ok(drafts);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while fetching drafts");

            return StatusCode(500, "An error occurred while retrieving drafts.");
        }
    }

    [HttpPost]
    [Route("addDraft")]
    public async Task<IActionResult> AddDraft([FromBody] Draft draft)
    {
        logger.LogInformation("POST /draft/addDraft request received for draft with title {DraftTitle}", draft.Title);

        if (draft == null)
        {
            logger.LogWarning("Attempted to add null draft");
            return BadRequest("Draft cannot be null");
        }

        try
        {
            await draftService.AddDraft(draft);

            logger.LogInformation("Draft successfully created with title {DraftTitle}", draft.Title);

            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while adding draft with title {DraftTitle}", draft.Title);

            return StatusCode(500, "An error occurred while creating the draft.");
        }
    }
}