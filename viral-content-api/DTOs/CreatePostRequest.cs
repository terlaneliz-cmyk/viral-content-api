using System.ComponentModel.DataAnnotations;

namespace ViralContentApi.DTOs;

public class CreatePostRequest
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [Url]
    public string Url { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int Views { get; set; }

    [Range(0, int.MaxValue)]
    public int Likes { get; set; }

    [Range(0, int.MaxValue)]
    public int Shares { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}