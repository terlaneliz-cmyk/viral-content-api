namespace ViralContentApi.Models;

public class WebhookEventLog
{
    public int Id { get; set; }

    public string Provider { get; set; } = string.Empty;

    public string EventId { get; set; } = string.Empty;

    public string? ExternalEventId { get; set; }

    public string EventType { get; set; } = string.Empty;

    public string Payload { get; set; } = string.Empty;

    public bool Processed { get; set; }

    public bool Success { get; set; }

    public string? Message { get; set; }

    public int? UserId { get; set; }

    public string? CustomerId { get; set; }

    public string? SubscriptionId { get; set; }

    public string? CheckoutSessionId { get; set; }

    public string? SubscriptionStatus { get; set; }

    public string? PlanName { get; set; }

    public string? BillingCycle { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}