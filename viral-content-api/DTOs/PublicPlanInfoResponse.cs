namespace ViralContentApi.DTOs
{
    public class PublicPlanInfoResponse
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int DailyGenerateLimit { get; set; }
        public decimal MonthlyPrice { get; set; }
        public decimal YearlyPrice { get; set; }
        public int YearlyDiscountPercent { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string StripePriceLookupKeyMonthly { get; set; } = string.Empty;
        public string StripePriceLookupKeyYearly { get; set; } = string.Empty;
        public List<string> Features { get; set; } = new();
        public List<string> FeatureKeys { get; set; } = new();
        public bool IsRecommended { get; set; }
    }
}