namespace ViralContentApi.Models
{
    public class SubscriptionProcessingSettings
    {
        public bool Enabled { get; set; }
        public int IntervalMinutes { get; set; } = 60;
    }
}