using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ViralContentApi.DTOs;
using ViralContentApi.Services;
using ViralContentApi.Data;

namespace ViralContentApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AiController : ControllerBase
    {
        private readonly IAiContentService _aiContentService;
        private readonly IContentService _contentService;
        private readonly IAiUsageLimitService _aiUsageLimitService;
        private readonly AppDbContext _context;

        public AiController(
            IAiContentService aiContentService,
            IContentService contentService,
            IAiUsageLimitService aiUsageLimitService,
            AppDbContext context)
        {
            _aiContentService = aiContentService;
            _contentService = contentService;
            _aiUsageLimitService = aiUsageLimitService;
            _context = context;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] GenerateContentRequest request)
        {
            if (request == null)
                return BadRequest(new { message = "Request is null." });

            var userId = GetUserId();

            var usageResult = await _aiUsageLimitService.TryConsumeAsync(userId);

            Response.Headers["X-AI-Plan"] = usageResult.Plan;
            Response.Headers["X-AI-Daily-Limit"] = usageResult.DailyLimit.ToString();
            Response.Headers["X-AI-Remaining-Today"] = usageResult.RemainingToday.ToString();

            if (!usageResult.Allowed)
            {
                return StatusCode(429, new
                {
                    message = "Daily AI generation limit reached.",
                    plan = usageResult.Plan,
                    dailyLimit = usageResult.DailyLimit,
                    usedToday = usageResult.UsedToday,
                    remainingToday = usageResult.RemainingToday,
                    referralBonus = usageResult.ReferralBonus
                });
            }

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
            {
                return Unauthorized(new { message = "User not found." });
            }

            var today = DateTime.UtcNow.Date;

            if (user.LastActiveDateUtc == today.AddDays(-1))
            {
                user.CurrentStreak++;
            }
            else if (user.LastActiveDateUtc != today)
            {
                user.CurrentStreak = 1;
            }

            user.LastActiveDateUtc = today;

            if (user.CurrentStreak > user.BestStreak)
            {
                user.BestStreak = user.CurrentStreak;
            }

            await _context.SaveChangesAsync();

            try
            {
                var result = await _aiContentService.GenerateAsync(request);

                if (result == null)
                    return BadRequest(new { message = "AI generation returned null." });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error generating content.",
                    details = ex.Message
                });
            }
        }

        [HttpPost("save-as-post")]
        public async Task<ActionResult<PostResponse>> SaveAsPost([FromBody] SaveGeneratedContentAsPostRequest request)
        {
            var userId = GetUserId();
            var savedPost = await _contentService.SaveGeneratedContentAsPostAsync(request, userId);
            return CreatedAtAction(nameof(GetHistory), new { id = savedPost.Id }, savedPost);
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            var userId = GetUserId();

            var history = await _context.GeneratedContents
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new
                {
                    x.Id,
                    x.Topic,
                    x.Platform,
                    x.Tone,
                    x.CreatedAt,
                    x.Content
                })
                .ToListAsync();

            return Ok(history);
        }

        [HttpGet("usage")]
        public async Task<IActionResult> GetUsage()
        {
            var userId = GetUserId();
            var usage = await _aiUsageLimitService.GetUsageAsync(userId);
            var streak = await BuildStreakAsync(userId);

            return Ok(new
            {
                plan = usage.Plan,
                dailyLimit = usage.DailyLimit,
                usedToday = usage.UsedToday,
                remainingToday = usage.RemainingToday,
                referralBonus = usage.ReferralBonus,
                streak
            });
        }

        [HttpGet("usage/history")]
        public async Task<IActionResult> GetUsageHistory([FromQuery] int days = 30)
        {
            var userId = GetUserId();
            var usage = await _aiUsageLimitService.GetUsageAsync(userId);
            var history = await _aiUsageLimitService.GetUsageHistoryAsync(userId, days);
            var streak = await BuildStreakAsync(userId);

            return Ok(new
            {
                plan = usage.Plan,
                dailyLimit = usage.DailyLimit,
                referralBonus = usage.ReferralBonus,
                days = Math.Min(Math.Max(days, 1), 365),
                items = history,
                streak
            });
        }

        [HttpGet("streak")]
        public async Task<IActionResult> GetStreak()
        {
            var userId = GetUserId();
            var streak = await BuildStreakAsync(userId);
            return Ok(streak);
        }

        [HttpPost("{id}/regenerate")]
        public async Task<ActionResult<GenerateContentResponse>> Regenerate(int id)
        {
            var userId = GetUserId();

            var usageResult = await _aiUsageLimitService.TryConsumeAsync(userId);

            Response.Headers["X-AI-Plan"] = usageResult.Plan;
            Response.Headers["X-AI-Daily-Limit"] = usageResult.DailyLimit.ToString();
            Response.Headers["X-AI-Remaining-Today"] = usageResult.RemainingToday.ToString();

            if (!usageResult.Allowed)
            {
                return StatusCode(429, new
                {
                    message = "Daily AI generation limit reached.",
                    plan = usageResult.Plan,
                    dailyLimit = usageResult.DailyLimit,
                    usedToday = usageResult.UsedToday,
                    remainingToday = usageResult.RemainingToday,
                    referralBonus = usageResult.ReferralBonus
                });
            }

            var existingItem = await _context.GeneratedContents
                .Where(x => x.UserId == userId && x.Id == id)
                .FirstOrDefaultAsync();

            if (existingItem == null)
                return NotFound(new { message = "AI history item not found." });

            var request = new GenerateContentRequest
            {
                Topic = string.IsNullOrWhiteSpace(existingItem.Topic) ? "content creation" : existingItem.Topic,
                Platform = string.IsNullOrWhiteSpace(existingItem.Platform) ? "LinkedIn" : existingItem.Platform,
                Tone = string.IsNullOrWhiteSpace(existingItem.Tone) ? "confident" : existingItem.Tone,
                Goal = "engagement",
                ContentType = "post",
                TargetAudience = "general audience",
                NumberOfVariants = 3
            };

            var response = await _aiContentService.GenerateAsync(request);
            return Ok(response);
        }

        [HttpPut("{id}/apply-variant")]
        public async Task<ActionResult<PostResponse>> ApplyVariant(int id, [FromBody] ApplyGeneratedVariantRequest request)
        {
            var userId = GetUserId();
            var updatedPost = await _contentService.ApplyGeneratedVariantAsync(id, request, userId);
            return Ok(updatedPost);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAiHistoryItem(int id)
        {
            var userId = GetUserId();
            await _contentService.DeleteAsync(id, userId);
            return NoContent();
        }

        private async Task<object> BuildStreakAsync(int userId)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
            {
                return new
                {
                    currentStreak = 0,
                    bestStreak = 0,
                    activeToday = false,
                    lastUsageDateUtc = (DateTime?)null
                };
            }

            var today = DateTime.UtcNow.Date;
            var lastActiveDate = user.LastActiveDateUtc?.Date;

            return new
            {
                currentStreak = user.CurrentStreak,
                bestStreak = user.BestStreak,
                activeToday = lastActiveDate == today,
                lastUsageDateUtc = user.LastActiveDateUtc
            };
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                throw new UnauthorizedAccessException("Invalid or missing user ID claim.");

            return userId;
        }
    }
}