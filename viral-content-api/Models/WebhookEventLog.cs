namespace ViralContentApi.Models;

public class WebhookEventLog
{
    public int Id { get; set; }

    public string Provider { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string ExternalEventId { get; set; } = string.Empty;

    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    public int? UserId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string SubscriptionId { get; set; } = string.Empty;
    public string CheckoutSessionId { get; set; } = string.Empty;
    public string SubscriptionStatus { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public string BillingCycle { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}