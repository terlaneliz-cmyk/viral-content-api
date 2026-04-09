namespace ViralContentApi.DTOs;

public class BillingWebhookResponse
{
    public bool Success { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    public string? ExternalEventId { get; set; }
    public string? SubscriptionStatus { get; set; }
    public string? CustomerId { get; set; }
    public string? SubscriptionId { get; set; }
    public string? CheckoutSessionId { get; set; }
    public int? UserId { get; set; }
    public string? PlanName { get; set; }
    public string? BillingCycle { get; set; }
}