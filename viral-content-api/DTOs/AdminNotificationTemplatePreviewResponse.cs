namespace ViralContentApi.DTOs;

public class AdminNotificationTemplatePreviewResponse
{
    public string TemplateType { get; set; } = string.Empty;
    public string ToEmail { get; set; } = string.Empty;
    public string? ToName { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string HtmlContent { get; set; } = string.Empty;
    public string PlainTextContent { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
}