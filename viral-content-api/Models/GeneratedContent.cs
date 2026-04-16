using System.ComponentModel.DataAnnotations;

namespace ViralContentApi.Models;

public class GeneratedContent
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public string Topic { get; set; } = string.Empty;

    [Required]
    public string Platform { get; set; } = string.Empty;

    [Required]
    public string Tone { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty; // JSON string

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}