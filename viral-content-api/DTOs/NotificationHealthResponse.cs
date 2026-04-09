namespace ViralContentApi.DTOs;

public class NotificationHealthResponse
{
    public string Provider { get; set; } = string.Empty;
    public bool IsHealthy { get; set; }
    public string Message { get; set; } = string.Empty;
    public SendGridNotificationHealthResponse SendGrid { get; set; } = new();
}