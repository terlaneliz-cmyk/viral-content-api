namespace ViralContentApi.Models;

public class StripeBillingSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string PublishableKey { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;

    public string FreePriceId { get; set; } = string.Empty;
    public string ProMonthlyPriceId { get; set; } = string.Empty;
    public string ProYearlyPriceId { get; set; } = string.Empty;
    public string AgencyMonthlyPriceId { get; set; } = string.Empty;
    public string AgencyYearlyPriceId { get; set; } = string.Empty;
}