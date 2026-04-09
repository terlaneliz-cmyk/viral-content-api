using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ViralContentApi.Services;

namespace ViralContentApi.Controllers;

[ApiController]
[Route("api/Admin/webhook-events")]
[Authorize(Roles = "Admin")]
public class AdminWebhookEventsController : ControllerBase
{
    private readonly IProcessedWebhookEventService _processedWebhookEventService;

    public AdminWebhookEventsController(IProcessedWebhookEventService processedWebhookEventService)
    {
        _processedWebhookEventService = processedWebhookEventService;
    }

    [HttpGet]
    public async Task<IActionResult> GetRecent([FromQuery] string? provider = null, [FromQuery] int take = 50)
    {
        var result = await _processedWebhookEventService.GetRecentAsync(provider, take);
        return Ok(result);
    }

    [HttpDelete]
    public async Task<IActionResult> Clear([FromQuery] string? provider = null, [FromQuery] int? olderThanDays = null)
    {
        var result = await _processedWebhookEventService.ClearAsync(provider, olderThanDays);
        return Ok(result);
    }
}