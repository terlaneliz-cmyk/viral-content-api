using System.Text;
using System.Text.Json;
using OpenAI.Chat;
using ViralContentApi.DTOs;

namespace ViralContentApi.Services
{
    public class AiContentService : IAiContentService
    {
        private readonly IConfiguration _configuration;

        public AiContentService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<GenerateContentResponse> GenerateAsync(GenerateContentRequest request)
        {
            var apiKey = _configuration["OpenAI:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException("OpenAI API key not configured.");
            }

            var client = new ChatClient(model: "gpt-4o-mini", apiKey: apiKey);

            string prompt = BuildPrompt(request);

            ChatCompletion completion = await client.CompleteChatAsync(
                new SystemChatMessage(
                    "You write sharp, concrete, high-retention social content. No fluff, no filler, no vague motivational language. Return only valid JSON."
                ),
                new UserChatMessage(prompt)
            );

            string content = string.Concat(
                completion.Content
                    .Select(part => part.Text)
                    .Where(text => !string.IsNullOrWhiteSpace(text))
            );

            if (string.IsNullOrWhiteSpace(content))
            {
                throw new InvalidOperationException("OpenAI returned empty content.");
            }

            var parsed = JsonSerializer.Deserialize<GenerateContentResponse>(
                content,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (parsed == null)
            {
                throw new InvalidOperationException("Failed to parse OpenAI response.");
            }

            return parsed;
        }

        private static string BuildPrompt(GenerateContentRequest request)
        {
            var safeTopic = string.IsNullOrWhiteSpace(request.Topic) ? "content creation" : request.Topic.Trim();
            var safePlatform = string.IsNullOrWhiteSpace(request.Platform) ? "TikTok" : request.Platform.Trim();
            var safeTone = string.IsNullOrWhiteSpace(request.Tone) ? "bold" : request.Tone.Trim();
            var safeGoal = string.IsNullOrWhiteSpace(request.Goal) ? "engagement" : request.Goal.Trim();
            var safeContentType = string.IsNullOrWhiteSpace(request.ContentType) ? "post" : request.ContentType.Trim();
            var safeAudience = string.IsNullOrWhiteSpace(request.TargetAudience) ? "general audience" : request.TargetAudience.Trim();
            var safeVariantCount = Math.Clamp(request.NumberOfVariants <= 0 ? 1 : request.NumberOfVariants, 1, 10);

            var sb = new StringBuilder();

            sb.AppendLine("Generate viral social media content.");
            sb.AppendLine("Return ONLY valid JSON.");
            sb.AppendLine();
            sb.AppendLine("Required JSON format:");
            sb.AppendLine("""
{
  "topic": "string",
  "platform": "string",
  "tone": "string",
  "variants": [
    {
      "variantNumber": 1,
      "hook": "string",
      "content": "string",
      "callToAction": "string",
      "hashtags": ["#tag1", "#tag2"]
    }
  ]
}
""");
            sb.AppendLine();
            sb.AppendLine($"Topic: {safeTopic}");
            sb.AppendLine($"Platform: {safePlatform}");
            sb.AppendLine($"Tone: {safeTone}");
            sb.AppendLine($"Goal: {safeGoal}");
            sb.AppendLine($"ContentType: {safeContentType}");
            sb.AppendLine($"TargetAudience: {safeAudience}");
            sb.AppendLine($"NumberOfVariants: {safeVariantCount}");
            sb.AppendLine();
            sb.AppendLine("Rules:");
            sb.AppendLine("- Write like a top creator, not like AI.");
            sb.AppendLine("- No generic filler.");
            sb.AppendLine("- No abstract nonsense.");
            sb.AppendLine("- Make the hook strong and specific.");
            sb.AppendLine("- Make the content useful, concrete, and readable.");
            sb.AppendLine("- Keep the CTA natural.");
            sb.AppendLine("- Hashtags should be relevant and not spammy.");
            sb.AppendLine("- Return exactly the requested number of variants.");
            sb.AppendLine("- Do not wrap JSON in markdown fences.");

            return sb.ToString();
        }
    }
}