namespace ViralContentApi.DTOs;

public class PagedPostsResponse
{
    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public int TotalPages { get; set; }

    public string? Search { get; set; }

    public int? MinLikes { get; set; }

    public string SortBy { get; set; } = string.Empty;

    public string SortOrder { get; set; } = string.Empty;

    public List<PostResponse> Items { get; set; } = new();
}