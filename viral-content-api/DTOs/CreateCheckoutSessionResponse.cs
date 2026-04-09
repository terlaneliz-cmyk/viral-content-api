namespace ViralContentApi.DTOs;

public class CreateCheckoutSessionResponse
{
    public string Provider { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string CheckoutUrl { get; set; } = string.Empty;
    public DateTime? ExpiresAtUtc { get; set; }
    public string Message { get; set; } = string.Empty;

    // Backward-compatible field expected by PlansService
    public UserSubscriptionResponse? Subscription { get; set; }
}