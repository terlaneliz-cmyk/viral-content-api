namespace ViralContentApi.DTOs
{
    public class AiPlanUpdateResponse
    {
        public int UserId { get; set; }
        public string Plan { get; set; } = string.Empty;
        public int DailyLimit { get; set; }
    }
}