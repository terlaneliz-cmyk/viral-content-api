using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ViralContentApi.DTOs;
using ViralContentApi.Services;

namespace ViralContentApi.Controllers;

[ApiController]
[Route("api/Admin/notifications")]
[Authorize(Roles = "Admin")]
public class AdminNotificationsController : ControllerBase
{
    private readonly BillingNotificationOrchestrator _billingNotificationOrchestrator;
    private readonly INotificationTemplateService _notificationTemplateService;

    public AdminNotificationsController(
        BillingNotificationOrchestrator billingNotificationOrchestrator,
        INotificationTemplateService notificationTemplateService)
    {
        _billingNotificationOrchestrator = billingNotificationOrchestrator;
        _notificationTemplateService = notificationTemplateService;
    }

    [HttpPost("test-email")]
    public async Task<IActionResult> SendTestEmail([FromBody] SendTestBillingEmailRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ToEmail))
        {
            return BadRequest(new { message = "ToEmail is required." });
        }

        var result = await _billingNotificationOrchestrator.SendTestAsync(
            request.ToEmail,
            request.ToName,
            request.Subject,
            request.Message);

        if (!result.Success)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, result);
        }

        return Ok(result);
    }

    [HttpPost("preview-template")]
    public IActionResult PreviewTemplate([FromBody] AdminNotificationTemplatePreviewRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.TemplateType))
        {
            return BadRequest(new { message = "TemplateType is required." });
        }

        if (string.IsNullOrWhiteSpace(request.ToEmail))
        {
            return BadRequest(new { message = "ToEmail is required." });
        }

        var email = _notificationTemplateService.BuildByType(
            request.TemplateType,
            request.ToEmail,
            request.ToName,
            request.Plan,
            request.OldPlan,
            request.NewPlan,
            request.EffectiveDateUtc,
            request.Subject,
            request.Message);

        return Ok(new AdminNotificationTemplatePreviewResponse
        {
            TemplateType = request.TemplateType,
            ToEmail = email.ToEmail,
            ToName = email.ToName,
            Subject = email.Subject,
            HtmlContent = email.HtmlContent,
            PlainTextContent = email.PlainTextContent ?? string.Empty,
            EventType = email.EventType
        });
    }

    [HttpPost("send-template")]
    public async Task<IActionResult> SendTemplate([FromBody] AdminNotificationTemplatePreviewRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.TemplateType))
        {
            return BadRequest(new { message = "TemplateType is required." });
        }

        if (string.IsNullOrWhiteSpace(request.ToEmail))
        {
            return BadRequest(new { message = "ToEmail is required." });
        }

        var email = _notificationTemplateService.BuildByType(
            request.TemplateType,
            request.ToEmail,
            request.ToName,
            request.Plan,
            request.OldPlan,
            request.NewPlan,
            request.EffectiveDateUtc,
            request.Subject,
            request.Message);

        var result = await _billingNotificationOrchestrator.SendAsync(email);

        if (!result.Success)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, result);
        }

        return Ok(result);
    }
}