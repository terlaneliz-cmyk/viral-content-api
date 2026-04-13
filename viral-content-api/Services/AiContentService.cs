using System.Text;
using System.Text.Json;
using OpenAI;
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
                throw new Exception("OpenAI API key not configured.");
            }

            var client = new OpenAIClient(apiKey);

            var prompt = BuildPrompt(request);

            var chatRequest = new ChatRequest
            {
                Model = "gpt-4o-mini",
                Messages = new List<Message>
                {
                    new Message(Role.System, "You are a world-class viral content creator. No fluff. No generic advice. Sharp, punchy, high-retention content."),
                    new Message(Role.User, prompt)
                },
                Temperature = 0.9
            };

            var response = await client.ChatEndpoint.GetCompletionAsync(chatRequest);

            var content = response.FirstChoice.Message.Content;

            var parsed = JsonSerializer.Deserialize<GenerateContentResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (parsed == null)
            {
                throw new Exception("Failed to parse AI response.");
            }

            return parsed;
        }

        private string BuildPrompt(GenerateContentRequest request)
        {
            var sb = new StringBuilder();

            sb.AppendLine("Generate viral social media content.");
            sb.AppendLine("Return ONLY valid JSON. No explanations.");

            sb.AppendLine("Format:");
            sb.AppendLine(@"
{
  ""topic"": ""string"",
  ""platform"": ""string"",
  ""tone"": ""string"",
  ""variants"": [
    {
      ""variantNumber"": 1,
      ""hook"": ""string"",
      ""content"": ""string"",
      ""callToAction"": ""string"",
      ""hashtags"": [""#tag""]
    }
  ]
}
");

            sb.AppendLine($"Topic: {request.Topic}");
            sb.AppendLine($"Platform: {request.Platform}");
            sb.AppendLine($"Tone: {request.Tone}");
            sb.AppendLine($"Goal: {request.Goal}");
            sb.AppendLine($"ContentType: {request.ContentType}");
            sb.AppendLine($"TargetAudience: {request.TargetAudience}");
            sb.AppendLine($"Variants: {request.NumberOfVariants}");

            sb.AppendLine("Rules:");
            sb.AppendLine("- Hook must be strong and scroll-stopping");
            sb.AppendLine("- No generic phrases");
            sb.AppendLine("- Content must be specific and practical");
            sb.AppendLine("- Use short punchy sentences");
            sb.AppendLine("- Avoid fluff and repetition");
            sb.AppendLine("- Write like a top creator, not like AI");

            return sb.ToString();
        }
    }
}