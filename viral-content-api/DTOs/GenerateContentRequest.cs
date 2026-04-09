using System.ComponentModel.DataAnnotations;

namespace ViralContentApi.DTOs;

public class GenerateContentRequest
{
    [Required]
    public string Topic { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^(?i)(linkedin|youtube|tiktok|tik tok|x|twitter|instagram|facebook)$",
        ErrorMessage = "Platform must be one of: LinkedIn, YouTube, TikTok, X, Twitter, Instagram, Facebook.")]
    public string Platform { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^(?i)(confident|professional|friendly|bold|inspirational)$",
        ErrorMessage = "Tone must be one of: confident, professional, friendly, bold, inspirational.")]
    public string Tone { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^(?i)(engagement|followers|leads|authority)$",
        ErrorMessage = "Goal must be one of: engagement, followers, leads, authority.")]
    public string Goal { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^(?i)(post|caption|script|thread|video idea|videoidea|video-idea|video)$",
        ErrorMessage = "ContentType must be one of: post, caption, script, thread, video idea.")]
    public string ContentType { get; set; } = string.Empty;

    public string TargetAudience { get; set; } = string.Empty;

    [Range(1, 10, ErrorMessage = "NumberOfVariants must be between 1 and 10.")]
    public int NumberOfVariants { get; set; } = 3;
}