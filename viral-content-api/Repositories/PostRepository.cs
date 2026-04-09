using Microsoft.EntityFrameworkCore;
using ViralContentApi.Data;
using ViralContentApi.Models;

namespace ViralContentApi.Repositories;

public class PostRepository
{
    private readonly AppDbContext _context;

    public PostRepository(AppDbContext context)
    {
        _context = context;
    }

    public IEnumerable<Post> GetAll(
        int page = 1,
        int pageSize = 10,
        string? search = null,
        int? minLikes = null,
        string? sortBy = null,
        string? sortOrder = "desc")
    {
        if (page < 1)
            page = 1;

        if (pageSize < 1)
            pageSize = 10;

        IQueryable<Post> query = _context.Posts.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => p.Title.Contains(search));
        }

        if (minLikes.HasValue)
        {
            query = query.Where(p => p.Likes >= minLikes.Value);
        }

        var normalizedSortBy = sortBy?.Trim().ToLower();
        var normalizedSortOrder = sortOrder?.Trim().ToLower() == "asc" ? "asc" : "desc";

        query = normalizedSortBy switch
        {
            "title" => normalizedSortOrder == "asc"
                ? query.OrderBy(p => p.Title)
                : query.OrderByDescending(p => p.Title),

            "likes" => normalizedSortOrder == "asc"
                ? query.OrderBy(p => p.Likes)
                : query.OrderByDescending(p => p.Likes),

            "shares" => normalizedSortOrder == "asc"
                ? query.OrderBy(p => p.Shares)
                : query.OrderByDescending(p => p.Shares),

            "views" => normalizedSortOrder == "asc"
                ? query.OrderBy(p => p.Views)
                : query.OrderByDescending(p => p.Views),

            _ => normalizedSortOrder == "asc"
                ? query.OrderBy(p => p.CreatedAt)
                : query.OrderByDescending(p => p.CreatedAt)
        };

        return query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public int GetCount(string? search = null, int? minLikes = null)
    {
        IQueryable<Post> query = _context.Posts;

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => p.Title.Contains(search));
        }

        if (minLikes.HasValue)
        {
            query = query.Where(p => p.Likes >= minLikes.Value);
        }

        return query.Count();
    }

    public Post? GetById(int id)
    {
        return _context.Posts
            .AsNoTracking()
            .FirstOrDefault(p => p.Id == id);
    }

    public IEnumerable<Post> GetTrending()
    {
        return _context.Posts
            .AsNoTracking()
            .ToList()
            .OrderByDescending(p => p.ViralScore)
            .Take(10);
    }

    public Post Add(Post post)
    {
        post.CreatedAt = DateTime.UtcNow;

        _context.Posts.Add(post);
        _context.SaveChanges();

        return post;
    }

    public bool Update(Post post)
    {
        var existing = _context.Posts.FirstOrDefault(p => p.Id == post.Id);
        if (existing == null)
            return false;

        existing.Title = post.Title;
        existing.Url = post.Url;
        existing.Views = post.Views;
        existing.Likes = post.Likes;
        existing.Shares = post.Shares;

        _context.SaveChanges();
        return true;
    }

    public bool Delete(int id)
    {
        var post = _context.Posts.FirstOrDefault(p => p.Id == id);
        if (post == null)
            return false;

        _context.Posts.Remove(post);
        _context.SaveChanges();

        return true;
    }
}