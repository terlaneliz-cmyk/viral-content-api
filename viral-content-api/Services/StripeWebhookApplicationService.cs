using Stripe;
using Stripe.Checkout;
using ViralContentApi.DTOs;

namespace ViralContentApi.Services;

public class StripeWebhookApplicationService : IStripeWebhookApplicationService
{
    private readonly IUserSubscriptionService _userSubscriptionService;
    private readonly IBillingEventLogService _billingEventLogService;
    private readonly ILogger<StripeWebhookApplicationService> _logger;

    public StripeWebhookApplicationService(
        IUserSubscriptionService userSubscriptionService,
        IBillingEventLogService billingEventLogService,
        ILogger<StripeWebhookApplicationService> logger)
    {
        _userSubscriptionService = userSubscriptionService;
        _billingEventLogService = billingEventLogService;
        _logger = logger;
    }

    public async Task ApplyAsync(BillingWebhookResponse webhookResponse)
    {
        if (webhookResponse == null)
        {
            throw new ArgumentNullException(nameof(webhookResponse));
        }

        await LogAsync(
            "billing.webhook.response",
            true,
            "Billing webhook response received.",
            null);
    }

    public async Task ApplyAsync(Event stripeEvent)
    {
        if (stripeEvent == null)
        {
            throw new ArgumentNullException(nameof(stripeEvent));
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

            case "invoice.paid":
                await HandleInvoicePaidAsync(stripeEvent);
                break;

            case "invoice.payment_failed":
                await HandleInvoicePaymentFailedAsync(stripeEvent);
                break;

            default:
                _logger.LogInformation("Stripe event type {EventType} is not handled.", stripeEvent.Type);
                break;
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
            await LogAsync(
                stripeEvent.Type,
                false,
                "checkout.session.completed missing valid userId metadata.",
                null);

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
            null);

        await LogAsync(
            stripeEvent.Type,
            true,
            $"checkout.session.completed processed for user {userId}.",
            userId);
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
            await LogAsync(
                stripeEvent.Type,
                false,
                "customer.subscription.updated missing valid userId metadata.",
                null);

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
            null);

        await LogAsync(
            stripeEvent.Type,
            true,
            $"customer.subscription.updated processed for user {userId}.",
            userId);
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
            await LogAsync(
                stripeEvent.Type,
                false,
                "customer.subscription.deleted missing valid userId metadata.",
                null);

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
            DateTime.UtcNow);

        await LogAsync(
            stripeEvent.Type,
            true,
            $"customer.subscription.deleted processed for user {userId}.",
            userId);
    }

    private async Task HandleInvoicePaidAsync(Event stripeEvent)
    {
        await LogAsync(
            stripeEvent.Type,
            true,
            "invoice.paid received.",
            null);
    }

    private async Task HandleInvoicePaymentFailedAsync(Event stripeEvent)
    {
        await LogAsync(
            stripeEvent.Type,
            false,
            "invoice.payment_failed received.",
            null);
    }

    private async Task LogAsync(string eventType, bool success, string message, int? userId)
    {
        try
        {
            await _billingEventLogService.LogAsync(
                userId,
                "Stripe",
                eventType,
                null,
                message,
                success,
                "system");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log Stripe billing event.");
        }
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