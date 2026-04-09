namespace ViralContentApi.DTOs;

public class StripeProviderReadinessResponse
{
    public bool SecretKeyConfigured { get; set; }
    public bool PublishableKeyConfigured { get; set; }
    public bool WebhookSecretConfigured { get; set; }
    public bool SuccessUrlConfigured { get; set; }
    public bool CancelUrlConfigured { get; set; }
    public bool BillingPortalReturnUrlConfigured { get; set; }
    public bool PriceIdFreeConfigured { get; set; }
    public bool PriceIdProMonthlyConfigured { get; set; }
    public bool PriceIdProYearlyConfigured { get; set; }
    public bool PriceIdAgencyMonthlyConfigured { get; set; }
    public bool PriceIdAgencyYearlyConfigured { get; set; }
}