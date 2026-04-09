using ViralContentApi.DTOs;
using ViralContentApi.Services.Models;

namespace ViralContentApi.Services
{
    public class AiContentService : IAiContentService
    {
        private readonly AiTopicProfileBuilder _topicProfileBuilder;
        private readonly AiStrategyProfileBuilder _strategyProfileBuilder;
        private readonly AiEngagementTextBuilder _engagementTextBuilder;
        private readonly AiContentFormatter _contentFormatter;
        private readonly AiHookBuilder _hookBuilder;
        private readonly AiBodyBuilder _bodyBuilder;

        public AiContentService(
            AiTopicProfileBuilder topicProfileBuilder,
            AiStrategyProfileBuilder strategyProfileBuilder,
            AiEngagementTextBuilder engagementTextBuilder,
            AiContentFormatter contentFormatter,
            AiHookBuilder hookBuilder,
            AiBodyBuilder bodyBuilder)
        {
            _topicProfileBuilder = topicProfileBuilder;
            _strategyProfileBuilder = strategyProfileBuilder;
            _engagementTextBuilder = engagementTextBuilder;
            _contentFormatter = contentFormatter;
            _hookBuilder = hookBuilder;
            _bodyBuilder = bodyBuilder;
        }

        public Task<GenerateContentResponse> GenerateAsync(GenerateContentRequest request)
        {
            var topic = Normalize(request.Topic);
            var platform = NormalizePlatform(request.Platform);
            var tone = NormalizeTone(request.Tone);
            var goal = NormalizeGoal(request.Goal);
            var contentType = NormalizeContentType(request.ContentType);
            var targetAudience = Normalize(request.TargetAudience);

            if (string.IsNullOrWhiteSpace(topic))
            {
                topic = "content creation";
            }

            var variants = BuildVariants(
                topic,
                platform,
                tone,
                goal,
                contentType,
                targetAudience,
                request.NumberOfVariants);

            var response = new GenerateContentResponse
            {
                Topic = topic,
                Platform = platform,
                Tone = tone,
                Variants = variants
            };

            return Task.FromResult(response);
        }

        private List<GeneratedContentVariantResponse> BuildVariants(
            string topic,
            string platform,
            string tone,
            string goal,
            string contentType,
            string targetAudience,
            int numberOfVariants)
        {
            var safeVariantCount = Math.Clamp(numberOfVariants <= 0 ? 3 : numberOfVariants, 1, 12);

            var topicProfile = _topicProfileBuilder.Build(topic);
            var strategyProfile = _strategyProfileBuilder.Build(platform, goal, contentType);

            var variants = new List<GeneratedContentVariantResponse>();

            for (int i = 1; i <= safeVariantCount; i++)
            {
                var styleIndex = ((i - 1) % 6) + 1;

                var hook = _hookBuilder.BuildHook(
                    styleIndex,
                    i,
                    topic,
                    platform,
                    tone,
                    goal,
                    contentType,
                    targetAudience,
                    topicProfile,
                    strategyProfile);

                var body = _bodyBuilder.BuildBody(
                    styleIndex,
                    i,
                    topic,
                    platform,
                    tone,
                    goal,
                    contentType,
                    targetAudience,
                    topicProfile,
                    strategyProfile);

                var content = _contentFormatter.FormatContentForPlatformAndType(body, platform, contentType, i);
                var cta = _engagementTextBuilder.BuildCallToAction(i, platform, goal, contentType, targetAudience, strategyProfile);
                var hashtags = _engagementTextBuilder.BuildHashtags(topic, platform, topicProfile);

                variants.Add(new GeneratedContentVariantResponse
                {
                    VariantNumber = i,
                    Hook = hook,
                    Content = content,
                    CallToAction = cta,
                    Hashtags = hashtags
                });
            }

            return variants;
        }

        private string Normalize(string? value)
        {
            return value?.Trim() ?? string.Empty;
        }

        private string NormalizePlatform(string? value)
        {
            var normalized = value?.Trim().ToLowerInvariant() ?? string.Empty;

            return normalized switch
            {
                "twitter" => "x",
                "x/twitter" => "x",
                "x (twitter)" => "x",
                _ => normalized
            };
        }

        private string NormalizeTone(string? value)
        {
            var normalized = value?.Trim().ToLowerInvariant() ?? string.Empty;

            return normalized switch
            {
                "casual" => "friendly",
                "expert" => "professional",
                _ => normalized
            };
        }

        private string NormalizeGoal(string? value)
        {
            var normalized = value?.Trim().ToLowerInvariant() ?? string.Empty;

            return normalized switch
            {
                "" => "engagement",
                "sales" => "leads",
                _ => normalized
            };
        }

        private string NormalizeContentType(string? value)
        {
            var normalized = value?.Trim().ToLowerInvariant() ?? string.Empty;

            return normalized switch
            {
                "" => "post",
                "tweet thread" => "thread",
                "idea" => "video idea",
                _ => normalized
            };
        }
    }
}