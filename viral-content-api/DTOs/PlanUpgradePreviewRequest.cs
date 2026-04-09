namespace ViralContentApi.DTOs
{
    public class PlanUpgradePreviewRequest
    {
        public string TargetPlanName { get; set; } = string.Empty;
        public string BillingCycle { get; set; } = string.Empty;
    }
}