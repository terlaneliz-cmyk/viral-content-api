using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OpenAI.Chat;
using ViralContentApi.Data;
using ViralContentApi.DTOs;
using ViralContentApi.Models;

namespace ViralContentApi.Services
{
    public class AiContentService : IAiContentService
    {
        private readonly IConfiguration _configuration;
        private readonly IAiUsageLimitService _usageService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _context;

        public AiContentService(
            IConfiguration configuration,
            IAiUsageLimitService usageService,
            IHttpContextAccessor httpContextAccessor,
            AppDbContext context)
        {
            _configuration = configuration;
            _usageService = usageService;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        public async Task<GenerateContentResponse> GenerateAsync(GenerateContentRequest request)
        {
            var apiKey = _configuration["OpenAI:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("OpenAI API key not configured.");

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                throw new InvalidOperationException("HttpContext is not available.");

            var userIdClaim =
                httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                httpContext.User.FindFirst("nameid")?.Value ??
                httpContext.User.FindFirst("sub")?.Value;

            if (!int.TryParse(userIdClaim, out var userId) || userId <= 0)
                throw new InvalidOperationException("Authenticated user ID could not be resolved.");

            var usageStatus = await _usageService.GetUsageStatusAsync(userId);
            if (usageStatus.RemainingToday <= 0)
                throw new InvalidOperationException("Daily limit reached.");

            var client = new ChatClient(model: "gpt-4o-mini", apiKey: apiKey);

            string prompt = BuildPrompt(request);

            ChatCompletion completion = await client.CompleteChatAsync(
                new SystemChatMessage(
                    "You are a top 1% viral content creator. Write like a human. No generic advice. No filler phrases. No vague motivational language. Use concrete examples, sharp hooks, short sentences, and platform-native style. Return only valid JSON."
                ),
                new UserChatMessage(prompt)
            );

            string content = string.Concat(
                completion.Content
                    .Select(part => part.Text)
                    .Where(text => !string.IsNullOrWhiteSpace(text))
            );

            if (string.IsNullOrWhiteSpace(content))
                throw new InvalidOperationException("OpenAI returned empty content.");

            GenerateContentResponse? parsed;
            try
            {
                parsed = JsonSerializer.Deserialize<GenerateContentResponse>(
                    content,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to parse OpenAI response. Raw response: {content}", ex);
            }

            if (parsed == null)
                throw new InvalidOperationException("Failed to parse OpenAI response.");

            await _usageService.RecordUsageAsync(userId);

            // ✅ SAVE TO DB
            var generatedJson = JsonSerializer.Serialize(parsed);

            var dbContent = new GeneratedContent
            {
                UserId = userId,
                Topic = request.Topic,
                Platform = request.Platform,
                Tone = request.Tone,
                Content = generatedJson,
                CreatedAt = DateTime.UtcNow
            };

            _context.GeneratedContents.Add(dbContent);
            await _context.SaveChangesAsync();

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
            sb.AppendLine("- Write like a strong human creator, not like AI.");
            sb.AppendLine("- No generic filler.");
            sb.AppendLine("- No abstract nonsense.");
            sb.AppendLine("- Do not repeat ideas.");
            sb.AppendLine("- Avoid generic motivation.");
            sb.AppendLine("- Every sentence must add value.");
            sb.AppendLine("- Make the hook strong, specific, and scroll-stopping.");
            sb.AppendLine("- Make the content concrete, readable, and practical.");
            sb.AppendLine("- Keep the CTA natural.");
            sb.AppendLine("- Hashtags should be relevant and not spammy.");
            sb.AppendLine("- Return exactly the requested number of variants.");
            sb.AppendLine("- Do not wrap JSON in markdown fences.");

            return sb.ToString();
        }
    }
}