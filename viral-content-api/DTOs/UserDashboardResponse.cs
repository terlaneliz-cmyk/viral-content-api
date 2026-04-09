namespace ViralContentApi.DTOs;

public class UserDashboardResponse
{
    public string Username { get; set; } = string.Empty;

    public int TotalPosts { get; set; }

    public int TotalViews { get; set; }

    public int TotalLikes { get; set; }

    public int TotalShares { get; set; }

    public double AverageViralScore { get; set; }

    public int PostsLast7Days { get; set; }

    public TopPostResponse? TopPost { get; set; }

    public List<PostResponse> RecentPosts { get; set; } = new();
}