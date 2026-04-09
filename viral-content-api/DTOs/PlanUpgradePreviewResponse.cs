namespace ViralContentApi.DTOs
{
    public class PlanUpgradePreviewResponse
    {
        public string CurrentPlanName { get; set; } = string.Empty;
        public string TargetPlanName { get; set; } = string.Empty;
        public string TargetDisplayName { get; set; } = string.Empty;
        public string BillingCycle { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string StripePriceLookupKey { get; set; } = string.Empty;
        public int CurrentDailyGenerateLimit { get; set; }
        public int TargetDailyGenerateLimit { get; set; }
        public bool IsUpgrade { get; set; }
        public bool IsSamePlan { get; set; }
        public bool IsDowngrade { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Features { get; set; } = new();
    }
}