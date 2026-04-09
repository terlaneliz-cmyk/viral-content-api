using ViralContentApi.Services.Models;

namespace ViralContentApi.Services
{
    public class AiStrategyProfileBuilder
    {
        public AiStrategyProfile Build(string platform, string goal, string contentType)
        {
            var normalizedPlatform = Normalize(platform);
            var normalizedGoal = Normalize(goal);
            var normalizedContentType = Normalize(contentType);

            var profile = new AiStrategyProfile
            {
                PrimaryAngle = "clarity",
                BodyEmphasis = "practical value",
                HookStyle = "direct",
                CtaMode = "conversation",
                UsesCuriosity = false,
                UsesCredibility = false,
                UsesConversionIntent = false,
                UsesEmotionalRelatability = false
            };

            ApplyPlatform(profile, normalizedPlatform);
            ApplyGoal(profile, normalizedGoal);
            ApplyContentType(profile, normalizedContentType);

            return profile;
        }

        private static void ApplyPlatform(AiStrategyProfile profile, string platform)
        {
            switch (platform)
            {
                case "linkedin":
                    profile.PrimaryAngle = "insight";
                    profile.BodyEmphasis = "professional value";
                    profile.HookStyle = "authority";
                    profile.CtaMode = "discussion";
                    profile.UsesCredibility = true;
                    break;

                case "youtube":
                    profile.PrimaryAngle = "retention";
                    profile.BodyEmphasis = "watchability";
                    profile.HookStyle = "curiosity";
                    profile.CtaMode = "subscription";
                    profile.UsesCuriosity = true;
                    break;

                case "tiktok":
                    profile.PrimaryAngle = "relatability";
                    profile.BodyEmphasis = "pace and punch";
                    profile.HookStyle = "pattern interrupt";
                    profile.CtaMode = "engagement";
                    profile.UsesCuriosity = true;
                    profile.UsesEmotionalRelatability = true;
                    break;

                case "x":
                case "twitter":
                    profile.PrimaryAngle = "sharp opinion";
                    profile.BodyEmphasis = "brevity";
                    profile.HookStyle = "punchy";
                    profile.CtaMode = "reply";
                    break;

                case "instagram":
                    profile.PrimaryAngle = "clarity";
                    profile.BodyEmphasis = "caption rhythm";
                    profile.HookStyle = "scroll stop";
                    profile.CtaMode = "save/share";
                    profile.UsesEmotionalRelatability = true;
                    break;

                case "facebook":
                    profile.PrimaryAngle = "conversation";
                    profile.BodyEmphasis = "accessibility";
                    profile.HookStyle = "relatable";
                    profile.CtaMode = "comment";
                    profile.UsesEmotionalRelatability = true;
                    break;
            }
        }

        private static void ApplyGoal(AiStrategyProfile profile, string goal)
        {
            switch (goal)
            {
                case "engagement":
                    profile.CtaMode = "engagement";
                    profile.UsesCuriosity = true;
                    profile.UsesEmotionalRelatability = true;
                    break;

                case "followers":
                    profile.CtaMode = "follow";
                    profile.UsesCuriosity = true;
                    break;

                case "leads":
                    profile.PrimaryAngle = "conversion";
                    profile.BodyEmphasis = "specific business value";
                    profile.CtaMode = "lead";
                    profile.UsesConversionIntent = true;
                    profile.UsesCredibility = true;
                    break;

                case "authority":
                    profile.PrimaryAngle = "credibility";
                    profile.BodyEmphasis = "expert insight";
                    profile.HookStyle = "authority";
                    profile.CtaMode = "trust";
                    profile.UsesCredibility = true;
                    break;
            }
        }

        private static void ApplyContentType(AiStrategyProfile profile, string contentType)
        {
            switch (contentType)
            {
                case "caption":
                    profile.BodyEmphasis = "tight readability";
                    break;

                case "script":
                    profile.BodyEmphasis = "spoken delivery";
                    profile.UsesCuriosity = true;
                    break;

                case "thread":
                    profile.BodyEmphasis = "structured progression";
                    break;

                case "video idea":
                    profile.PrimaryAngle = "concept strength";
                    profile.BodyEmphasis = "angle and why it works";
                    profile.UsesCuriosity = true;
                    break;
            }
        }

        private static string Normalize(string? value)
        {
            return value?.Trim().ToLowerInvariant() ?? string.Empty;
        }
    }
}