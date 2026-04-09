namespace ViralContentApi.DTOs;

public class AdminNotificationTemplatePreviewRequest
{
    public string TemplateType { get; set; } = string.Empty;
    public string ToEmail { get; set; } = "test@example.com";
    public string? ToName { get; set; } = "Test User";
    public string? Plan { get; set; }
    public string? OldPlan { get; set; }
    public string? NewPlan { get; set; }
    public DateTime? EffectiveDateUtc { get; set; }
    public string? Subject { get; set; }
    public string? Message { get; set; }
}