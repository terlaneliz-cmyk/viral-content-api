using System.Text.RegularExpressions;
using ViralContentApi.Services.Models;

namespace ViralContentApi.Services
{
    public class AiEngagementTextBuilder
    {
        public string BuildCallToAction(
            int variantNumber,
            string platform,
            string goal,
            string contentType,
            string targetAudience,
            AiStrategyProfile strategyProfile)
        {
            if (goal == "leads")
            {
                return Rotate(variantNumber,
                    "If you want help applying this, send me a message.",
                    "If this matches what you're working on, reach out and let's talk.",
                    "If you want a practical version for your situation, message me.");
            }

            if (goal == "followers")
            {
                return Rotate(variantNumber,
                    "Follow for more practical content like this.",
                    "Follow if you want more sharp ideas you can actually use.",
                    "Follow for more useful breakdowns like this.");
            }

            if (goal == "authority")
            {
                return Rotate(variantNumber,
                    "What would you add to this?",
                    "What is your take on this approach?",
                    "Curious how you think about this.");
            }

            if (platform == "instagram")
            {
                return Rotate(variantNumber,
                    "Save this for later and share it with someone who needs it.",
                    "Save this if you want to come back to it later.",
                    "Share this with someone working on the same thing.");
            }

            if (platform == "x" || platform == "twitter")
            {
                return Rotate(variantNumber,
                    "Reply with your take.",
                    "What would you add?",
                    "Agree or disagree?");
            }

            if (platform == "youtube")
            {
                return Rotate(variantNumber,
                    "Subscribe for more content like this.",
                    "Let me know in the comments if you want more examples.",
                    "Comment if you want a part two.");
            }

            return Rotate(variantNumber,
                "What do you think?",
                "Would you try this approach?",
                "Which part stands out most to you?");
        }

        public List<string> BuildHashtags(string topic, string platform, AiTopicProfile topicProfile)
        {
            var tags = new List<string>
            {
                ToHashtag(topic),
                ToHashtag(topicProfile.Family),
                ToHashtag(platform),
                "#ContentCreation",
                "#Growth"
            };

            return tags
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(5)
                .ToList();
        }

        private string ToHashtag(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var cleaned = Regex.Replace(value, @"[^a-zA-Z0-9\s\-]", "");
            cleaned = cleaned.Replace("-", " ");

            cleaned = string.Concat(
                cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                       .Select(CapitalizeFirst));

            if (string.IsNullOrWhiteSpace(cleaned))
            {
                return string.Empty;
            }

            return $"#{cleaned}";
        }

        private string CapitalizeFirst(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            if (value.Length == 1)
            {
                return value.ToUpperInvariant();
            }

            return char.ToUpperInvariant(value[0]) + value[1..].ToLowerInvariant();
        }

        private string Rotate(int variantNumber, params string[] options)
        {
            if (options == null || options.Length == 0)
            {
                return string.Empty;
            }

            var index = (variantNumber - 1) % options.Length;
            return options[index];
        }
    }
}