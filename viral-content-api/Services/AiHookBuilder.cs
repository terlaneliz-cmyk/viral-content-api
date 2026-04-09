using System.Text.RegularExpressions;
using ViralContentApi.Services.Models;

namespace ViralContentApi.Services
{
    public class AiHookBuilder
    {
        public string BuildHook(
            int styleIndex,
            int variantNumber,
            string topic,
            string platform,
            string tone,
            string goal,
            string contentType,
            string targetAudience,
            AiTopicProfile topicProfile,
            AiStrategyProfile strategyProfile)
        {
            var audienceSnippet = BuildAudienceHookSnippet(targetAudience, variantNumber);
            var platformSnippet = BuildPlatformHookSnippet(platform, variantNumber);
            var topicShort = RewriteTopicNaturally(topic, variantNumber);

            return styleIndex switch
            {
                1 => BuildContrarianHook(topicShort, topicProfile, strategyProfile, audienceSnippet, platformSnippet, variantNumber),
                2 => BuildAuthorityHook(topicShort, topicProfile, strategyProfile, audienceSnippet, platformSnippet, variantNumber),
                3 => BuildProblemSolutionHook(topicShort, topicProfile, strategyProfile, audienceSnippet, platformSnippet, variantNumber),
                4 => BuildStoryHook(topicShort, topicProfile, strategyProfile, audienceSnippet, platformSnippet, variantNumber),
                5 => BuildStepByStepHook(topicShort, topicProfile, strategyProfile, audienceSnippet, platformSnippet, variantNumber),
                _ => BuildMythBustingHook(topicShort, topicProfile, strategyProfile, audienceSnippet, platformSnippet, variantNumber)
            };
        }

        private string BuildContrarianHook(
            string topic,
            AiTopicProfile topicProfile,
            AiStrategyProfile strategyProfile,
            string audienceSnippet,
            string platformSnippet,
            int variantNumber)
        {
            var opener = Rotate(variantNumber,
                $"Most people are wrong about {topic}",
                $"The common advice about {topic} is broken",
                $"A lot of people make {topic} harder than it needs to be");

            var closer = Rotate(variantNumber,
                $"because they ignore {topicProfile.Vocabulary1}",
                $"when the real lever is {topicProfile.Vocabulary2}",
                $"and it shows up in weak {topicProfile.Vocabulary3}");

            return Clean($"{opener} {closer}{audienceSnippet}{platformSnippet}");
        }

        private string BuildAuthorityHook(
            string topic,
            AiTopicProfile topicProfile,
            AiStrategyProfile strategyProfile,
            string audienceSnippet,
            string platformSnippet,
            int variantNumber)
        {
            var opener = Rotate(variantNumber,
                $"What actually works in {topic}",
                $"A smarter way to approach {topic}",
                $"What separates strong {topic} from average results");

            var closer = strategyProfile.UsesCredibility
                ? Rotate(variantNumber,
                    $"is understanding {topicProfile.Vocabulary1} before tactics",
                    $"starts with stronger {topicProfile.Vocabulary2}",
                    $"is building trust through better {topicProfile.Vocabulary3}")
                : Rotate(variantNumber,
                    $"is focusing on what moves results",
                    $"starts with clarity, not more noise",
                    $"comes from a better process, not more effort");

            return Clean($"{opener} {closer}{audienceSnippet}{platformSnippet}");
        }

        private string BuildProblemSolutionHook(
            string topic,
            AiTopicProfile topicProfile,
            AiStrategyProfile strategyProfile,
            string audienceSnippet,
            string platformSnippet,
            int variantNumber)
        {
            var opener = Rotate(variantNumber,
                $"If {topic} feels frustrating right now",
                $"If your {topicProfile.Vocabulary1} is inconsistent",
                $"If you're putting effort into {topic} but not seeing enough return");

            var closer = Rotate(variantNumber,
                $"there is usually one reason why",
                $"the fix is simpler than it looks",
                $"you probably need a better system, not more effort");

            return Clean($"{opener}, {closer}{audienceSnippet}{platformSnippet}");
        }

        private string BuildStoryHook(
            string topic,
            AiTopicProfile topicProfile,
            AiStrategyProfile strategyProfile,
            string audienceSnippet,
            string platformSnippet,
            int variantNumber)
        {
            var opener = Rotate(variantNumber,
                $"A small shift changed how I think about {topic}",
                $"One insight completely changed my approach to {topic}",
                $"I used to make the same mistake most people make with {topic}");

            var closer = Rotate(variantNumber,
                $"and it came down to {topicProfile.Mistake}",
                $"until I focused on {topicProfile.Vocabulary1} instead",
                $"which is why this works better now");

            return Clean($"{opener} {closer}{audienceSnippet}{platformSnippet}");
        }

        private string BuildStepByStepHook(
            string topic,
            AiTopicProfile topicProfile,
            AiStrategyProfile strategyProfile,
            string audienceSnippet,
            string platformSnippet,
            int variantNumber)
        {
            var opener = Rotate(variantNumber,
                $"A simple way to improve {topic}",
                $"Here is a better framework for {topic}",
                $"Try this structure if you want stronger {topicProfile.Vocabulary3}");

            var closer = Rotate(variantNumber,
                $"without making it complicated",
                $"in a way that is actually repeatable",
                $"especially if you want more consistent results");

            return Clean($"{opener} {closer}{audienceSnippet}{platformSnippet}");
        }

        private string BuildMythBustingHook(
            string topic,
            AiTopicProfile topicProfile,
            AiStrategyProfile strategyProfile,
            string audienceSnippet,
            string platformSnippet,
            int variantNumber)
        {
            var opener = Rotate(variantNumber,
                $"One myth about {topic} keeps holding people back",
                $"There is a popular belief about {topic} that needs to go",
                $"The biggest misunderstanding in {topic} is this");

            var closer = Rotate(variantNumber,
                $"more effort is not always the answer",
                $"generic advice usually creates generic results",
                $"the better move is improving the system behind it");

            return Clean($"{opener}: {closer}{audienceSnippet}{platformSnippet}");
        }

        private string BuildAudienceHookSnippet(string targetAudience, int variantNumber)
        {
            if (string.IsNullOrWhiteSpace(targetAudience))
            {
                return string.Empty;
            }

            return Rotate(variantNumber,
                $" for {targetAudience}",
                $" if you're speaking to {targetAudience}",
                $" especially for {targetAudience}");
        }

        private string BuildPlatformHookSnippet(string platform, int variantNumber)
        {
            return platform switch
            {
                "linkedin" => Rotate(variantNumber, "", "", ""),
                "x" => Rotate(variantNumber, "", "", ""),
                "twitter" => Rotate(variantNumber, "", "", ""),
                "instagram" => Rotate(variantNumber, "", "", ""),
                "facebook" => Rotate(variantNumber, "", "", ""),
                "youtube" => Rotate(variantNumber, "", "", ""),
                "tiktok" => Rotate(variantNumber, "", "", ""),
                _ => string.Empty
            };
        }

        private string RewriteTopicNaturally(string topic, int variantNumber)
        {
            return Rotate(variantNumber, topic, topic, topic);
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

        private string Clean(string value)
        {
            return Regex.Replace(value, @"\s+", " ").Trim();
        }
    }
}