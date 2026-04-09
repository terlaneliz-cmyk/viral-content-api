using ViralContentApi.DTOs;

namespace ViralContentApi.Services;

public class StubBillingProviderService : IBillingProviderService
{
    private readonly ILogger<StubBillingProviderService> _logger;

    public StubBillingProviderService(ILogger<StubBillingProviderService> logger)
    {
        _logger = logger;
    }

    public Task<CreateCheckoutSessionResponse> CreateCheckoutSessionAsync(BillingCheckoutSessionRequest request)
    {
        _logger.LogInformation(
            "Stub checkout session created. UserId: {UserId}, Plan: {PlanName}, BillingCycle: {BillingCycle}",
            request.UserId,
            request.PlanName,
            request.BillingCycle);

        return Task.FromResult(new CreateCheckoutSessionResponse
        {
            Provider = "Stub",
            SessionId = Guid.NewGuid().ToString("N"),
            CheckoutUrl = "https://example.com/stub-checkout",
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(30),
            Message = "Stub checkout session created successfully."
        });
    }

    public Task<BillingWebhookResponse> ProcessWebhookAsync(BillingWebhookRequest request)
    {
        _logger.LogInformation("Stub billing webhook processed.");

        return Task.FromResult(new BillingWebhookResponse
        {
            Success = true,
            EventType = "stub_webhook",
            Message = "Stub webhook processed successfully."
        });
    }
}