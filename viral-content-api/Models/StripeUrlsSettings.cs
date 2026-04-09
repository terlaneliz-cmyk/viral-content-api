namespace ViralContentApi.Models;

public class StripeUrlsSettings
{
    public string SuccessUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
    public string BillingPortalReturnUrl { get; set; } = string.Empty;
}