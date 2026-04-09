using System.Text.RegularExpressions;

namespace ViralContentApi.Services
{
    public class AiContentFormatter
    {
        public string FormatContentForPlatformAndType(string body, string platform, string contentType, int variantNumber)
        {
            if (contentType == "thread")
            {
                return FormatAsThread(body, platform, variantNumber);
            }

            if (contentType == "script")
            {
                return FormatAsScript(body, platform, variantNumber);
            }

            if (contentType == "video idea")
            {
                return FormatAsVideoIdea(body, platform, variantNumber);
            }

            if (contentType == "caption")
            {
                return FormatAsCaption(body, platform, variantNumber);
            }

            return FormatAsPost(body, platform, variantNumber);
        }

        private string FormatAsPost(string body, string platform, int variantNumber)
        {
            return platform switch
            {
                "linkedin" => body,
                "x" => TightenForX(body),
                "twitter" => TightenForX(body),
                "tiktok" => MakeMoreSpoken(body),
                "youtube" => AddYouTubeRhythm(body, variantNumber),
                "instagram" => AddInstagramRhythm(body),
                "facebook" => AddFacebookFlow(body),
                _ => body
            };
        }

        private string FormatAsCaption(string body, string platform, int variantNumber)
        {
            var tightened = ShortenParagraphs(body);

            return platform switch
            {
                "instagram" => AddInstagramRhythm(tightened),
                "facebook" => AddFacebookFlow(tightened),
                "x" => TightenForX(tightened),
                "twitter" => TightenForX(tightened),
                _ => tightened
            };
        }

        private string FormatAsScript(string body, string platform, int variantNumber)
        {
            var lines = body
                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            var script = string.Join(Environment.NewLine, lines.Select(x => $"- {x}"));

            if (platform == "youtube" || platform == "tiktok")
            {
                script = $"Hook:{Environment.NewLine}{script}";
            }

            return script;
        }

        private string FormatAsThread(string body, string platform, int variantNumber)
        {
            var lines = body
                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            var numbered = new List<string>();

            for (int i = 0; i < lines.Count; i++)
            {
                numbered.Add($"{i + 1}. {lines[i]}");
            }

            var result = string.Join(Environment.NewLine + Environment.NewLine, numbered);

            if (platform == "x" || platform == "twitter")
            {
                result = TightenForX(result);
            }

            return result;
        }

        private string FormatAsVideoIdea(string body, string platform, int variantNumber)
        {
            var lines = body
                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            var concept = lines.Count > 0 ? lines[0] : "Strong concept";
            var angle = lines.Count > 1 ? lines[1] : "Useful angle";
            var whyItWorks = lines.Count > 2 ? lines[2] : "Easy to understand and relevant";

            return
                "Concept:" + Environment.NewLine +
                concept + Environment.NewLine + Environment.NewLine +
                "Angle:" + Environment.NewLine +
                angle + Environment.NewLine + Environment.NewLine +
                "Why it works:" + Environment.NewLine +
                whyItWorks;
        }

        private string TightenForX(string text)
        {
            var lines = text
                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Take(6);

            return string.Join(Environment.NewLine + Environment.NewLine, lines);
        }

        private string MakeMoreSpoken(string text)
        {
            return text
                .Replace("usually", "often")
                .Replace("tends to", "often")
                .Replace("increases", "goes up");
        }

        private string AddYouTubeRhythm(string text, int variantNumber)
        {
            return $"{Rotate(variantNumber, "Here is the key idea.", "Let's break this down.", "This is where it gets interesting.")}{Environment.NewLine}{Environment.NewLine}{text}";
        }

        private string AddInstagramRhythm(string text)
        {
            var lines = text
                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(x => !string.IsNullOrWhiteSpace(x));

            return string.Join(Environment.NewLine + Environment.NewLine, lines);
        }

        private string AddFacebookFlow(string text)
        {
            return text;
        }

        private string ShortenParagraphs(string text)
        {
            var lines = text
                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Take(5);

            return string.Join(Environment.NewLine + Environment.NewLine, lines);
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