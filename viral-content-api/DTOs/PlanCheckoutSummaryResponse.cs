namespace ViralContentApi.DTOs
{
    public class PlanCheckoutSummaryResponse
    {
        public string PlanName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string BillingCycle { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string StripePriceLookupKey { get; set; } = string.Empty;
        public int DailyGenerateLimit { get; set; }
        public decimal MonthlyPrice { get; set; }
        public decimal YearlyPrice { get; set; }
        public int YearlyDiscountPercent { get; set; }
        public decimal YearlySavingsAmount { get; set; }
        public bool IsRecommended { get; set; }
        public string Description { get; set; } = string.Empty;
        public List<string> Features { get; set; } = new();
    }
}