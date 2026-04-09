using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ViralContentApi.DTOs;
using ViralContentApi.Services;

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

        public AiController(
            IAiContentService aiContentService,
            IContentService contentService,
            IAiUsageLimitService aiUsageLimitService)
        {
            _aiContentService = aiContentService;
            _contentService = contentService;
            _aiUsageLimitService = aiUsageLimitService;
        }

        [HttpPost("generate")]
        public async Task<ActionResult<GenerateContentResponse>> Generate([FromBody] GenerateContentRequest request)
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
                    remainingToday = usageResult.RemainingToday
                });
            }

            var response = await _aiContentService.GenerateAsync(request);
            return Ok(response);
        }

        [HttpPost("save-as-post")]
        public async Task<ActionResult<PostResponse>> SaveAsPost([FromBody] SaveGeneratedContentAsPostRequest request)
        {
            var userId = GetUserId();
            var savedPost = await _contentService.SaveGeneratedContentAsPostAsync(request, userId);
            return CreatedAtAction(nameof(GetHistory), new { id = savedPost.Id }, savedPost);
        }

        [HttpGet("history")]
        public async Task<ActionResult<IEnumerable<PostResponse>>> GetHistory()
        {
            var userId = GetUserId();
            var posts = await _contentService.GetMyPostsAsync(userId, aiOnly: true);
            return Ok(posts);
        }

        [HttpGet("usage")]
        public async Task<IActionResult> GetUsage()
        {
            var userId = GetUserId();
            var usage = await _aiUsageLimitService.GetUsageAsync(userId);

            return Ok(new
            {
                plan = usage.Plan,
                dailyLimit = usage.DailyLimit,
                usedToday = usage.UsedToday,
                remainingToday = usage.RemainingToday
            });
        }

        [HttpGet("usage/history")]
        public async Task<IActionResult> GetUsageHistory([FromQuery] int days = 30)
        {
            var userId = GetUserId();
            var usage = await _aiUsageLimitService.GetUsageAsync(userId);
            var history = await _aiUsageLimitService.GetUsageHistoryAsync(userId, days);

            return Ok(new
            {
                plan = usage.Plan,
                dailyLimit = usage.DailyLimit,
                days = Math.Min(Math.Max(days, 1), 365),
                items = history
            });
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
                    remainingToday = usageResult.RemainingToday
                });
            }

            var myAiPosts = await _contentService.GetMyPostsAsync(userId, aiOnly: true);
            var existingPost = myAiPosts.FirstOrDefault(x => x.Id == id);

            if (existingPost == null)
            {
                return NotFound(new { message = "AI history item not found." });
            }

            if (!existingPost.IsAiGenerated)
            {
                return BadRequest(new { message = "Only AI-generated posts can be regenerated." });
            }

            var request = new GenerateContentRequest
            {
                Topic = string.IsNullOrWhiteSpace(existingPost.Topic) ? existingPost.Title : existingPost.Topic,
                Platform = string.IsNullOrWhiteSpace(existingPost.Platform) ? "LinkedIn" : existingPost.Platform,
                Tone = string.IsNullOrWhiteSpace(existingPost.Tone) ? "confident" : existingPost.Tone,
                Goal = string.IsNullOrWhiteSpace(existingPost.Goal) ? "engagement" : existingPost.Goal,
                ContentType = string.IsNullOrWhiteSpace(existingPost.ContentType) ? "post" : existingPost.ContentType,
                TargetAudience = string.IsNullOrWhiteSpace(existingPost.TargetAudience) ? "general audience" : existingPost.TargetAudience,
                NumberOfVariants = existingPost.NumberOfVariants.HasValue && existingPost.NumberOfVariants.Value > 0
                    ? existingPost.NumberOfVariants.Value
                    : 3
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

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("Invalid or missing user ID claim.");
            }

            return userId;
        }
    }
}