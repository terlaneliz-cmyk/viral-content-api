namespace ViralContentApi.DTOs;

public class TopPostResponse
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public int Views { get; set; }

    public int Likes { get; set; }

    public int Shares { get; set; }

    public DateTime CreatedAt { get; set; }

    public double ViralScore { get; set; }

    public string? Username { get; set; }
}