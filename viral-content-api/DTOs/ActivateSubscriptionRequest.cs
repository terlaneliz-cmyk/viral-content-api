namespace ViralContentApi.DTOs
{
    public class ActivateSubscriptionRequest
    {
        public int UserId { get; set; }
        public string? ExternalCheckoutSessionId { get; set; }
    }
}