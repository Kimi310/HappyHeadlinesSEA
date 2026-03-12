using System.Data;
using ProfanityService.Service.Interfaces;
using ProfanityService.Controllers.Models;

namespace ProfanityService.Controllers;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ProfanityController : ControllerBase
{
    private readonly IProfanityService _profanityService;

    public ProfanityController(IProfanityService profanityService)
    {
        this._profanityService = profanityService;
    }
    
    [HttpPost("check")]
    public async Task<IActionResult> CheckProfanity([FromBody] TextRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Text))
        {
            return BadRequest(new { error = "Text is required" });
        }

        var containsProfanity = await _profanityService.ContainsProfanityAsync(request.Text);
        
        return Ok(new { containsProfanity });
    }

    [HttpPost("filter")]
    public async Task<IActionResult> FilterProfanity([FromBody] FilterRequest request)
    {
        try
        {
            var filteredText = await _profanityService.FilterProfanityAsync(
                request.Text, 
                request.ReplacementChar ?? '*'
            );
            
            return Ok(new { filteredText });
        }
        catch (NoNullAllowedException)
        {
            return BadRequest(new { error = "Text is required" });
        }
    }
}

public class FilterRequest
{
    public string Text { get; set; }
    public char? ReplacementChar { get; set; }
}

/*
 How to use :
{
    "text": "This is a bad word",
    "replacementChar": "*"
}

Response:
{
    "filteredText": "This is a *** word"
}

*/