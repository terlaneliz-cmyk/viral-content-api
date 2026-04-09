using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using ViralContentApi.DTOs;
using ViralContentApi.Services;

namespace ViralContentApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BillingWebhookController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IUserSubscriptionService _userSubscriptionService;
        private readonly ILogger<BillingWebhookController> _logger;

        public BillingWebhookController(
            IConfiguration configuration,
            IUserSubscriptionService userSubscriptionService,
            ILogger<BillingWebhookController> logger)
        {
            _configuration = configuration;
            _userSubscriptionService = userSubscriptionService;
            _logger = logger;
        }

        [HttpPost]
        [HttpPost("stripe")]
        public async Task<IActionResult> Receive()
        {
            var stripeWebhookSecret = _configuration["StripeBilling:WebhookSecret"];
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

                switch (stripeEvent.Type)
                {
                    case "checkout.session.completed":
                        await HandleCheckoutSessionCompletedAsync(stripeEvent);
                        break;

                    case "customer.subscription.updated":
                        await HandleCustomerSubscriptionUpdatedAsync(stripeEvent);
                        break;

                    case "customer.subscription.deleted":
                        await HandleCustomerSubscriptionDeletedAsync(stripeEvent);
                        break;

                    case "customer.subscription.created":
                        await HandleCustomerSubscriptionCreatedAsync(stripeEvent);
                        break;

                    case "invoice.paid":
                    case "invoice.payment_succeeded":
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
                    Message = $"Stripe webhook processed: {stripeEvent.Type}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing Stripe webhook.");

                return BadRequest(new BillingWebhookResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        private async Task HandleCheckoutSessionCompletedAsync(Event stripeEvent)
        {
            var session = stripeEvent.Data.Object as Session;
            if (session == null)
            {
                _logger.LogWarning("checkout.session.completed had null session object.");
                return;
            }

            var userId = GetUserId(session.Metadata);
            var planName = GetMetadataValue(session.Metadata, "planName");
            var billingCycle = GetMetadataValue(session.Metadata, "billingCycle");

            if (userId <= 0)
            {
                _logger.LogWarning("checkout.session.completed missing valid userId metadata.");
                return;
            }

            var status = string.Equals(session.PaymentStatus, "paid", StringComparison.OrdinalIgnoreCase)
                ? "active"
                : "pending";

            await _userSubscriptionService.SyncSubscriptionDetailsAsync(
                userId,
                status,
                session.SubscriptionId,
                session.CustomerId,
                planName,
                billingCycle,
                null,
                null,
                false,
                null,
                session.Id,
                null,
                null,
                null);
        }

        private async Task HandleCustomerSubscriptionCreatedAsync(Event stripeEvent)
        {
            var subscription = stripeEvent.Data.Object as Subscription;
            if (subscription == null)
            {
                _logger.LogWarning("customer.subscription.created had null subscription object.");
                return;
            }

            var userId = GetUserId(subscription.Metadata);
            var planName = GetMetadataValue(subscription.Metadata, "planName");
            var billingCycle = GetMetadataValue(subscription.Metadata, "billingCycle");

            if (userId <= 0)
            {
                _logger.LogWarning("customer.subscription.created missing valid userId metadata.");
                return;
            }

            await _userSubscriptionService.SyncSubscriptionDetailsAsync(
                userId,
                subscription.Status ?? "active",
                subscription.Id,
                subscription.CustomerId,
                planName,
                billingCycle,
                null,
                null,
                subscription.CancelAtPeriodEnd,
                null,
                null,
                null,
                null,
                null);
        }

        private async Task HandleCustomerSubscriptionUpdatedAsync(Event stripeEvent)
        {
            var subscription = stripeEvent.Data.Object as Subscription;
            if (subscription == null)
            {
                _logger.LogWarning("customer.subscription.updated had null subscription object.");
                return;
            }

            var userId = GetUserId(subscription.Metadata);
            var planName = GetMetadataValue(subscription.Metadata, "planName");
            var billingCycle = GetMetadataValue(subscription.Metadata, "billingCycle");

            if (userId <= 0)
            {
                _logger.LogWarning("customer.subscription.updated missing valid userId metadata.");
                return;
            }

            await _userSubscriptionService.SyncSubscriptionDetailsAsync(
                userId,
                subscription.Status ?? "unknown",
                subscription.Id,
                subscription.CustomerId,
                planName,
                billingCycle,
                null,
                null,
                subscription.CancelAtPeriodEnd,
                null,
                null,
                null,
                null,
                null);
        }

        private async Task HandleCustomerSubscriptionDeletedAsync(Event stripeEvent)
        {
            var subscription = stripeEvent.Data.Object as Subscription;
            if (subscription == null)
            {
                _logger.LogWarning("customer.subscription.deleted had null subscription object.");
                return;
            }

            var userId = GetUserId(subscription.Metadata);
            var planName = GetMetadataValue(subscription.Metadata, "planName");
            var billingCycle = GetMetadataValue(subscription.Metadata, "billingCycle");

            if (userId <= 0)
            {
                _logger.LogWarning("customer.subscription.deleted missing valid userId metadata.");
                return;
            }

            await _userSubscriptionService.SyncSubscriptionDetailsAsync(
                userId,
                "canceled",
                subscription.Id,
                subscription.CustomerId,
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

        private static int GetUserId(IDictionary<string, string>? metadata)
        {
            if (metadata == null)
            {
                return 0;
            }

            if (!metadata.TryGetValue("userId", out var rawUserId))
            {
                return 0;
            }

            return int.TryParse(rawUserId, out var userId) ? userId : 0;
        }

        private static string? GetMetadataValue(IDictionary<string, string>? metadata, string key)
        {
            if (metadata == null)
            {
                return null;
            }

            return metadata.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
                ? value
                : null;
        }
    }
}