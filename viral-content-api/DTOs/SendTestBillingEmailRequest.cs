namespace ViralContentApi.DTOs;

public class SendTestBillingEmailRequest
{
    public string ToEmail { get; set; } = string.Empty;
    public string? ToName { get; set; }
    public string? Subject { get; set; }
    public string? Message { get; set; }
}