namespace ViralContentApi.DTOs
{
    public class AiUsageStatusResponse
    {
        public string Plan { get; set; } = string.Empty;
        public int DailyLimit { get; set; }
        public int UsedToday { get; set; }
        public int RemainingToday { get; set; }

        // Existing code expects this exact name
        public bool Allowed { get; set; }

        // Keep compatibility with any newer code too
        public bool CanUse { get; set; }
    }
}