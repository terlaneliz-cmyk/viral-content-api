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

        public DateTime? UpdatedAt { get; set; }

        public DateTime? DeactivatedAt { get; set; }

        public string? DeactivationReason { get; set; }

        public DateTime? LastLoginAt { get; set; }

        public DateTime? LastBillingActionAt { get; set; }

        [MaxLength(50)]
        public string? ReferralCode { get; set; }

        public int? ReferredByUserId { get; set; }

        public int ReferralInviteCount { get; set; }

        public int ReferralSignupCount { get; set; }

        public DateTime? ReferralCodeCreatedAtUtc { get; set; }

        public DateTime? ReferralTrialEndsAtUtc { get; set; }

        public int CurrentStreak { get; set; }

        public int BestStreak { get; set; }

        public DateTime? LastActiveDateUtc { get; set; }

        public List<Post> Posts { get; set; } = new();
        public List<UserSubscription> UserSubscriptions { get; set; } = new();
        public List<AiUsageRecord> AiUsageRecords { get; set; } = new();
        public List<BillingEventLog> BillingEventLogs { get; set; } = new();
        public List<GeneratedContent> GeneratedContents { get; set; } = new();
        public List<WebhookEventLog> WebhookEventLogs { get; set; } = new();
        public List<ProcessedWebhookEvent> ProcessedWebhookEvents { get; set; } = new();
    }
}