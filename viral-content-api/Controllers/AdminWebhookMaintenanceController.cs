using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ViralContentApi.Services;

namespace ViralContentApi.Controllers;

[ApiController]
[Route("api/Admin/webhook-maintenance")]
[Authorize(Roles = "Admin")]
public class AdminWebhookMaintenanceController : ControllerBase
{
    private readonly IWebhookMaintenanceService _webhookMaintenanceService;

    public AdminWebhookMaintenanceController(IWebhookMaintenanceService webhookMaintenanceService)
    {
        _webhookMaintenanceService = webhookMaintenanceService;
    }

    [HttpPost("run")]
    public async Task<IActionResult> Run()
    {
        var result = await _webhookMaintenanceService.RunCleanupAsync();
        return Ok(result);
    }
}