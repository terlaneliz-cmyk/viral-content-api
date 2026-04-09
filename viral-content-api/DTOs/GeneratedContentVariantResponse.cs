namespace ViralContentApi.DTOs;

public class GeneratedContentVariantResponse
{
    public int VariantNumber { get; set; }

    public string Hook { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public string CallToAction { get; set; } = string.Empty;

    public List<string> Hashtags { get; set; } = new();
}