namespace ViralContentApi.DTOs
{
    public class ProcessSubscriptionExpirationsResponse
    {
        public int ProcessedCount { get; set; }
        public int CancelledCount { get; set; }
        public int ExpiredCount { get; set; }
        public List<int> UserIds { get; set; } = new();
        public DateTime ProcessedAtUtc { get; set; }
    }
}