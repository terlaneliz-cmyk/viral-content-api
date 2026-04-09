using System.Collections.Generic;

namespace ViralContentApi.DTOs
{
    public class SaveGeneratedContentAsPostRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;

        public string Topic { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;

        public string Hook { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string CallToAction { get; set; } = string.Empty;
        public List<string> Hashtags { get; set; } = new();

        public int Views { get; set; }
        public int Likes { get; set; }
        public int Shares { get; set; }
        public double ScoreBoost { get; set; }

        // Stored AI generation settings
        public string Tone { get; set; } = "confident";
        public string Goal { get; set; } = "engagement";
        public string ContentType { get; set; } = "post";
        public string TargetAudience { get; set; } = "general audience";
        public int NumberOfVariants { get; set; } = 3;
    }
}