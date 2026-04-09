using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ViralContentApi.DTOs;
using ViralContentApi.Services;

namespace ViralContentApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContentController : ControllerBase
    {
        private readonly IContentService _contentService;

        public ContentController(IContentService contentService)
        {
            _contentService = contentService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedPostsResponse>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] int? minLikes = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortOrder = null,
            [FromQuery] bool? aiOnly = null)
        {
            if (page <= 0)
            {
                return BadRequest(new { message = "Page must be greater than 0." });
            }

            if (pageSize <= 0 || pageSize > 100)
            {
                return BadRequest(new { message = "PageSize must be between 1 and 100." });
            }

            if (minLikes.HasValue && minLikes.Value < 0)
            {
                return BadRequest(new { message = "MinLikes must be 0 or greater." });
            }

            var allowedSortByValues = new[] { "createdAt", "likes", "views", "shares", "viralScore", "title" };
            if (!string.IsNullOrWhiteSpace(sortBy) &&
                !allowedSortByValues.Contains(sortBy, StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    message = "SortBy must be one of: createdAt, likes, views, shares, viralScore, title."
                });
            }

            var allowedSortOrderValues = new[] { "asc", "desc" };
            if (!string.IsNullOrWhiteSpace(sortOrder) &&
                !allowedSortOrderValues.Contains(sortOrder, StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    message = "SortOrder must be either 'asc' or 'desc'."
                });
            }

            var result = await _contentService.GetAllAsync(search, minLikes, sortBy, sortOrder, page, pageSize, aiOnly);
            return Ok(result);
        }

        [HttpGet("mine")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<PostResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<PostResponse>>> GetMine([FromQuery] bool? aiOnly = null)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Invalid or missing user id in token." });
            }

            var posts = await _contentService.GetMyPostsAsync(userId, aiOnly);
            return Ok(posts);
        }

        [HttpGet("dashboard")]
        [Authorize]
        [ProducesResponseType(typeof(UserDashboardResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserDashboardResponse>> GetDashboard()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Invalid or missing user id in token." });
            }

            var dashboard = await _contentService.GetDashboardAsync(userId);
            return Ok(dashboard);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PostResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PostResponse>> GetById(int id)
        {
            var post = await _contentService.GetByIdAsync(id);

            if (post is null)
            {
                return NotFound(new { message = $"Post with id {id} was not found." });
            }

            return Ok(post);
        }

        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(PostResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<PostResponse>> Create([FromBody] CreatePostRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Invalid or missing user id in token." });
            }

            var createdPost = await _contentService.CreateAsync(request, userId);
            return CreatedAtAction(nameof(GetById), new { id = createdPost.Id }, createdPost);
        }

        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(PostResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PostResponse>> Update(int id, [FromBody] UpdatePostRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Invalid or missing user id in token." });
            }

            var updatedPost = await _contentService.UpdateAsync(id, request, userId);

            if (updatedPost is null)
            {
                return NotFound(new { message = $"Post with id {id} was not found or does not belong to you." });
            }

            return Ok(updatedPost);
        }

        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Invalid or missing user id in token." });
            }

            var deleted = await _contentService.DeleteAsync(id, userId);

            if (!deleted)
            {
                return NotFound(new { message = $"Post with id {id} was not found or does not belong to you." });
            }

            return NoContent();
        }

        [HttpGet("deleted")]
        public async Task<ActionResult<IEnumerable<PostResponse>>> GetDeleted()
        {
            var deletedPosts = await _contentService.GetDeletedAsync();
            return Ok(deletedPosts);
        }

        [HttpPost("{id}/restore")]
        public async Task<ActionResult<PostResponse>> Restore(int id)
        {
            await _contentService.RestoreAsync(id);

            var restoredPost = await _contentService.GetByIdAsync(id);

            if (restoredPost is null)
            {
                return NotFound(new { message = $"Post with id {id} was not found." });
            }

            return Ok(restoredPost);
        }

        [HttpGet("stats")]
        public async Task<ActionResult<ContentStatsResponse>> GetStats()
        {
            var stats = await _contentService.GetStatsAsync();
            return Ok(stats);
        }

        [HttpPost("{id}/recalculate-score")]
        public async Task<ActionResult<PostResponse>> RecalculateScore(int id)
        {
            var post = await _contentService.RecalculateScoreAsync(id);

            if (post is null)
            {
                return NotFound(new { message = $"Post with id {id} was not found." });
            }

            return Ok(post);
        }

        [HttpPost("{id}/boost")]
        public async Task<ActionResult<PostResponse>> Boost(int id, [FromBody] BoostPostRequest request)
        {
            var post = await _contentService.BoostAsync(id, request);

            if (post is null)
            {
                return NotFound(new { message = $"Post with id {id} was not found." });
            }

            return Ok(post);
        }

        [HttpGet("leaderboard")]
        public async Task<ActionResult<LeaderboardResponse>> GetLeaderboard([FromQuery] int count = 5)
        {
            if (count <= 0)
            {
                return BadRequest(new { message = "Count must be greater than 0." });
            }

            var leaderboard = await _contentService.GetLeaderboardAsync(count);
            return Ok(leaderboard);
        }
    }
}