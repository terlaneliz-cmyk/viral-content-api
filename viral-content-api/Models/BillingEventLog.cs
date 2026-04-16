namespace ViralContentApi.Models;

public class BillingEventLog
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public string EventType { get; set; } = string.Empty;

    public string? Status { get; set; }

    public string? ExternalCheckoutSessionId { get; set; }

    public string? PayloadJson { get; set; }

    public bool Success { get; set; }

    public string? Message { get; set; }

    public string? Metadata { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}