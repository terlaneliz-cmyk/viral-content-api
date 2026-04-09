namespace ViralContentApi.DTOs;

public class WebhookEventLogStatsResponse
{
    public string? Provider { get; set; }
    public int TotalCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public int StripeCount { get; set; }
    public int Recent24HoursCount { get; set; }
}