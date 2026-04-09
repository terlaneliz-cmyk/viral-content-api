using ViralContentApi.DTOs;

namespace ViralContentApi.Services;

public interface IStripeWebhookApplicationService
{
    Task ApplyAsync(BillingWebhookResponse webhook);
}