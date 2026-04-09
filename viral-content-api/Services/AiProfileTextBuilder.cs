using ViralContentApi.Services.Models;

namespace ViralContentApi.Services
{
    public class AiProfileTextBuilder
    {
        public string BuildTopicLine(string topic, int variantNumber, AiTopicProfile topicProfile)
        {
            return Rotate(variantNumber,
                $"When it comes to {topic}, most improvement starts with better {topicProfile.Vocabulary1}.",
                $"In {topic}, progress tends to come from clearer {topicProfile.Vocabulary2} and more consistent execution.",
                $"With {topic}, stronger outcomes usually come from improving {topicProfile.Vocabulary3} instead of adding more noise.");
        }

        public string BuildStrategyLine(AiStrategyProfile strategyProfile, int variantNumber)
        {
            return Rotate(variantNumber,
                $"The message works best when the angle is built around {strategyProfile.PrimaryAngle} and the body emphasizes {strategyProfile.BodyEmphasis}.",
                $"A stronger post usually leads with {strategyProfile.PrimaryAngle} and backs it up with {strategyProfile.BodyEmphasis}.",
                $"The content becomes more effective when the hook uses a {strategyProfile.HookStyle} style and the CTA invites {strategyProfile.CtaMode}.");
        }

        public string BuildExampleLine(string topic, AiTopicProfile topicProfile, int variantNumber)
        {
            return Rotate(variantNumber,
                $"A useful example is focusing on {topicProfile.ExampleFocus} instead of generic tips.",
                $"One practical angle is to make the message about {topicProfile.ExampleFocus}.",
                $"A stronger example usually highlights {topicProfile.ExampleFocus} in a concrete way.");
        }

        public string BuildCredibilityLine(AiStrategyProfile strategyProfile, AiTopicProfile topicProfile, int variantNumber)
        {
            if (!strategyProfile.UsesCredibility)
            {
                return string.Empty;
            }

            return Rotate(variantNumber,
                $"That is usually where trust starts: clearer {topicProfile.Vocabulary1} and stronger proof in the message.",
                $"Credibility tends to increase when the advice is specific enough to sound earned.",
                $"People trust this faster when the message sounds grounded in real execution.");
        }

        public string BuildCuriosityLine(AiStrategyProfile strategyProfile, AiTopicProfile topicProfile, int variantNumber)
        {
            if (!strategyProfile.UsesCuriosity)
            {
                return string.Empty;
            }

            return Rotate(variantNumber,
                $"That is also why a curiosity-driven hook can pull people into the rest of the message.",
                $"A little tension early on makes people more likely to keep reading.",
                $"Curiosity works best when it opens a loop that the body quickly resolves.");
        }

        public string BuildRelatabilityLine(AiStrategyProfile strategyProfile, AiTopicProfile topicProfile, int variantNumber)
        {
            if (!strategyProfile.UsesEmotionalRelatability)
            {
                return string.Empty;
            }

            return Rotate(variantNumber,
                $"It lands better when people can immediately see themselves in the situation.",
                $"Relatable language lowers resistance and makes the point easier to accept.",
                $"The message feels stronger when it reflects what people are already experiencing.");
        }

        public string BuildConversionLine(AiStrategyProfile strategyProfile, string goal, int variantNumber)
        {
            if (!strategyProfile.UsesConversionIntent && goal != "leads")
            {
                return string.Empty;
            }

            return Rotate(variantNumber,
                $"The next step should feel natural, not forced.",
                $"The strongest conversion moments usually come from relevance and timing.",
                $"The more specific the value feels, the easier it is for someone to take action.");
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