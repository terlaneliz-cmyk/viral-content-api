using ViralContentApi.DTOs;

namespace ViralContentApi.Services;

public interface IBillingProviderService
{
    Task<CreateCheckoutSessionResponse> CreateCheckoutSessionAsync(BillingCheckoutSessionRequest request);
    Task<BillingWebhookResponse> ProcessWebhookAsync(BillingWebhookRequest request);
}