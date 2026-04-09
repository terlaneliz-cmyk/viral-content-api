namespace ViralContentApi.DTOs;

public class LeaderboardResponse
{
    public int Count { get; set; }
    public List<TopPostResponse> Posts { get; set; } = [];
}