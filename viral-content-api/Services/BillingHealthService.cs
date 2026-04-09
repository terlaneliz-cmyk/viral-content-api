using Microsoft.Extensions.Options;
using ViralContentApi.DTOs;
using ViralContentApi.Models;

namespace ViralContentApi.Services;

public class BillingHealthService : IBillingHealthService
{
    private readonly IConfiguration _configuration;
    private readonly StripeBillingSettings _stripeSettings;
    private readonly StripeUrlsSettings _stripeUrlsSettings;

    public BillingHealthService(
        IConfiguration configuration,
        IOptions<StripeBillingSettings> stripeSettings,
        IOptions<StripeUrlsSettings> stripeUrlsSettings)
    {
        _configuration = configuration;
        _stripeSettings = stripeSettings.Value;
        _stripeUrlsSettings = stripeUrlsSettings.Value;
    }

    public Task<BillingHealthResponse> GetHealthAsync()
    {
        var selectedProvider = _configuration["BillingProvider:Provider"] ?? "Stub";

        var stripeSettingsHealth = new StripeBillingSettingsHealthResponse
        {
            SecretKeyConfigured = !string.IsNullOrWhiteSpace(_stripeSettings.SecretKey),
            PublishableKeyConfigured = !string.IsNullOrWhiteSpace(_stripeSettings.PublishableKey),
            WebhookSecretConfigured = !string.IsNullOrWhiteSpace(_stripeSettings.WebhookSecret)
        };

        var stripeProviderReadiness = new StripeProviderReadinessResponse
        {
            SecretKeyConfigured = !string.IsNullOrWhiteSpace(_stripeSettings.SecretKey),
            PublishableKeyConfigured = !string.IsNullOrWhiteSpace(_stripeSettings.PublishableKey),
            WebhookSecretConfigured = !string.IsNullOrWhiteSpace(_stripeSettings.WebhookSecret),
            SuccessUrlConfigured = !string.IsNullOrWhiteSpace(_stripeUrlsSettings.SuccessUrl),
            CancelUrlConfigured = !string.IsNullOrWhiteSpace(_stripeUrlsSettings.CancelUrl),
            BillingPortalReturnUrlConfigured = !string.IsNullOrWhiteSpace(_stripeUrlsSettings.BillingPortalReturnUrl),
            PriceIdFreeConfigured = !string.IsNullOrWhiteSpace(_stripeSettings.FreePriceId),
            PriceIdProMonthlyConfigured = !string.IsNullOrWhiteSpace(_stripeSettings.ProMonthlyPriceId),
            PriceIdProYearlyConfigured = !string.IsNullOrWhiteSpace(_stripeSettings.ProYearlyPriceId),
            PriceIdAgencyMonthlyConfigured = !string.IsNullOrWhiteSpace(_stripeSettings.AgencyMonthlyPriceId),
            PriceIdAgencyYearlyConfigured = !string.IsNullOrWhiteSpace(_stripeSettings.AgencyYearlyPriceId)
        };

        var response = new BillingHealthResponse
        {
            Provider = selectedProvider,
            IsHealthy = true,
            Message = "Billing provider health evaluated.",
            Stripe = stripeSettingsHealth,
            StripeProviderReadiness = stripeProviderReadiness
        };

        if (selectedProvider.Equals("Stripe", StringComparison.OrdinalIgnoreCase))
        {
            response.IsHealthy =
                stripeProviderReadiness.SecretKeyConfigured &&
                stripeProviderReadiness.PublishableKeyConfigured &&
                stripeProviderReadiness.WebhookSecretConfigured &&
                stripeProviderReadiness.SuccessUrlConfigured &&
                stripeProviderReadiness.CancelUrlConfigured &&
                stripeProviderReadiness.BillingPortalReturnUrlConfigured &&
                stripeProviderReadiness.PriceIdProMonthlyConfigured &&
                stripeProviderReadiness.PriceIdProYearlyConfigured &&
                stripeProviderReadiness.PriceIdAgencyMonthlyConfigured &&
                stripeProviderReadiness.PriceIdAgencyYearlyConfigured;

            response.Message = response.IsHealthy
                ? "Stripe configuration looks ready."
                : "Stripe provider selected but required settings are missing.";
        }

        return Task.FromResult(response);
    }

    public Task<BillingHealthResponse> GetHealth()
    {
        return GetHealthAsync();
    }
}