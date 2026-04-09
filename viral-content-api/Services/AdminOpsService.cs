using ViralContentApi.DTOs;

namespace ViralContentApi.Services;

public class AdminOpsService : IAdminOpsService
{
    private readonly IBillingHealthService _billingHealthService;
    private readonly INotificationHealthService _notificationHealthService;
    private readonly IProcessedWebhookEventService _processedWebhookEventService;

    public AdminOpsService(
        IBillingHealthService billingHealthService,
        INotificationHealthService notificationHealthService,
        IProcessedWebhookEventService processedWebhookEventService)
    {
        _billingHealthService = billingHealthService;
        _notificationHealthService = notificationHealthService;
        _processedWebhookEventService = processedWebhookEventService;
    }

    public async Task<AdminOpsSummaryResponse> GetSummaryAsync(int recentWebhookTake = 20)
    {
        if (recentWebhookTake <= 0)
        {
            recentWebhookTake = 20;
        }

        if (recentWebhookTake > 100)
        {
            recentWebhookTake = 100;
        }

        var billing = await _billingHealthService.GetHealthAsync();
        var notification = await _notificationHealthService.GetHealthAsync();
        var recentWebhookEvents = await _processedWebhookEventService.GetRecentAsync(take: recentWebhookTake);
        var stripeWebhookEvents = await _processedWebhookEventService.GetRecentAsync("Stripe", recentWebhookTake);

        return new AdminOpsSummaryResponse
        {
            Billing = billing,
            Notification = notification,
            RecentWebhookEvents = recentWebhookEvents,
            RecentWebhookEventCount = recentWebhookEvents.Count,
            StripeWebhookEventCount = stripeWebhookEvents.Count
        };
    }
}