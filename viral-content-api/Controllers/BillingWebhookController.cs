using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using ViralContentApi.Data;
using ViralContentApi.DTOs;
using ViralContentApi.Services;

namespace ViralContentApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BillingWebhookController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _context;
    private readonly IUserSubscriptionService _userSubscriptionService;
    private readonly ILogger<BillingWebhookController> _logger;

    public BillingWebhookController(
        IConfiguration configuration,
        AppDbContext context,
        IUserSubscriptionService userSubscriptionService,
        ILogger<BillingWebhookController> logger)
    {
        _configuration = configuration;
        _context = context;
        _userSubscriptionService = userSubscriptionService;
        _logger = logger;
    }

    [HttpPost]
    [HttpPost("stripe")]
    public async Task<IActionResult> Receive()
    {
        var stripeWebhookSecret =
            _configuration["StripeBilling:WebhookSecret"] ??
            _configuration["Stripe:WebhookSecret"];

        var fallbackWebhookSecret = _configuration["BillingWebhook:Secret"];

        string requestBody;
        using (var reader = new StreamReader(Request.Body))
        {
            requestBody = await reader.ReadToEndAsync();
        }

        try
        {
            Event stripeEvent;

            var stripeSignature = Request.Headers["Stripe-Signature"].FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(stripeSignature) &&
                !string.IsNullOrWhiteSpace(stripeWebhookSecret))
            {
                stripeEvent = EventUtility.ConstructEvent(
                    requestBody,
                    stripeSignature,
                    stripeWebhookSecret);
            }
            else
            {
                var providedFallbackSecret = Request.Headers["X-Billing-Webhook-Secret"].FirstOrDefault();

                if (string.IsNullOrWhiteSpace(fallbackWebhookSecret) ||
                    providedFallbackSecret != fallbackWebhookSecret)
                {
                    return Unauthorized(new BillingWebhookResponse
                    {
                        Success = false,
                        Message = "Invalid webhook secret."
                    });
                }

                return Ok(new BillingWebhookResponse
                {
                    Success = true,
                    Message = "Fallback billing webhook received."
                });
            }

            using var json = JsonDocument.Parse(requestBody);

            switch (stripeEvent.Type)
            {
                case "checkout.session.completed":
                    await HandleCheckoutSessionCompletedAsync(json.RootElement);
                    break;

                case "customer.subscription.created":
                    await HandleCustomerSubscriptionCreatedAsync(json.RootElement);
                    break;

                case "customer.subscription.updated":
                    await HandleCustomerSubscriptionUpdatedAsync(json.RootElement);
                    break;

                case "customer.subscription.deleted":
                    await HandleCustomerSubscriptionDeletedAsync(json.RootElement);
                    break;

                case "invoice.paid":
                case "invoice.payment_succeeded":
                    await HandleInvoicePaymentSucceededAsync(json.RootElement);
                    break;

                case "charge.succeeded":
                case "payment_intent.succeeded":
                    _logger.LogInformation("Stripe event {EventType} received and ignored safely.", stripeEvent.Type);
                    break;

                default:
                    _logger.LogInformation("Unhandled Stripe event type: {EventType}", stripeEvent.Type);
                    break;
            }

            return Ok(new BillingWebhookResponse
            {
                Success = true,
                EventType = stripeEvent.Type,
                Message = $"Stripe webhook processed: {stripeEvent.Type}"
            });
        }
        catch (Exception ex)
        {
            var fullMessage = ex.InnerException == null
                ? ex.Message
                : $"{ex.Message} | Inner: {ex.InnerException.Message}";

            _logger.LogError(ex, "Error while processing Stripe webhook.");

            return BadRequest(new BillingWebhookResponse
            {
                Success = false,
                Message = fullMessage
            });
        }
    }

    private async Task HandleCheckoutSessionCompletedAsync(JsonElement root)
    {
        if (!TryGetNested(root, out var session, "data", "object"))
        {
            _logger.LogWarning("checkout.session.completed missing data.object.");
            return;
        }

        var userId = GetIntFromMetadata(session, "metadata", "userId");
        var rawPlanName = GetStringFromMetadata(session, "metadata", "planName");
        var planName = NormalizePlanName(rawPlanName);
        var billingCycle = GetStringFromMetadata(session, "metadata", "billingCycle");

        if (userId <= 0)
        {
            _logger.LogWarning("checkout.session.completed missing valid userId metadata.");
            return;
        }

        if (string.IsNullOrWhiteSpace(planName))
        {
            _logger.LogWarning("checkout.session.completed missing valid planName metadata.");
            return;
        }

        var paymentStatus = GetString(session, "payment_status");
        var status = string.Equals(paymentStatus, "paid", StringComparison.OrdinalIgnoreCase)
            ? "active"
            : "pending";

        var sessionId = GetString(session, "id");
        var subscriptionId = GetString(session, "subscription");
        var customerId = GetString(session, "customer");

        await _userSubscriptionService.SyncSubscriptionDetailsAsync(
            userId,
            status,
            subscriptionId,
            customerId,
            planName,
            billingCycle,
            null,
            null,
            false,
            null,
            sessionId,
            null,
            null,
            null);

        if (!string.Equals(status, "active", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (!string.Equals(planName, "Pro", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(planName, "Creator", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var existingSubscription = await _context.UserSubscriptions
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.UserId == userId &&
                x.ExternalSubscriptionId == (subscriptionId ?? string.Empty) &&
                x.ExternalCustomerId == (customerId ?? string.Empty) &&
                string.Equals(x.PlanName, planName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.Status, "active", StringComparison.OrdinalIgnoreCase));

        if (existingSubscription != null)
        {
            _logger.LogInformation(
                "User {UserId} already has matching active subscription {SubscriptionId}. Skipping duplicate activation.",
                userId,
                subscriptionId);
            return;
        }

        await _userSubscriptionService.ActivateSubscriptionAsync(
            userId,
            planName,
            string.IsNullOrWhiteSpace(billingCycle) ? "monthly" : billingCycle,
            subscriptionId,
            customerId,
            null,
            null);

        _logger.LogInformation(
            "Activated plan {PlanName} for user {UserId} from checkout.session.completed.",
            planName,
            userId);
    }

    private async Task HandleCustomerSubscriptionCreatedAsync(JsonElement root)
    {
        if (!TryGetNested(root, out var subscription, "data", "object"))
        {
            _logger.LogWarning("customer.subscription.created missing data.object.");
            return;
        }

        var userId = GetIntFromMetadata(subscription, "metadata", "userId");
        var planName = NormalizePlanName(GetStringFromMetadata(subscription, "metadata", "planName"));
        var billingCycle = GetStringFromMetadata(subscription, "metadata", "billingCycle");

        if (userId <= 0)
        {
            _logger.LogWarning("customer.subscription.created missing valid userId metadata.");
            return;
        }

        await _userSubscriptionService.SyncSubscriptionDetailsAsync(
            userId,
            GetString(subscription, "status") ?? "active",
            GetString(subscription, "id"),
            GetString(subscription, "customer"),
            planName,
            billingCycle,
            null,
            null,
            GetBool(subscription, "cancel_at_period_end"),
            null,
            null,
            null,
            null,
            null);
    }

    private async Task HandleCustomerSubscriptionUpdatedAsync(JsonElement root)
    {
        if (!TryGetNested(root, out var subscription, "data", "object"))
        {
            _logger.LogWarning("customer.subscription.updated missing data.object.");
            return;
        }

        var userId = GetIntFromMetadata(subscription, "metadata", "userId");
        var planName = NormalizePlanName(GetStringFromMetadata(subscription, "metadata", "planName"));
        var billingCycle = GetStringFromMetadata(subscription, "metadata", "billingCycle");

        if (userId <= 0)
        {
            _logger.LogWarning("customer.subscription.updated missing valid userId metadata.");
            return;
        }

        await _userSubscriptionService.SyncSubscriptionDetailsAsync(
            userId,
            GetString(subscription, "status") ?? "unknown",
            GetString(subscription, "id"),
            GetString(subscription, "customer"),
            planName,
            billingCycle,
            null,
            null,
            GetBool(subscription, "cancel_at_period_end"),
            null,
            null,
            null,
            null,
            null);
    }

    private async Task HandleCustomerSubscriptionDeletedAsync(JsonElement root)
    {
        if (!TryGetNested(root, out var subscription, "data", "object"))
        {
            _logger.LogWarning("customer.subscription.deleted missing data.object.");
            return;
        }

        var userId = GetIntFromMetadata(subscription, "metadata", "userId");
        var planName = NormalizePlanName(GetStringFromMetadata(subscription, "metadata", "planName"));
        var billingCycle = GetStringFromMetadata(subscription, "metadata", "billingCycle");

        if (userId <= 0)
        {
            _logger.LogWarning("customer.subscription.deleted missing valid userId metadata.");
            return;
        }

        await _userSubscriptionService.SyncSubscriptionDetailsAsync(
            userId,
            "canceled",
            GetString(subscription, "id"),
            GetString(subscription, "customer"),
            planName,
            billingCycle,
            null,
            null,
            false,
            DateTime.UtcNow,
            null,
            null,
            null,
            null);
    }

    private async Task HandleInvoicePaymentSucceededAsync(JsonElement root)
    {
        if (!TryGetNested(root, out var invoice, "data", "object"))
        {
            _logger.LogWarning("invoice.payment_succeeded missing data.object.");
            return;
        }

        var userId =
            GetIntFromPath(root, "data", "object", "parent", "subscription_details", "metadata", "userId") ??
            GetIntFromPath(root, "data", "object", "lines", "data", 0, "metadata", "userId") ??
            0;

        var planName =
            NormalizePlanName(
                GetStringFromPath(root, "data", "object", "parent", "subscription_details", "metadata", "planName") ??
                GetStringFromPath(root, "data", "object", "lines", "data", 0, "metadata", "planName"));

        var billingCycle =
            GetStringFromPath(root, "data", "object", "parent", "subscription_details", "metadata", "billingCycle") ??
            GetStringFromPath(root, "data", "object", "lines", "data", 0, "metadata", "billingCycle");

        if (userId <= 0)
        {
            _logger.LogWarning("invoice.payment_succeeded missing valid userId metadata.");
            return;
        }

        var subscriptionId =
            GetStringFromPath(root, "data", "object", "parent", "subscription_details", "subscription") ??
            GetStringFromPath(root, "data", "object", "lines", "data", 0, "parent", "subscription_item_details", "subscription");

        var customerId = GetString(invoice, "customer");

        await _userSubscriptionService.SyncSubscriptionDetailsAsync(
            userId,
            "active",
            subscriptionId,
            customerId,
            planName,
            billingCycle,
            null,
            null,
            false,
            null,
            null,
            null,
            null,
            null);
    }

    private static string? NormalizePlanName(string? planName)
    {
        if (string.IsNullOrWhiteSpace(planName))
        {
            return planName;
        }

        var normalized = planName.Trim().ToLowerInvariant();

        return normalized switch
        {
            "pro" => "Pro",
            "creator" => "Creator",
            "free" => "Free",
            "agency" => "Agency",
            _ => planName.Trim()
        };
    }

    private static string? GetString(JsonElement element, string propertyName)
    {
        if (element.ValueKind != JsonValueKind.Object ||
            !element.TryGetProperty(propertyName, out var value))
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Number => value.ToString(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => null
        };
    }

    private static bool GetBool(JsonElement element, string propertyName)
    {
        if (element.ValueKind != JsonValueKind.Object ||
            !element.TryGetProperty(propertyName, out var value))
        {
            return false;
        }

        return value.ValueKind == JsonValueKind.True ||
               (value.ValueKind == JsonValueKind.String &&
                bool.TryParse(value.GetString(), out var parsed) &&
                parsed);
    }

    private static string? GetStringFromMetadata(JsonElement parent, string metadataPropertyName, string key)
    {
        if (!parent.TryGetProperty(metadataPropertyName, out var metadata) ||
            metadata.ValueKind != JsonValueKind.Object ||
            !metadata.TryGetProperty(key, out var value))
        {
            return null;
        }

        return value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();
    }

    private static int GetIntFromMetadata(JsonElement parent, string metadataPropertyName, string key)
    {
        var raw = GetStringFromMetadata(parent, metadataPropertyName, key);
        return int.TryParse(raw, out var parsed) ? parsed : 0;
    }

    private static string? GetStringFromPath(JsonElement root, params object[] path)
    {
        if (!TryGetNested(root, out var value, path))
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Number => value.ToString(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => null
        };
    }

    private static int? GetIntFromPath(JsonElement root, params object[] path)
    {
        var raw = GetStringFromPath(root, path);
        return int.TryParse(raw, out var parsed) ? parsed : null;
    }

    private static bool TryGetNested(JsonElement element, out JsonElement value, params object[] path)
    {
        value = element;

        foreach (var segment in path)
        {
            if (segment is string propertyName)
            {
                if (value.ValueKind != JsonValueKind.Object ||
                    !value.TryGetProperty(propertyName, out value))
                {
                    value = default;
                    return false;
                }
            }
            else if (segment is int index)
            {
                if (value.ValueKind != JsonValueKind.Array ||
                    value.GetArrayLength() <= index)
                {
                    value = default;
                    return false;
                }

                value = value[index];
            }
            else
            {
                value = default;
                return false;
            }
        }

        return true;
    }
}