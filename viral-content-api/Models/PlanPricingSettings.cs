namespace ViralContentApi.Models
{
    public class PlanPricingSettings
    {
        public decimal MonthlyPrice { get; set; }
        public decimal YearlyPrice { get; set; }
        public int YearlyDiscountPercent { get; set; }
        public string Currency { get; set; } = "USD";
        public string Description { get; set; } = string.Empty;
        public bool IsRecommended { get; set; }
        public string StripePriceLookupKeyMonthly { get; set; } = string.Empty;
        public string StripePriceLookupKeyYearly { get; set; } = string.Empty;
        public List<string> Features { get; set; } = new();
        public List<string> FeatureKeys { get; set; } = new();
    }
}