

using Microsoft.AspNetCore.Http.HttpResults;
using SubscriberService.DataAccess.Models;
using SubscriberService.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;


namespace SubscriberService.Controllers;


[ApiController]
[Route("api/[controller]")]
public class SubscriberController : ControllerBase
{
    private readonly ISubscriberService _subscriberService;

    public SubscriberController(ISubscriberService subscriberService)
    {
        _subscriberService = subscriberService;
    }

    [HttpPost("subscriber/create")]
    public async Task<IActionResult> CreateSubscriber([FromBody] Subscriber request)
    {
        try
        {
            var subscriber = await _subscriberService.CreateSubscriberAsync(request.Email);
            return Ok(subscriber);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("subscriber/{subscriberId}")]
    public async Task<IActionResult> RemoveSubscriber(Guid subscriberId)
    {
        await _subscriberService.RemoveSubscriberAsync(subscriberId);
        return NoContent();
    }
}