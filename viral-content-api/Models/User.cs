using System.ComponentModel.DataAnnotations;

namespace ViralContentApi.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Plan { get; set; } = "Free";

        [Required]
        [MaxLength(20)]
        public string Role { get; set; } = "User";

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<Post> Posts { get; set; } = new();
        public List<UserSubscription> UserSubscriptions { get; set; } = new();
        public List<AiUsageRecord> AiUsageRecords { get; set; } = new();
        public List<BillingEventLog> BillingEventLogs { get; set; } = new();
    }
}