using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ViralContentApi.Services;

namespace ViralContentApi.Controllers;

[ApiController]
[Route("api/Admin/ops")]
[Authorize(Roles = "Admin")]
public class AdminOpsController : ControllerBase
{
    private readonly IAdminOpsService _adminOpsService;

    public AdminOpsController(IAdminOpsService adminOpsService)
    {
        _adminOpsService = adminOpsService;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary([FromQuery] int recentWebhookTake = 20)
    {
        var result = await _adminOpsService.GetSummaryAsync(recentWebhookTake);
        return Ok(result);
    }
}