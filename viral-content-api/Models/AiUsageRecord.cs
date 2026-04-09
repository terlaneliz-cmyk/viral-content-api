namespace ViralContentApi.Models
{
    public class AiUsageRecord
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        // Existing DbContext/Admin code expects these exact names
        public DateTime UsageDateUtc { get; set; }
        public int UsedCount { get; set; }

        public User? User { get; set; }
    }
}