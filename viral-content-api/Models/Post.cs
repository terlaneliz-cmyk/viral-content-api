using System;

namespace ViralContentApi.Models
{
    public class Post
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;

        public int Views { get; set; }
        public int Likes { get; set; }
        public int Shares { get; set; }

        public double ViralScore { get; set; }
        public double ScoreBoost { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        // AI fields
        public string? Topic { get; set; }
        public string? Platform { get; set; }
        public string? Hook { get; set; }
        public string? Content { get; set; }
        public string? CallToAction { get; set; }
        public string? Hashtags { get; set; }
        public bool IsAiGenerated { get; set; } = false;

        // Stored AI generation settings
        public string? Tone { get; set; }
        public string? Goal { get; set; }
        public string? ContentType { get; set; }
        public string? TargetAudience { get; set; }
        public int? NumberOfVariants { get; set; }
    }
}