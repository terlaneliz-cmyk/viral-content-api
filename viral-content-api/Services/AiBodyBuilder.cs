using System.Text;
using System.Text.RegularExpressions;
using ViralContentApi.Services.Models;

namespace ViralContentApi.Services
{
    public class AiBodyBuilder
    {
        private readonly AiProfileTextBuilder _profileTextBuilder;

        public AiBodyBuilder(AiProfileTextBuilder profileTextBuilder)
        {
            _profileTextBuilder = profileTextBuilder;
        }

        public string BuildBody(
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
            var toneLead = BuildToneLead(tone, variantNumber);
            var audienceLine = BuildAudienceLine(targetAudience, variantNumber);
            var goalLine = BuildGoalLine(goal, variantNumber);
            var topicLine = _profileTextBuilder.BuildTopicLine(topic, variantNumber, topicProfile);
            var strategyLine = _profileTextBuilder.BuildStrategyLine(strategyProfile, variantNumber);
            var exampleLine = _profileTextBuilder.BuildExampleLine(topic, topicProfile, variantNumber);
            var credibilityLine = _profileTextBuilder.BuildCredibilityLine(strategyProfile, topicProfile, variantNumber);
            var curiosityLine = _profileTextBuilder.BuildCuriosityLine(strategyProfile, topicProfile, variantNumber);
            var relatabilityLine = _profileTextBuilder.BuildRelatabilityLine(strategyProfile, topicProfile, variantNumber);
            var conversionLine = _profileTextBuilder.BuildConversionLine(strategyProfile, goal, variantNumber);

            return styleIndex switch
            {
                1 => BuildContrarianBody(
                    topicLine,
                    toneLead,
                    audienceLine,
                    goalLine,
                    strategyLine,
                    exampleLine,
                    credibilityLine,
                    curiosityLine,
                    relatabilityLine,
                    conversionLine,
                    topicProfile,
                    strategyProfile,
                    variantNumber),

                2 => BuildAuthorityBody(
                    topicLine,
                    toneLead,
                    audienceLine,
                    goalLine,
                    strategyLine,
                    exampleLine,
                    credibilityLine,
                    curiosityLine,
                    relatabilityLine,
                    conversionLine,
                    topicProfile,
                    strategyProfile,
                    variantNumber),

                3 => BuildProblemSolutionBody(
                    topicLine,
                    toneLead,
                    audienceLine,
                    goalLine,
                    strategyLine,
                    exampleLine,
                    credibilityLine,
                    curiosityLine,
                    relatabilityLine,
                    conversionLine,
                    topicProfile,
                    strategyProfile,
                    variantNumber),

                4 => BuildStoryBody(
                    topicLine,
                    toneLead,
                    audienceLine,
                    goalLine,
                    strategyLine,
                    exampleLine,
                    credibilityLine,
                    curiosityLine,
                    relatabilityLine,
                    conversionLine,
                    topicProfile,
                    strategyProfile,
                    variantNumber),

                5 => BuildStepByStepBody(
                    topicLine,
                    toneLead,
                    audienceLine,
                    goalLine,
                    strategyLine,
                    exampleLine,
                    credibilityLine,
                    curiosityLine,
                    relatabilityLine,
                    conversionLine,
                    topicProfile,
                    strategyProfile,
                    variantNumber),

                _ => BuildMythBody(
                    topicLine,
                    toneLead,
                    audienceLine,
                    goalLine,
                    strategyLine,
                    exampleLine,
                    credibilityLine,
                    curiosityLine,
                    relatabilityLine,
                    conversionLine,
                    topicProfile,
                    strategyProfile,
                    variantNumber)
            };
        }

        private string BuildContrarianBody(
            string topicLine,
            string toneLead,
            string audienceLine,
            string goalLine,
            string strategyLine,
            string exampleLine,
            string credibilityLine,
            string curiosityLine,
            string relatabilityLine,
            string conversionLine,
            AiTopicProfile topicProfile,
            AiStrategyProfile strategyProfile,
            int variantNumber)
        {
            var sb = new StringBuilder();

            sb.AppendLine(toneLead);
            sb.AppendLine();
            sb.AppendLine(topicLine);
            sb.AppendLine();
            sb.AppendLine(Rotate(variantNumber,
                $"The usual mistake is {topicProfile.Mistake}.",
                $"A lot of advice fails because it ignores {topicProfile.Vocabulary1}.",
                $"The problem is not effort. It is weak {topicProfile.Vocabulary2}."));
            sb.AppendLine();
            sb.AppendLine(Rotate(variantNumber,
                $"A better approach is to build around {topicProfile.DesiredOutcome} instead of chasing random tactics.",
                $"The smarter move is to create a repeatable system that supports {topicProfile.DesiredOutcome}.",
                $"The win comes from simplifying the process until it actually supports {topicProfile.Transformation}."));
            sb.AppendLine();
            sb.AppendLine(audienceLine);
            sb.AppendLine(goalLine);
            sb.AppendLine(strategyLine);

            if (!string.IsNullOrWhiteSpace(curiosityLine))
            {
                sb.AppendLine(curiosityLine);
            }

            if (!string.IsNullOrWhiteSpace(credibilityLine))
            {
                sb.AppendLine(credibilityLine);
            }

            sb.AppendLine(exampleLine);

            return CleanMultiline(sb.ToString());
        }

        private string BuildAuthorityBody(
            string topicLine,
            string toneLead,
            string audienceLine,
            string goalLine,
            string strategyLine,
            string exampleLine,
            string credibilityLine,
            string curiosityLine,
            string relatabilityLine,
            string conversionLine,
            AiTopicProfile topicProfile,
            AiStrategyProfile strategyProfile,
            int variantNumber)
        {
            var sb = new StringBuilder();

            sb.AppendLine(toneLead);
            sb.AppendLine();
            sb.AppendLine(topicLine);
            sb.AppendLine();
            sb.AppendLine(Rotate(variantNumber,
                $"Strong results usually come from mastering {topicProfile.Vocabulary1}, sharpening {topicProfile.Vocabulary2}, and staying consistent with {topicProfile.Vocabulary3}.",
                $"People who improve faster tend to focus on {topicProfile.Vocabulary1}, remove weak {topicProfile.Vocabulary2}, and make better execution easier.",
                $"What works best is not more noise. It is clearer {topicProfile.Vocabulary1}, better {topicProfile.Vocabulary2}, and disciplined {topicProfile.Vocabulary3}."));
            sb.AppendLine();
            sb.AppendLine(strategyLine);
            sb.AppendLine(audienceLine);
            sb.AppendLine(goalLine);

            if (!string.IsNullOrWhiteSpace(credibilityLine))
            {
                sb.AppendLine(credibilityLine);
            }

            sb.AppendLine(exampleLine);

            if (!string.IsNullOrWhiteSpace(conversionLine))
            {
                sb.AppendLine(conversionLine);
            }

            return CleanMultiline(sb.ToString());
        }

        private string BuildProblemSolutionBody(
            string topicLine,
            string toneLead,
            string audienceLine,
            string goalLine,
            string strategyLine,
            string exampleLine,
            string credibilityLine,
            string curiosityLine,
            string relatabilityLine,
            string conversionLine,
            AiTopicProfile topicProfile,
            AiStrategyProfile strategyProfile,
            int variantNumber)
        {
            var sb = new StringBuilder();

            sb.AppendLine(toneLead);
            sb.AppendLine();
            sb.AppendLine(Rotate(variantNumber,
                $"A common problem in this space is {topicProfile.PainPoint}.",
                $"One reason progress stalls is {topicProfile.PainPoint}.",
                $"Many people get stuck because of {topicProfile.PainPoint}."));
            sb.AppendLine();
            sb.AppendLine(Rotate(variantNumber,
                $"The fix is usually not bigger effort. It is replacing {topicProfile.Mistake} with a process that supports {topicProfile.DesiredOutcome}.",
                $"What helps most is removing the friction behind {topicProfile.Mistake} and building toward {topicProfile.DesiredOutcome}.",
                $"The better path is to make the system simpler, clearer, and easier to repeat."));
            sb.AppendLine();
            sb.AppendLine(topicLine);
            sb.AppendLine(audienceLine);
            sb.AppendLine(goalLine);
            sb.AppendLine(strategyLine);
            sb.AppendLine(exampleLine);

            if (!string.IsNullOrWhiteSpace(relatabilityLine))
            {
                sb.AppendLine(relatabilityLine);
            }

            return CleanMultiline(sb.ToString());
        }

        private string BuildStoryBody(
            string topicLine,
            string toneLead,
            string audienceLine,
            string goalLine,
            string strategyLine,
            string exampleLine,
            string credibilityLine,
            string curiosityLine,
            string relatabilityLine,
            string conversionLine,
            AiTopicProfile topicProfile,
            AiStrategyProfile strategyProfile,
            int variantNumber)
        {
            var sb = new StringBuilder();

            sb.AppendLine(toneLead);
            sb.AppendLine();
            sb.AppendLine(Rotate(variantNumber,
                $"At first, the obvious move looked like more effort.",
                $"The first instinct is usually to do more.",
                $"I used to think the answer was pushing harder."));
            sb.AppendLine();
            sb.AppendLine(Rotate(variantNumber,
                $"But the real shift came from noticing that {topicProfile.Mistake}.",
                $"Then it became clear that the real issue was {topicProfile.Mistake}.",
                $"What changed everything was realizing the system itself was weak."));
            sb.AppendLine();
            sb.AppendLine(Rotate(variantNumber,
                $"Once the focus moved toward {topicProfile.Vocabulary1} and {topicProfile.Vocabulary2}, results became more consistent.",
                $"Once the process supported {topicProfile.DesiredOutcome}, momentum became easier to maintain.",
                $"Once the approach became simpler and more specific, progress felt less random."));
            sb.AppendLine();
            sb.AppendLine(topicLine);
            sb.AppendLine(audienceLine);
            sb.AppendLine(goalLine);
            sb.AppendLine(exampleLine);

            if (!string.IsNullOrWhiteSpace(relatabilityLine))
            {
                sb.AppendLine(relatabilityLine);
            }

            return CleanMultiline(sb.ToString());
        }

        private string BuildStepByStepBody(
            string topicLine,
            string toneLead,
            string audienceLine,
            string goalLine,
            string strategyLine,
            string exampleLine,
            string credibilityLine,
            string curiosityLine,
            string relatabilityLine,
            string conversionLine,
            AiTopicProfile topicProfile,
            AiStrategyProfile strategyProfile,
            int variantNumber)
        {
            var step1 = Rotate(variantNumber,
                $"1. Get clear on the real outcome: {topicProfile.DesiredOutcome}.",
                $"1. Define what better actually means: {topicProfile.DesiredOutcome}.",
                $"1. Start with the target result instead of random activity.");

            var step2 = Rotate(variantNumber,
                $"2. Remove the biggest mistake: {topicProfile.Mistake}.",
                $"2. Find the friction point causing inconsistent execution.",
                $"2. Stop doing the thing that weakens {topicProfile.Vocabulary1}.");

            var step3 = Rotate(variantNumber,
                $"3. Build around {topicProfile.Vocabulary1}, {topicProfile.Vocabulary2}, and repeatable action.",
                $"3. Create a simpler system that improves {topicProfile.Vocabulary3} over time.",
                $"3. Make the process easier to repeat than to avoid.");

            var sb = new StringBuilder();

            sb.AppendLine(toneLead);
            sb.AppendLine();
            sb.AppendLine(topicLine);
            sb.AppendLine();
            sb.AppendLine(step1);
            sb.AppendLine(step2);
            sb.AppendLine(step3);
            sb.AppendLine();
            sb.AppendLine(audienceLine);
            sb.AppendLine(goalLine);
            sb.AppendLine(strategyLine);
            sb.AppendLine(exampleLine);

            return CleanMultiline(sb.ToString());
        }

        private string BuildMythBody(
            string topicLine,
            string toneLead,
            string audienceLine,
            string goalLine,
            string strategyLine,
            string exampleLine,
            string credibilityLine,
            string curiosityLine,
            string relatabilityLine,
            string conversionLine,
            AiTopicProfile topicProfile,
            AiStrategyProfile strategyProfile,
            int variantNumber)
        {
            var sb = new StringBuilder();

            sb.AppendLine(toneLead);
            sb.AppendLine();
            sb.AppendLine(Rotate(variantNumber,
                $"A common myth is that more effort automatically fixes the problem.",
                $"One bad assumption is that generic tactics will create strong results.",
                $"A lot of people believe intensity matters more than structure."));
            sb.AppendLine();
            sb.AppendLine(Rotate(variantNumber,
                $"In reality, better {topicProfile.Vocabulary1} and clearer {topicProfile.Vocabulary2} usually matter more.",
                $"What usually changes outcomes is improving the system behind the work.",
                $"The real edge comes from consistency, relevance, and better execution."));
            sb.AppendLine();
            sb.AppendLine(topicLine);
            sb.AppendLine(audienceLine);
            sb.AppendLine(goalLine);
            sb.AppendLine(strategyLine);
            sb.AppendLine(exampleLine);

            if (!string.IsNullOrWhiteSpace(curiosityLine))
            {
                sb.AppendLine(curiosityLine);
            }

            return CleanMultiline(sb.ToString());
        }

        private string BuildToneLead(string tone, int variantNumber)
        {
            return tone switch
            {
                "confident" => Rotate(variantNumber,
                    "Here is the honest version.",
                    "Let's be direct about this.",
                    "This is worth saying clearly."),

                "friendly" => Rotate(variantNumber,
                    "Here is a simple way to look at it.",
                    "Let's make this easier.",
                    "This gets a lot simpler when you break it down."),

                "professional" => Rotate(variantNumber,
                    "A practical way to frame this is:",
                    "A stronger approach looks like this.",
                    "From a strategic point of view, this matters."),

                "bold" => Rotate(variantNumber,
                    "Most people are overcomplicating it.",
                    "This needs to be said more often.",
                    "The usual advice misses the point."),

                "motivational" => Rotate(variantNumber,
                    "Progress gets easier when the approach is clear.",
                    "Small shifts can create real momentum.",
                    "Consistency improves when the process gets simpler."),

                _ => Rotate(variantNumber,
                    "Here is the core idea.",
                    "This is the main point.",
                    "This is the part that matters most.")
            };
        }

        private string BuildAudienceLine(string targetAudience, int variantNumber)
        {
            if (string.IsNullOrWhiteSpace(targetAudience))
            {
                return Rotate(variantNumber,
                    "This works best when you keep it specific and practical.",
                    "The simpler the execution, the easier it is to stay consistent.",
                    "Clarity usually creates better results than more complexity.");
            }

            return Rotate(variantNumber,
                $"This matters especially for {targetAudience}.",
                $"For {targetAudience}, this can remove a lot of friction.",
                $"If you're speaking to {targetAudience}, this becomes even more useful.");
        }

        private string BuildGoalLine(string goal, int variantNumber)
        {
            return goal switch
            {
                "engagement" => Rotate(variantNumber,
                    "The more relatable and specific the message feels, the more people respond to it.",
                    "People engage more when the point is clear and easy to connect with.",
                    "Specificity usually creates better conversation than broad advice."),

                "followers" => Rotate(variantNumber,
                    "Strong growth usually comes from repeatable value and a clear point of view.",
                    "People follow when they know what kind of value to expect from you.",
                    "Consistency and clarity tend to grow an audience faster than random spikes."),

                "leads" => Rotate(variantNumber,
                    "Real business results usually come from relevance, trust, and clear intent.",
                    "Attention helps, but qualified interest matters more.",
                    "The best content for leads makes the next step feel obvious."),

                "authority" => Rotate(variantNumber,
                    "Authority grows when your thinking feels clear, specific, and useful.",
                    "Trust builds faster when the advice feels grounded and practical.",
                    "Credibility increases when your message sounds earned, not recycled."),

                _ => Rotate(variantNumber,
                    "Clearer communication usually improves results.",
                    "Better positioning makes the message easier to act on.",
                    "The strongest ideas are usually the easiest to understand.")
            };
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

        private string CleanMultiline(string value)
        {
            var normalized = value.Replace("\r\n", "\n").Replace("\r", "\n");
            normalized = Regex.Replace(normalized, @"\n{3,}", "\n\n");

            var lines = normalized
                .Split('\n')
                .Select(x => x.TrimEnd())
                .ToList();

            return string.Join(Environment.NewLine, lines).Trim();
        }
    }
}