namespace ViralContentApi.DTOs
{
    public class CreateCheckoutSessionRequest
    {
        public string? TargetPlanName { get; set; }
        public string? PlanName { get; set; }
        public string? BillingCycle { get; set; }
        public string? SuccessUrl { get; set; }
        public string? CancelUrl { get; set; }
    }
}