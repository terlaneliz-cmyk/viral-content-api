namespace ViralContentApi.DTOs
{
    public class UserSubscriptionResponse
    {
        public int UserId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public string BillingCycle { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string StripePriceLookupKey { get; set; } = string.Empty;
        public string ExternalCheckoutSessionId { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? ActivatedAtUtc { get; set; }
        public DateTime? CurrentPeriodStartUtc { get; set; }
        public DateTime? CurrentPeriodEndUtc { get; set; }
        public bool CancelAtPeriodEnd { get; set; }
        public DateTime? CancelRequestedAtUtc { get; set; }
    }
}