namespace ViralContentApi.DTOs;

public class BillingCheckoutSessionRequest
{
    public int UserId { get; set; }

    public string CustomerEmail { get; set; } = string.Empty;

    public string PlanName { get; set; } = string.Empty;

    public string BillingCycle { get; set; } = string.Empty;

    // Backward-compatible fields already used by PlansService
    public decimal Price { get; set; }

    public string Currency { get; set; } = "usd";

    public string StripePriceLookupKey { get; set; } = string.Empty;

    public string? SuccessUrl { get; set; }

    public string? CancelUrl { get; set; }
}