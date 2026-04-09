namespace ViralContentApi.DTOs
{
    public class MyPlanResponse
    {
        public string PlanName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int DailyGenerateLimit { get; set; }
        public int UsedToday { get; set; }
        public int RemainingToday { get; set; }
        public bool IsLimitReached { get; set; }
        public decimal MonthlyPrice { get; set; }
        public decimal YearlyPrice { get; set; }
        public int YearlyDiscountPercent { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Features { get; set; } = new();
        public List<string> AvailableUpgrades { get; set; } = new();
    }
}