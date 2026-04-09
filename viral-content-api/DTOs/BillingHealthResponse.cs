namespace ViralContentApi.DTOs;

public class BillingHealthResponse
{
    public string Provider { get; set; } = string.Empty;
    public bool IsHealthy { get; set; }
    public string Message { get; set; } = string.Empty;
    public StripeBillingSettingsHealthResponse Stripe { get; set; } = new();
    public StripeProviderReadinessResponse StripeProviderReadiness { get; set; } = new();
}