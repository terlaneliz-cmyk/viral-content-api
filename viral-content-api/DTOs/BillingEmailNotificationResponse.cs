namespace ViralContentApi.DTOs;

public class BillingEmailNotificationResponse
{
    public bool Success { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int? StatusCode { get; set; }
    public string? ProviderMessageId { get; set; }
}