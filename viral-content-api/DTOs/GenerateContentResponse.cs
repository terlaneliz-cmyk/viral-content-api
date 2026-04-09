namespace ViralContentApi.DTOs;

public class GenerateContentResponse
{
    public string Topic { get; set; } = string.Empty;

    public string Platform { get; set; } = string.Empty;

    public string Tone { get; set; } = string.Empty;

    public List<GeneratedContentVariantResponse> Variants { get; set; } = new();
}