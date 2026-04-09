using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ViralContentApi.Services;

namespace ViralContentApi.Controllers;

[ApiController]
[Route("api/Admin/webhook-logs")]
[Authorize(Roles = "Admin")]
public class AdminWebhookLogsController : ControllerBase
{
    private readonly IWebhookEventLogService _webhookEventLogService;

    public AdminWebhookLogsController(IWebhookEventLogService webhookEventLogService)
    {
        _webhookEventLogService = webhookEventLogService;
    }

    [HttpGet]
    public async Task<IActionResult> GetRecent([FromQuery] string? provider = null, [FromQuery] int take = 50)
    {
        var result = await _webhookEventLogService.GetRecentAsync(provider, take);
        return Ok(result);
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats([FromQuery] string? provider = null)
    {
        var result = await _webhookEventLogService.GetStatsAsync(provider);
        return Ok(result);
    }

    [HttpDelete]
    public async Task<IActionResult> Clear(
        [FromQuery] string? provider = null,
        [FromQuery] bool? success = null,
        [FromQuery] int? olderThanDays = null)
    {
        var result = await _webhookEventLogService.ClearAsync(provider, success, olderThanDays);
        return Ok(result);
    }
}