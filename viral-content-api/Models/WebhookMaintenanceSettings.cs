namespace ViralContentApi.Models;

public class WebhookMaintenanceSettings
{
    public bool Enabled { get; set; } = true;
    public int RunIntervalMinutes { get; set; } = 60;
    public int ProcessedWebhookEventsRetentionDays { get; set; } = 30;
    public int WebhookEventLogsRetentionDays { get; set; } = 30;
}