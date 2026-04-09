namespace ViralContentApi.DTOs;

public class WebhookMaintenanceRunResponse
{
    public int DeletedProcessedWebhookEventsCount { get; set; }
    public int DeletedWebhookEventLogsCount { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime ExecutedAtUtc { get; set; } = DateTime.UtcNow;
}