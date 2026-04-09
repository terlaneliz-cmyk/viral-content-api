using Microsoft.EntityFrameworkCore;
using ViralContentApi.Data;
using ViralContentApi.DTOs;
using ViralContentApi.Models;

namespace ViralContentApi.Services;

public class ContentService : IContentService
{
    private readonly AppDbContext _context;

    public ContentService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedPostsResponse> GetAllAsync(
        string? search,
        int? minLikes,
        string? sortBy,
        string? sortOrder,
        int page,
        int pageSize,
        bool? aiOnly = null)
    {
        var query = _context.Posts
            .Include(p => p.User)
            .Where(p => !p.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();

            query = query.Where(p =>
                p.Title.ToLower().Contains(term) ||
                p.Url.ToLower().Contains(term) ||
                (p.Topic != null && p.Topic.ToLower().Contains(term)) ||
                (p.Platform != null && p.Platform.ToLower().Contains(term)) ||
                (p.Hook != null && p.Hook.ToLower().Contains(term)) ||
                (p.Content != null && p.Content.ToLower().Contains(term)));
        }

        if (minLikes.HasValue)
        {
            query = query.Where(p => p.Likes >= minLikes.Value);
        }

        if (aiOnly.HasValue)
        {
            query = query.Where(p => p.IsAiGenerated == aiOnly.Value);
        }

        query = ApplySorting(query, sortBy, sortOrder);

        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : pageSize;

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var posts = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedPostsResponse
        {
            Items = posts.Select(MapToPostResponse).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages
        };
    }

    public async Task<IEnumerable<PostResponse>> GetMyPostsAsync(int userId, bool? aiOnly = null)
    {
        var query = _context.Posts
            .Include(p => p.User)
            .Where(p => p.UserId == userId && !p.IsDeleted)
            .AsQueryable();

        if (aiOnly.HasValue)
        {
            query = query.Where(p => p.IsAiGenerated == aiOnly.Value);
        }

        var posts = await query
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return posts.Select(MapToPostResponse);
    }

    public async Task<UserDashboardResponse> GetDashboardAsync(int userId)
    {
        var posts = await _context.Posts
            .Include(p => p.User)
            .Where(p => p.UserId == userId && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        var username = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => u.Username)
            .FirstOrDefaultAsync() ?? string.Empty;

        var topPost = posts
            .OrderByDescending(p => p.ViralScore)
            .FirstOrDefault();

        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);

        return new UserDashboardResponse
        {
            Username = username,
            TotalPosts = posts.Count,
            TotalViews = posts.Sum(p => p.Views),
            TotalLikes = posts.Sum(p => p.Likes),
            TotalShares = posts.Sum(p => p.Shares),
            AverageViralScore = posts.Count == 0 ? 0 : posts.Average(p => p.ViralScore),
            PostsLast7Days = posts.Count(p => p.CreatedAt >= sevenDaysAgo),
            TopPost = topPost == null ? null : MapToTopPostResponse(topPost),
            RecentPosts = posts
                .Take(5)
                .Select(MapToPostResponse)
                .ToList()
        };
    }

    public async Task<IEnumerable<PostResponse>> GetDeletedAsync()
    {
        var posts = await _context.Posts
            .Include(p => p.User)
            .Where(p => p.IsDeleted)
            .OrderByDescending(p => p.DeletedAt)
            .ThenByDescending(p => p.CreatedAt)
            .ToListAsync();

        return posts.Select(MapToPostResponse);
    }

    public async Task<PostResponse?> GetByIdAsync(int id)
    {
        var post = await _context.Posts
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null)
        {
            return null;
        }

        return MapToPostResponse(post);
    }

    public async Task<PostResponse> CreateAsync(CreatePostRequest request, int userId)
    {
        var post = new Post
        {
            Title = request.Title,
            Url = request.Url,
            Views = request.Views,
            Likes = request.Likes,
            Shares = request.Shares,
            CreatedAt = request.CreatedAt == default ? DateTime.UtcNow : request.CreatedAt,
            ScoreBoost = 0,
            ViralScore = CalculateViralScore(request.Views, request.Likes, request.Shares, 0),
            IsDeleted = false,
            DeletedAt = null,
            UserId = userId,
            IsAiGenerated = false
        };

        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        var savedPost = await _context.Posts
            .Include(p => p.User)
            .FirstAsync(p => p.Id == post.Id);

        return MapToPostResponse(savedPost);
    }

    public async Task<PostResponse> SaveGeneratedContentAsPostAsync(SaveGeneratedContentAsPostRequest request, int userId)
    {
        var hashtags = request.Hashtags ?? new List<string>();

        var post = new Post
        {
            Title = request.Title,
            Url = request.Url,
            Views = request.Views,
            Likes = request.Likes,
            Shares = request.Shares,
            CreatedAt = DateTime.UtcNow,
            ScoreBoost = request.ScoreBoost,
            ViralScore = CalculateViralScore(request.Views, request.Likes, request.Shares, request.ScoreBoost),
            IsDeleted = false,
            DeletedAt = null,
            UserId = userId,

            Topic = request.Topic,
            Platform = request.Platform,
            Hook = request.Hook,
            Content = request.Content,
            CallToAction = request.CallToAction,
            Hashtags = string.Join(",", hashtags),
            IsAiGenerated = true,

            Tone = request.Tone,
            Goal = request.Goal,
            ContentType = request.ContentType,
            TargetAudience = request.TargetAudience,
            NumberOfVariants = request.NumberOfVariants
        };

        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        var savedPost = await _context.Posts
            .Include(p => p.User)
            .FirstAsync(p => p.Id == post.Id);

        return MapToPostResponse(savedPost);
    }

    public async Task<PostResponse?> ApplyGeneratedVariantAsync(int id, ApplyGeneratedVariantRequest request, int userId)
    {
        var post = await _context.Posts
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        if (post == null)
        {
            return null;
        }

        if (post.UserId != userId)
        {
            return null;
        }

        post.Title = !string.IsNullOrWhiteSpace(request.Title) ? request.Title : post.Title;
        post.Topic = !string.IsNullOrWhiteSpace(request.Topic) ? request.Topic : post.Topic;
        post.Platform = !string.IsNullOrWhiteSpace(request.Platform) ? request.Platform : post.Platform;

        post.Hook = request.Hook;
        post.Content = request.Content;
        post.CallToAction = request.CallToAction;
        post.Hashtags = string.Join(",", request.Hashtags ?? new List<string>());
        post.IsAiGenerated = true;

        post.Tone = !string.IsNullOrWhiteSpace(request.Tone) ? request.Tone : post.Tone;
        post.Goal = !string.IsNullOrWhiteSpace(request.Goal) ? request.Goal : post.Goal;
        post.ContentType = !string.IsNullOrWhiteSpace(request.ContentType) ? request.ContentType : post.ContentType;
        post.TargetAudience = !string.IsNullOrWhiteSpace(request.TargetAudience) ? request.TargetAudience : post.TargetAudience;

        await _context.SaveChangesAsync();

        return MapToPostResponse(post);
    }

    public async Task<PostResponse?> UpdateAsync(int id, UpdatePostRequest request, int userId)
    {
        var post = await _context.Posts
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        if (post == null)
        {
            return null;
        }

        if (post.UserId != userId)
        {
            return null;
        }

        post.Title = request.Title;
        post.Url = request.Url;
        post.Views = request.Views;
        post.Likes = request.Likes;
        post.Shares = request.Shares;
        post.CreatedAt = request.CreatedAt;
        post.ViralScore = CalculateViralScore(post.Views, post.Likes, post.Shares, post.ScoreBoost);

        await _context.SaveChangesAsync();

        return MapToPostResponse(post);
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        var post = await _context.Posts
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        if (post == null)
        {
            return false;
        }

        if (post.UserId != userId)
        {
            return false;
        }

        post.IsDeleted = true;
        post.DeletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<PostResponse?> RestoreAsync(int id)
    {
        var post = await _context.Posts
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id && p.IsDeleted);

        if (post == null)
        {
            return null;
        }

        post.IsDeleted = false;
        post.DeletedAt = null;

        await _context.SaveChangesAsync();
        return MapToPostResponse(post);
    }

    public async Task<PostResponse?> RecalculateScoreAsync(int id)
    {
        var post = await _context.Posts
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null)
        {
            return null;
        }

        post.ViralScore = CalculateViralScore(post.Views, post.Likes, post.Shares, post.ScoreBoost);

        await _context.SaveChangesAsync();
        return MapToPostResponse(post);
    }

    public async Task<PostResponse?> BoostAsync(int id, BoostPostRequest request)
    {
        var post = await _context.Posts
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null)
        {
            return null;
        }

        post.ScoreBoost += request.Amount;
        post.ViralScore = CalculateViralScore(post.Views, post.Likes, post.Shares, post.ScoreBoost);

        await _context.SaveChangesAsync();
        return MapToPostResponse(post);
    }

    public async Task<ContentStatsResponse> GetStatsAsync()
    {
        var posts = await _context.Posts
            .Include(p => p.User)
            .Where(p => !p.IsDeleted)
            .ToListAsync();

        var topPost = posts
            .OrderByDescending(p => p.ViralScore)
            .FirstOrDefault();

        return new ContentStatsResponse
        {
            TotalPosts = posts.Count,
            TotalViews = posts.Sum(p => p.Views),
            TotalLikes = posts.Sum(p => p.Likes),
            TotalShares = posts.Sum(p => p.Shares),
            AverageViralScore = posts.Count == 0 ? 0 : posts.Average(p => p.ViralScore),
            TopPost = topPost == null ? null : MapToTopPostResponse(topPost)
        };
    }

    public async Task<LeaderboardResponse> GetLeaderboardAsync(int count)
    {
        count = count <= 0 ? 10 : count;

        var posts = await _context.Posts
            .Include(p => p.User)
            .Where(p => !p.IsDeleted)
            .OrderByDescending(p => p.ViralScore)
            .ThenByDescending(p => p.CreatedAt)
            .Take(count)
            .ToListAsync();

        return new LeaderboardResponse
        {
            Count = posts.Count,
            Posts = posts.Select(MapToTopPostResponse).ToList()
        };
    }

    private static IQueryable<Post> ApplySorting(IQueryable<Post> query, string? sortBy, string? sortOrder)
    {
        var normalizedSortBy = sortBy?.Trim().ToLowerInvariant();
        var descending = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);

        return normalizedSortBy switch
        {
            "title" => descending ? query.OrderByDescending(p => p.Title) : query.OrderBy(p => p.Title),
            "views" => descending ? query.OrderByDescending(p => p.Views) : query.OrderBy(p => p.Views),
            "likes" => descending ? query.OrderByDescending(p => p.Likes) : query.OrderBy(p => p.Likes),
            "shares" => descending ? query.OrderByDescending(p => p.Shares) : query.OrderBy(p => p.Shares),
            "viralscore" => descending ? query.OrderByDescending(p => p.ViralScore) : query.OrderBy(p => p.ViralScore),
            "createdat" => descending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };
    }

    private static double CalculateViralScore(int views, int likes, int shares, double scoreBoost)
    {
        return (views * 0.1) + (likes * 1.5) + (shares * 2.5) + scoreBoost;
    }

    private static TopPostResponse MapToTopPostResponse(Post post)
    {
        return new TopPostResponse
        {
            Id = post.Id,
            Title = post.Title,
            Url = post.Url,
            Views = post.Views,
            Likes = post.Likes,
            Shares = post.Shares,
            CreatedAt = post.CreatedAt,
            ViralScore = post.ViralScore,
            Username = post.User?.Username
        };
    }

    private static PostResponse MapToPostResponse(Post post)
    {
        return new PostResponse
        {
            Id = post.Id,
            Title = post.Title,
            Url = post.Url,
            Views = post.Views,
            Likes = post.Likes,
            Shares = post.Shares,
            ViralScore = post.ViralScore,
            ScoreBoost = post.ScoreBoost,
            CreatedAt = post.CreatedAt,
            IsDeleted = post.IsDeleted,
            DeletedAt = post.DeletedAt,
            Username = post.User?.Username ?? string.Empty,

            Topic = post.Topic,
            Platform = post.Platform,
            Hook = post.Hook,
            Content = post.Content,
            CallToAction = post.CallToAction,
            Hashtags = string.IsNullOrWhiteSpace(post.Hashtags)
                ? new List<string>()
                : post.Hashtags
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToList(),
            IsAiGenerated = post.IsAiGenerated,

            Tone = post.Tone,
            Goal = post.Goal,
            ContentType = post.ContentType,
            TargetAudience = post.TargetAudience,
            NumberOfVariants = post.NumberOfVariants
        };
    }
}