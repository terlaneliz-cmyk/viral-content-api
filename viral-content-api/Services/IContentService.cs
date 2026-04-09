using ViralContentApi.DTOs;

namespace ViralContentApi.Services;

public interface IContentService
{
    Task<PagedPostsResponse> GetAllAsync(
        string? search,
        int? minLikes,
        string? sortBy,
        string? sortOrder,
        int page,
        int pageSize,
        bool? aiOnly = null);

    Task<IEnumerable<PostResponse>> GetMyPostsAsync(int userId, bool? aiOnly = null);
    Task<UserDashboardResponse> GetDashboardAsync(int userId);
    Task<IEnumerable<PostResponse>> GetDeletedAsync();
    Task<PostResponse?> GetByIdAsync(int id);
    Task<PostResponse> CreateAsync(CreatePostRequest request, int userId);
    Task<PostResponse> SaveGeneratedContentAsPostAsync(SaveGeneratedContentAsPostRequest request, int userId);
    Task<PostResponse?> ApplyGeneratedVariantAsync(int id, ApplyGeneratedVariantRequest request, int userId);
    Task<PostResponse?> UpdateAsync(int id, UpdatePostRequest request, int userId);
    Task<bool> DeleteAsync(int id, int userId);
    Task<PostResponse?> RestoreAsync(int id);
    Task<PostResponse?> RecalculateScoreAsync(int id);
    Task<PostResponse?> BoostAsync(int id, BoostPostRequest request);
    Task<ContentStatsResponse> GetStatsAsync();
    Task<LeaderboardResponse> GetLeaderboardAsync(int count);
}