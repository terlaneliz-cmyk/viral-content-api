namespace ViralContentApi.DTOs
{
    public class BillingEventLogResponse
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string ExternalCheckoutSessionId { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }
    }
}