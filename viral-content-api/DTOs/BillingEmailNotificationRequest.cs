namespace ViralContentApi.DTOs;

public class BillingEmailNotificationRequest
{
    public string ToEmail { get; set; } = string.Empty;
    public string? ToName { get; set; }

    public string Subject { get; set; } = string.Empty;

    public string HtmlContent { get; set; } = string.Empty;
    public string? PlainTextContent { get; set; }

    public string EventType { get; set; } = string.Empty;
    public int? UserId { get; set; }

    public Dictionary<string, string>? Metadata { get; set; }
}