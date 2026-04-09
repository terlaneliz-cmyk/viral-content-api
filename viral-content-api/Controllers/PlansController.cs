using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ViralContentApi.DTOs;
using ViralContentApi.Services;

namespace ViralContentApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlansController : ControllerBase
    {
        private readonly IPlansService _plansService;
        private readonly IUserSubscriptionService _userSubscriptionService;

        public PlansController(IPlansService plansService, IUserSubscriptionService userSubscriptionService)
        {
            _plansService = plansService;
            _userSubscriptionService = userSubscriptionService;
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult<IEnumerable<PublicPlanInfoResponse>> GetPlans()
        {
            var plans = _plansService.GetPlans();
            return Ok(plans);
        }

        [HttpGet("comparison")]
        [AllowAnonymous]
        public ActionResult<PlanComparisonResponse> GetComparison()
        {
            var comparison = _plansService.GetComparison();
            return Ok(comparison);
        }

        [HttpGet("my-plan")]
        [Authorize]
        public async Task<ActionResult<MyPlanResponse>> GetMyPlan()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Invalid user token." });
            }

            var result = await _plansService.GetMyPlanAsync(userId);

            if (result == null)
            {
                return NotFound(new { message = "User plan was not found." });
            }

            return Ok(result);
        }

        [HttpGet("my-subscription")]
        [Authorize]
        public async Task<ActionResult<UserSubscriptionResponse>> GetMySubscription()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Invalid user token." });
            }

            var result = await _userSubscriptionService.GetMySubscriptionAsync(userId);

            if (result == null)
            {
                return NotFound(new { message = "User subscription was not found." });
            }

            return Ok(result);
        }

        [HttpPost("my-subscription/cancel")]
        [Authorize]
        public async Task<ActionResult<UserSubscriptionResponse>> CancelMySubscription()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Invalid user token." });
            }

            var result = await _userSubscriptionService.RequestCancelAtPeriodEndAsync(userId);

            if (result == null)
            {
                return NotFound(new { message = "User subscription was not found." });
            }

            return Ok(result);
        }

        [HttpPost("dev/process-expirations")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ProcessSubscriptionExpirationsResponse>> ProcessExpirations()
        {
            var result = await _userSubscriptionService.ProcessExpiredSubscriptionsAsync();
            return Ok(result);
        }

        [HttpPost("dev/activate-subscription")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserSubscriptionResponse>> ActivateSubscription([FromBody] ActivateSubscriptionRequest request)
        {
            if (request == null || request.UserId <= 0)
            {
                return BadRequest(new { message = "Valid UserId is required." });
            }

            if (string.IsNullOrWhiteSpace(request.ExternalCheckoutSessionId))
            {
                return BadRequest(new { message = "ExternalCheckoutSessionId is required." });
            }

            var result = await _userSubscriptionService.ActivateSubscriptionAsync(
                request.UserId,
                request.ExternalCheckoutSessionId);

            if (result == null)
            {
                return NotFound(new { message = "Matching subscription was not found." });
            }

            return Ok(result);
        }

        [HttpPost("dev/cancel-subscription")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserSubscriptionResponse>> CancelSubscriptionAsAdmin([FromBody] CancelSubscriptionRequest request)
        {
            if (request == null || request.UserId == null || request.UserId <= 0)
            {
                return BadRequest(new { message = "Valid UserId is required." });
            }

            var result = await _userSubscriptionService.CancelSubscriptionImmediatelyAsync(request.UserId.Value);

            if (result == null)
            {
                return NotFound(new { message = "User subscription was not found." });
            }

            return Ok(result);
        }

        [HttpPost("dev/sync-subscription-status")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserSubscriptionResponse>> SyncSubscriptionStatus([FromBody] SyncSubscriptionStatusRequest request)
        {
            if (request == null || request.UserId <= 0 || string.IsNullOrWhiteSpace(request.Status))
            {
                return BadRequest(new { message = "Valid UserId and Status are required." });
            }

            var result = await _userSubscriptionService.SyncSubscriptionStatusAsync(
                request.UserId,
                request.Status,
                request.ExternalCheckoutSessionId,
                request.CurrentPeriodStartUtc,
                request.CurrentPeriodEndUtc);

            if (result == null)
            {
                return NotFound(new { message = "Matching subscription was not found." });
            }

            return Ok(result);
        }

        [HttpPost("upgrade-preview")]
        [Authorize]
        public async Task<ActionResult<PlanUpgradePreviewResponse>> GetUpgradePreview([FromBody] PlanUpgradePreviewRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Invalid user token." });
            }

            if (request == null || string.IsNullOrWhiteSpace(request.TargetPlanName))
            {
                return BadRequest(new { message = "TargetPlanName is required." });
            }

            var result = await _plansService.GetUpgradePreviewAsync(userId, request);

            if (result == null)
            {
                return NotFound(new { message = $"Plan '{request.TargetPlanName}' was not found." });
            }

            if (result.IsSamePlan || result.IsDowngrade)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("create-checkout-session")]
        [Authorize]
        public async Task<ActionResult<CreateCheckoutSessionResponse>> CreateCheckoutSession([FromBody] CreateCheckoutSessionRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Invalid user token." });
            }

            if (request == null || string.IsNullOrWhiteSpace(request.TargetPlanName))
            {
                return BadRequest(new { message = "TargetPlanName is required." });
            }

            try
            {
                var result = await _plansService.CreateCheckoutSessionAsync(userId, request);

                if (result == null)
                {
                    return BadRequest(new { message = "Checkout session could not be created for this plan change." });
                }

                return Ok(result);
            }
            catch (BillingProviderNotReadyException ex)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    message = ex.Message
                });
            }
        }

        [HttpGet("{name}")]
        [AllowAnonymous]
        public ActionResult<PublicPlanInfoResponse> GetPlanByName(string name)
        {
            var plan = _plansService.GetPlanByName(name);

            if (plan == null)
            {
                return NotFound(new { message = $"Plan '{name}' was not found." });
            }

            return Ok(plan);
        }

        [HttpGet("{name}/checkout-summary")]
        [AllowAnonymous]
        public ActionResult<PlanCheckoutSummaryResponse> GetCheckoutSummary(
            string name,
            [FromQuery] string? billingCycle)
        {
            var summary = _plansService.GetCheckoutSummary(name, billingCycle);

            if (summary == null)
            {
                return NotFound(new { message = $"Plan '{name}' was not found." });
            }

            return Ok(summary);
        }
    }
}