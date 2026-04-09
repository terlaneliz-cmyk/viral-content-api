using ViralContentApi.Services.Models;

namespace ViralContentApi.Services
{
    public class AiTopicProfileBuilder
    {
        public AiTopicProfile Build(string topic)
        {
            var normalizedTopic = Normalize(topic);

            if (ContainsAny(normalizedTopic, "fitness", "workout", "gym", "exercise", "fat loss", "muscle", "health"))
            {
                return new AiTopicProfile
                {
                    Family = "fitness",
                    PainPoint = "inconsistent routines and low energy",
                    DesiredOutcome = "better energy, consistency, and visible progress",
                    Mistake = "trying to rely on motivation instead of a repeatable routine",
                    Transformation = "a simple fitness system that fits a busy schedule",
                    Vocabulary1 = "routine",
                    Vocabulary2 = "energy",
                    Vocabulary3 = "consistency",
                    ExampleFocus = "busy people trying to stay in shape"
                };
            }

            if (ContainsAny(normalizedTopic, "personal brand", "personal branding", "brand building", "visibility", "reputation", "thought leadership"))
            {
                return new AiTopicProfile
                {
                    Family = "personal-branding",
                    PainPoint = "low visibility and weak positioning",
                    DesiredOutcome = "more trust, recognition, and authority",
                    Mistake = "posting without a clear point of view or positioning",
                    Transformation = "a sharper reputation that attracts the right audience",
                    Vocabulary1 = "positioning",
                    Vocabulary2 = "visibility",
                    Vocabulary3 = "trust",
                    ExampleFocus = "becoming known for a clear expertise"
                };
            }

            if (ContainsAny(normalizedTopic, "lead generation", "lead-gen", "leads", "pipeline", "prospects", "inbound", "outbound", "appointments"))
            {
                return new AiTopicProfile
                {
                    Family = "lead-generation",
                    PainPoint = "an inconsistent pipeline and low-quality inquiries",
                    DesiredOutcome = "more qualified leads and better conversion",
                    Mistake = "creating attention without enough buying intent",
                    Transformation = "content that turns interest into qualified conversations",
                    Vocabulary1 = "pipeline",
                    Vocabulary2 = "inquiry",
                    Vocabulary3 = "conversion",
                    ExampleFocus = "turning content into real business opportunities"
                };
            }

            if (ContainsAny(normalizedTopic, "productivity", "focus", "deep work", "time management", "priorities", "discipline"))
            {
                return new AiTopicProfile
                {
                    Family = "productivity",
                    PainPoint = "constant distraction and unclear priorities",
                    DesiredOutcome = "more focus, momentum, and output",
                    Mistake = "confusing busyness with meaningful progress",
                    Transformation = "a simpler system for focused execution",
                    Vocabulary1 = "focus",
                    Vocabulary2 = "priorities",
                    Vocabulary3 = "systems",
                    ExampleFocus = "doing the right work with less friction"
                };
            }

            if (ContainsAny(normalizedTopic, "content", "content creation", "social media", "posting", "viral content", "hooks"))
            {
                return new AiTopicProfile
                {
                    Family = "content",
                    PainPoint = "low engagement and unclear messaging",
                    DesiredOutcome = "clearer messaging and stronger audience response",
                    Mistake = "posting ideas that are too broad or too forgettable",
                    Transformation = "content that feels sharper, more relevant, and easier to engage with",
                    Vocabulary1 = "message",
                    Vocabulary2 = "angle",
                    Vocabulary3 = "audience",
                    ExampleFocus = "making content more interesting and more strategic"
                };
            }

            if (ContainsAny(normalizedTopic, "email", "newsletter", "subject line", "cold email", "email marketing", "reply rate"))
            {
                return new AiTopicProfile
                {
                    Family = "email",
                    PainPoint = "low open rates and weak reply rates",
                    DesiredOutcome = "more opens, more replies, and better relevance",
                    Mistake = "writing messages that sound generic or easy to ignore",
                    Transformation = "emails that feel timely, specific, and worth answering",
                    Vocabulary1 = "subject line",
                    Vocabulary2 = "reply",
                    Vocabulary3 = "relevance",
                    ExampleFocus = "sending messages people actually notice"
                };
            }

            return new AiTopicProfile
            {
                Family = "general",
                PainPoint = "slow progress and unclear messaging",
                DesiredOutcome = "better clarity and stronger results",
                Mistake = "copying generic advice without adapting it",
                Transformation = "a more focused and effective approach",
                Vocabulary1 = "results",
                Vocabulary2 = "strategy",
                Vocabulary3 = "improvement",
                ExampleFocus = "practical actions that create momentum"
            };
        }

        private static string Normalize(string? value)
        {
            return value?.Trim().ToLowerInvariant() ?? string.Empty;
        }

        private static bool ContainsAny(string input, params string[] keywords)
        {
            foreach (var keyword in keywords)
            {
                if (input.Contains(keyword))
                {
                    return true;
                }
            }

            return false;
        }
    }
}