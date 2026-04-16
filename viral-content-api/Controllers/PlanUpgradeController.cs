using System.Net.Mail;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using ViralContentApi.Data;
using ViralContentApi.Models;
using ViralContentApi.Services;

namespace ViralContentApi.Controllers;

[ApiController]
[Route("api/Plans")]
[Authorize]
public class PlanUpgradeController : ControllerBase
{
    private static readonly HashSet<string> DisposableEmailDomains = new(StringComparer.OrdinalIgnoreCase)
    {
        "mailinator.com",
        "10minutemail.com",
        "tempmail.com",
        "guerrillamail.com",
        "yopmail.com",
        "trashmail.com",
        "throwawaymail.com",
        "dispostable.com",
        "fakeinbox.com",
        "sharklasers.com",
        "guerrillamailblock.com",
        "pokemail.net",
        "spam4.me",
        "bccto.me",
        "chacuo.net",
        "getnada.com",
        "mintemail.com",
        "mohmal.com",
        "emailondeck.com",
        "tmpmail.org"
    };

    private readonly IConfiguration _configuration;
    private readonly IUserSubscriptionService _userSubscriptionService;
    private readonly ILogger<PlanUpgradeController> _logger;
    private readonly AppDbContext _context;

    public PlanUpgradeController(
        IConfiguration configuration,
        IUserSubscriptionService userSubscriptionService,
        ILogger<PlanUpgradeController> logger,
        AppDbContext context)
    {
        _configuration = configuration;
        _userSubscriptionService = userSubscriptionService;
        _logger = logger;
        _context = context;
    }

    [HttpGet("subscription")]
    public async Task<IActionResult> GetSubscription()
    {
        var userId = GetUserId();
        if (userId <= 0)
        {
            return Unauthorized(new { message = "User id not found in token." });
        }

        var subscription = await _userSubscriptionService.GetMySubscriptionAsync(userId);
        return Ok(subscription);
    }

    [HttpPost("cancel")]
    public async Task<IActionResult> CancelAtPeriodEnd()
    {
        var userId = GetUserId();
        if (userId <= 0)
        {
            return Unauthorized(new { message = "User id not found in token." });
        }

        var subscription = await _userSubscriptionService.GetByUserIdAsync(userId);
        if (subscription == null)
        {
            return NotFound(new { message = "Subscription not found." });
        }

        if (string.Equals(subscription.PlanName, "Free", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "Free plan cannot be canceled." });
        }

        if (string.IsNullOrWhiteSpace(subscription.ExternalSubscriptionId))
        {
            return BadRequest(new { message = "Stripe subscription id is missing." });
        }

        if (subscription.CancelAtPeriodEnd)
        {
            return BadRequest(new { message = "Subscription is already set to cancel at period end." });
        }

        try
        {
            var stripeSubscriptionService = new Stripe.SubscriptionService();

            var updatedStripeSubscription = await stripeSubscriptionService.UpdateAsync(
                subscription.ExternalSubscriptionId,
                new SubscriptionUpdateOptions
                {
                    CancelAtPeriodEnd = true
                });

            _logger.LogInformation(
                "Stripe cancel_at_period_end enabled for user {UserId}. StripeCustomerId={StripeCustomerId}, StripeSubscriptionId={StripeSubscriptionId}",
                userId,
                updatedStripeSubscription.CustomerId,
                updatedStripeSubscription.Id);

            await _userSubscriptionService.SyncSubscriptionDetailsAsync(
                userId,
                updatedStripeSubscription.CancelAtPeriodEnd ? "canceling" : (updatedStripeSubscription.Status ?? "active"),
                updatedStripeSubscription.Id,
                updatedStripeSubscription.CustomerId,
                subscription.PlanName,
                subscription.BillingCycle,
                null,
                null,
                updatedStripeSubscription.CancelAtPeriodEnd,
                null,
                null,
                null,
                null,
                null);

            await AddBillingAuditLogAsync(
                userId,
                "cancel_requested",
                $"Plan={subscription.PlanName}; StripeSubscriptionId={updatedStripeSubscription.Id}; CancelAtPeriodEnd=true");

            var response = await _userSubscriptionService.GetMySubscriptionAsync(userId);
            return Ok(response);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe cancel at period end failed for user {UserId}", userId);

            await AddBillingAuditLogAsync(
                userId,
                "cancel_failed",
                ex.StripeError?.Message ?? ex.Message);

            return BadRequest(new { message = ex.StripeError?.Message ?? "Failed to cancel subscription at period end." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected cancel at period end failure for user {UserId}", userId);

            await AddBillingAuditLogAsync(
                userId,
                "cancel_failed",
                ex.Message);

            return BadRequest(new { message = "Failed to cancel subscription at period end." });
        }
    }

    [HttpPost("reactivate")]
    public async Task<IActionResult> Reactivate()
    {
        var userId = GetUserId();
        if (userId <= 0)
        {
            return Unauthorized(new { message = "User id not found in token." });
        }

        var subscription = await _userSubscriptionService.GetByUserIdAsync(userId);
        if (subscription == null)
        {
            return NotFound(new { message = "Subscription not found." });
        }

        if (string.Equals(subscription.PlanName, "Free", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "Free plan cannot be reactivated." });
        }

        if (string.IsNullOrWhiteSpace(subscription.ExternalSubscriptionId))
        {
            return BadRequest(new { message = "Stripe subscription id is missing." });
        }

        if (!subscription.CancelAtPeriodEnd)
        {
            return BadRequest(new { message = "Subscription is not scheduled for cancellation." });
        }

        try
        {
            var stripeSubscriptionService = new Stripe.SubscriptionService();

            var updatedStripeSubscription = await stripeSubscriptionService.UpdateAsync(
                subscription.ExternalSubscriptionId,
                new SubscriptionUpdateOptions
                {
                    CancelAtPeriodEnd = false
                });

            _logger.LogInformation(
                "Stripe cancel_at_period_end disabled for user {UserId}. StripeCustomerId={StripeCustomerId}, StripeSubscriptionId={StripeSubscriptionId}",
                userId,
                updatedStripeSubscription.CustomerId,
                updatedStripeSubscription.Id);

            await _userSubscriptionService.SyncSubscriptionDetailsAsync(
                userId,
                updatedStripeSubscription.Status ?? "active",
                updatedStripeSubscription.Id,
                updatedStripeSubscription.CustomerId,
                subscription.PlanName,
                subscription.BillingCycle,
                null,
                null,
                updatedStripeSubscription.CancelAtPeriodEnd,
                null,
                null,
                null,
                null,
                null);

            await AddBillingAuditLogAsync(
                userId,
                "reactivated",
                $"Plan={subscription.PlanName}; StripeSubscriptionId={updatedStripeSubscription.Id}; CancelAtPeriodEnd=false");

            var response = await _userSubscriptionService.GetMySubscriptionAsync(userId);
            return Ok(response);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe reactivation failed for user {UserId}", userId);

            await AddBillingAuditLogAsync(
                userId,
                "reactivate_failed",
                ex.StripeError?.Message ?? ex.Message);

            return BadRequest(new { message = ex.StripeError?.Message ?? "Failed to reactivate subscription." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected reactivation failure for user {UserId}", userId);

            await AddBillingAuditLogAsync(
                userId,
                "reactivate_failed",
                ex.Message);

            return BadRequest(new { message = "Failed to reactivate subscription." });
        }
    }


[HttpPost("billing-portal")]
public async Task<IActionResult> CreateBillingPortal()
{
    var userId = GetUserId();
    if (userId <= 0) return Unauthorized();

    var sub = await _userSubscriptionService.GetByUserIdAsync(userId);

    if (sub == null || string.IsNullOrWhiteSpace(sub.ExternalCustomerId))
    {
        return BadRequest(new { message = "No Stripe customer found." });
    }

    var frontendBaseUrl =
        _configuration["Frontend:BaseUrl"] ??
        _configuration["App:FrontendBaseUrl"] ??
        _configuration["ClientAppUrl"] ??
        _configuration["FrontendBaseUrl"];

    var options = new Stripe.BillingPortal.SessionCreateOptions
    {
        Customer = sub.ExternalCustomerId,
        ReturnUrl = frontendBaseUrl
    };

    var service = new Stripe.BillingPortal.SessionService();
    var session = await service.CreateAsync(options);

    return Ok(new { url = session.Url });
}


    [HttpPost("upgrade")]
    public async Task<IActionResult> Upgrade([FromBody] UpgradePlanRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.PlanName))
        {
            return BadRequest(new { message = "Plan name is required." });
        }

        var normalizedPlan = request.PlanName.Trim().ToLowerInvariant();

        if (normalizedPlan != "pro" && normalizedPlan != "creator")
        {
            return BadRequest(new { message = "Only Pro and Creator upgrades are supported." });
        }

        var userEmail =
            User.FindFirstValue(ClaimTypes.Email) ??
            User.FindFirstValue("email") ??
            User.FindFirstValue(ClaimTypes.Name);

        if (string.IsNullOrWhiteSpace(userEmail))
        {
            return Unauthorized(new { message = "User email not found in token." });
        }

        var trimmedEmail = userEmail.Trim();

        if (!IsValidEmail(trimmedEmail))
        {
            return BadRequest(new
            {
                message = "Your account email is invalid. Please use a valid email address before starting Stripe checkout."
            });
        }

        if (IsDisposableEmail(trimmedEmail))
        {
            return BadRequest(new
            {
                message = "Disposable or fake email addresses are not allowed for billing."
            });
        }

        var userId = GetUserId();
        if (userId <= 0)
        {
            return Unauthorized(new { message = "User id not found in token." });
        }

        var currentSubscription = await _userSubscriptionService.GetByUserIdAsync(userId);

        if (currentSubscription != null &&
            string.Equals(currentSubscription.PlanName, normalizedPlan, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(currentSubscription.Status, "canceled", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = $"{request.PlanName} is already your current plan." });
        }

        var frontendBaseUrl =
            _configuration["Frontend:BaseUrl"] ??
            _configuration["App:FrontendBaseUrl"] ??
            _configuration["ClientAppUrl"] ??
            _configuration["FrontendBaseUrl"];

        if (string.IsNullOrWhiteSpace(frontendBaseUrl))
        {
            return StatusCode(500, new { message = "Frontend base URL is not configured." });
        }

        frontendBaseUrl = frontendBaseUrl.TrimEnd('/');

        var proPriceId =
            _configuration["Stripe:PriceIds:Pro"] ??
            _configuration["Stripe:Prices:Pro"] ??
            _configuration["Stripe:ProPriceId"];

        var creatorPriceId =
            _configuration["Stripe:PriceIds:Creator"] ??
            _configuration["Stripe:Prices:Creator"] ??
            _configuration["Stripe:CreatorPriceId"];

        var selectedPriceId = normalizedPlan switch
        {
            "pro" => proPriceId,
            "creator" => creatorPriceId,
            _ => null
        };

        if (string.IsNullOrWhiteSpace(selectedPriceId))
        {
            return StatusCode(500, new { message = $"Stripe price ID is missing for {request.PlanName}." });
        }

        string? existingCustomerId = null;

        if (!string.IsNullOrWhiteSpace(currentSubscription?.ExternalCustomerId))
        {
            existingCustomerId = currentSubscription.ExternalCustomerId;
            _logger.LogInformation(
                "Reusing stored Stripe customer for user {UserId}. StripeCustomerId={StripeCustomerId}",
                userId,
                existingCustomerId);
        }
        else
        {
            existingCustomerId = await FindExistingStripeCustomerIdByEmailAsync(trimmedEmail);
        }

        try
        {
            var options = new SessionCreateOptions
            {
                Mode = "subscription",
                SuccessUrl = $"{frontendBaseUrl}/?checkout=success",
                CancelUrl = $"{frontendBaseUrl}/?checkout=cancel",
                AllowPromotionCodes = true,
                LineItems = new List<SessionLineItemOptions>
                {
                    new()
                    {
                        Price = selectedPriceId,
                        Quantity = 1
                    }
                },
                Metadata = new Dictionary<string, string>
                {
                    ["planName"] = normalizedPlan,
                    ["userEmail"] = trimmedEmail,
                    ["userId"] = userId.ToString()
                },
                SubscriptionData = new SessionSubscriptionDataOptions
                {
                    Metadata = new Dictionary<string, string>
                    {
                        ["planName"] = normalizedPlan,
                        ["userEmail"] = trimmedEmail,
                        ["userId"] = userId.ToString()
                    }
                }
            };

            if (!string.IsNullOrWhiteSpace(existingCustomerId))
            {
                options.Customer = existingCustomerId;
            }
            else
            {
                options.CustomerEmail = trimmedEmail;
            }

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            if (string.IsNullOrWhiteSpace(session.Url))
            {
                return StatusCode(500, new { message = "Stripe checkout URL was not returned." });
            }

            await _userSubscriptionService.CreateOrUpdatePendingSubscriptionAsync(
                userId,
                normalizedPlan,
                "monthly",
                price: 0,
                currency: "usd",
                stripePriceLookupKey: selectedPriceId,
                externalCheckoutSessionId: session.Id);

            if (!string.IsNullOrWhiteSpace(existingCustomerId))
            {
                await _userSubscriptionService.SyncSubscriptionDetailsAsync(
                    userId,
                    "pending",
                    currentSubscription?.ExternalSubscriptionId,
                    existingCustomerId,
                    normalizedPlan,
                    "monthly",
                    null,
                    null,
                    false,
                    null,
                    session.Id,
                    null,
                    null,
                    selectedPriceId);
            }

            await AddBillingAuditLogAsync(
                userId,
                "upgrade_started",
                $"Plan={normalizedPlan}; CheckoutSessionId={session.Id}; CustomerId={existingCustomerId ?? "new"}; PriceId={selectedPriceId}");

            return Ok(new UpgradePlanResponse
            {
                CheckoutUrl = session.Url
            });
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe checkout creation failed for user {UserId}", userId);

            await AddBillingAuditLogAsync(
                userId,
                "upgrade_failed",
                ex.StripeError?.Message ?? ex.Message);

            return BadRequest(new { message = ex.StripeError?.Message ?? "Failed to start Stripe checkout." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected Stripe checkout creation failure for user {UserId}", userId);

            await AddBillingAuditLogAsync(
                userId,
                "upgrade_failed",
                ex.Message);

            return BadRequest(new { message = "Failed to start Stripe checkout." });
        }
    }

    private int GetUserId()
    {
        var userIdRaw =
            User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            User.FindFirstValue("sub");

        return int.TryParse(userIdRaw, out var userId) ? userId : 0;
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var normalized = email.Trim();
            var addr = new MailAddress(normalized);
            return addr.Address.Equals(normalized, StringComparison.OrdinalIgnoreCase) && normalized.Contains('.');
        }
        catch
        {
            return false;
        }
    }

    private static bool IsDisposableEmail(string email)
    {
        var atIndex = email.LastIndexOf('@');
        if (atIndex < 0 || atIndex == email.Length - 1)
        {
            return true;
        }

        var domain = email[(atIndex + 1)..].Trim();
        if (string.IsNullOrWhiteSpace(domain))
        {
            return true;
        }

        return DisposableEmailDomains.Contains(domain);
    }

    private async Task<string?> FindExistingStripeCustomerIdByEmailAsync(string email)
    {
        try
        {
            var customerService = new CustomerService();
            var customers = await customerService.ListAsync(new CustomerListOptions
            {
                Email = email,
                Limit = 1
            });

            var customer = customers.Data.FirstOrDefault();

            if (customer != null)
            {
                _logger.LogInformation("Reusing Stripe customer {StripeCustomerId} for {Email}", customer.Id, email);
                return customer.Id;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to look up existing Stripe customer for {Email}", email);
            return null;
        }
    }

    private async Task AddBillingAuditLogAsync(int userId, string eventType, string? metadata)
    {
        try
        {
            _context.BillingEventLogs.Add(new BillingEventLog
            {
                UserId = userId,
                EventType = eventType,
                Metadata = metadata,
                CreatedAtUtc = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to write billing audit log for user {UserId} and event {EventType}", userId, eventType);
        }
    }

    public class UpgradePlanRequest
    {
        public string PlanName { get; set; } = "";
    }

    public class UpgradePlanResponse
    {
        public string CheckoutUrl { get; set; } = "";
    }
}