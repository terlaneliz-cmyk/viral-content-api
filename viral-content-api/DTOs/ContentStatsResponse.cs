namespace ViralContentApi.DTOs;

public class ContentStatsResponse
{
    public int TotalPosts { get; set; }

    public int TotalViews { get; set; }

    public int TotalLikes { get; set; }

    public int TotalShares { get; set; }

    public double AverageViralScore { get; set; }

    public TopPostResponse? TopPost { get; set; }
}