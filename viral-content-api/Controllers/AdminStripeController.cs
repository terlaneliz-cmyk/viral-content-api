using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ViralContentApi.DTOs;
using ViralContentApi.Services;

namespace ViralContentApi.Controllers;

[ApiController]
[Route("api/Admin/stripe")]
[Authorize(Roles = "Admin")]
public class AdminStripeController : ControllerBase
{
    private readonly IBillingProviderService _billingProviderService;
    private readonly IStripeWebhookApplicationService _stripeWebhookApplicationService;

    public AdminStripeController(
        IBillingProviderService billingProviderService,
        IStripeWebhookApplicationService stripeWebhookApplicationService)
    {
        _billingProviderService = billingProviderService;
        _stripeWebhookApplicationService = stripeWebhookApplicationService;
    }

    [HttpPost("simulate-webhook")]
    public async Task<IActionResult> SimulateWebhook([FromBody] AdminStripeWebhookSimulationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RawBody))
        {
            return BadRequest(new { message = "RawBody is required." });
        }

        var result = await _billingProviderService.ProcessWebhookAsync(new BillingWebhookRequest
        {
            RawBody = request.RawBody,
            SignatureHeader = request.SignatureHeader,
            SkipSignatureValidation = request.SkipSignatureValidation,
            Provider = "Stripe"
        });

        if (result.Success)
        {
            await _stripeWebhookApplicationService.ApplyAsync(result);
            return Ok(result);
        }

        return BadRequest(result);
    }
}