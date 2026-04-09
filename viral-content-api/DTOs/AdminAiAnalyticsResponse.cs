namespace ViralContentApi.DTOs
{
    public class AdminAiAnalyticsResponse
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TotalGenerations { get; set; }

        public List<AdminAiAnalyticsDailyItemResponse> DailyUsage { get; set; } = new();
        public List<AdminAiAnalyticsPlanItemResponse> UsageByPlan { get; set; } = new();
        public List<AdminTopAiUserResponse> TopUsers { get; set; } = new();
    }

    public class AdminAiAnalyticsDailyItemResponse
    {
        public DateTime Date { get; set; }
        public int UsedCount { get; set; }
    }

    public class AdminAiAnalyticsPlanItemResponse
    {
        public string Plan { get; set; } = "Free";
        public int UsedCount { get; set; }
    }
}