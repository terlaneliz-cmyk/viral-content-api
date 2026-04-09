namespace ViralContentApi.Models;

public class ProcessedWebhookEvent
{
    public int Id { get; set; }

    public string Provider { get; set; } = string.Empty;

    public string ExternalEventId { get; set; } = string.Empty;

    public DateTime ProcessedAtUtc { get; set; } = DateTime.UtcNow;
}