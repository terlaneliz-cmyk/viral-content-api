namespace ViralContentApi.DTOs
{
    public class SyncSubscriptionStatusRequest
    {
        public int UserId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ExternalCheckoutSessionId { get; set; }
        public DateTime? CurrentPeriodStartUtc { get; set; }
        public DateTime? CurrentPeriodEndUtc { get; set; }
    }
}