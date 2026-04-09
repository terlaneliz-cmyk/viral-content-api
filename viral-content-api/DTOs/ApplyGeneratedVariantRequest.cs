using System.Collections.Generic;

namespace ViralContentApi.DTOs;

public class ApplyGeneratedVariantRequest
{
    public string? Title { get; set; }

    public string? Topic { get; set; }

    public string? Platform { get; set; }

    public string Hook { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public string CallToAction { get; set; } = string.Empty;

    public List<string> Hashtags { get; set; } = new();

    public string? Tone { get; set; }

    public string? Goal { get; set; }

    public string? ContentType { get; set; }

    public string? TargetAudience { get; set; }
}