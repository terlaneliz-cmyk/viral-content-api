namespace ViralContentApi.DTOs;

public class AdminOpsSummaryResponse
{
    public BillingHealthResponse Billing { get; set; } = new();
    public NotificationHealthResponse Notification { get; set; } = new();
    public List<ProcessedWebhookEventResponse> RecentWebhookEvents { get; set; } = new();
    public int RecentWebhookEventCount { get; set; }
    public int StripeWebhookEventCount { get; set; }
}