using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ViralContentApi.Data;
using ViralContentApi.Models;

namespace ViralContentApi.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromQuery] string? search)
    {
        var query = _context.Users
            .Include(u => u.UserSubscriptions)
            .Include(u => u.AiUsageRecords)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var lowered = search.Trim().ToLower();

            query = query.Where(x =>
                x.Email.ToLower().Contains(lowered) ||
                x.Username.ToLower().Contains(lowered));
        }

        var users = await query
            .OrderByDescending(x => x.CreatedAt)
            .Select(u => new
            {
                u.Id,
                u.Email,
                u.Username,
                u.Plan,
                u.Role,
                u.IsActive,
                u.CreatedAt,
                DailyUsage = u.AiUsageRecords.Count(),
                Subscription = u.UserSubscriptions
                    .OrderByDescending(s => s.CreatedAtUtc)
                    .Select(s => new
                    {
                        s.PlanName,
                        s.Status,
                        s.CancelAtPeriodEnd
                    })
                    .FirstOrDefault()
            })
            .ToListAsync();

        return Ok(users);
    }

    [HttpGet("analytics")]
    public async Task<IActionResult> GetAnalytics()
    {
        var totalUsers = await _context.Users.CountAsync();
        var free = await _context.Users.CountAsync(x => x.Plan == "Free");
        var pro = await _context.Users.CountAsync(x => x.Plan == "Pro");
        var creator = await _context.Users.CountAsync(x => x.Plan == "Creator");

        var totalGenerations = await _context.AiUsageRecords.CountAsync();
        var mrr = (pro * 19) + (creator * 49);

        var canceling = await _context.UserSubscriptions
            .CountAsync(x => x.CancelAtPeriodEnd);

        return Ok(new
        {
            totalUsers,
            free,
            pro,
            creator,
            todayGenerations = totalGenerations,
            mrr,
            churnSignals = canceling
        });
    }

    [HttpPost("set-plan")]
    public async Task<IActionResult> SetPlan([FromQuery] int userId, [FromQuery] string plan)
    {
        if (userId <= 0 || string.IsNullOrWhiteSpace(plan))
        {
            return BadRequest(new { message = "Valid userId and plan are required." });
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        user.Plan = plan.Trim();

        _context.BillingEventLogs.Add(new BillingEventLog
        {
            UserId = userId,
            EventType = "admin_set_plan",
            Status = "success",
            Success = true,
            Message = $"Plan changed to {user.Plan}",
            Metadata = $"Plan changed to {user.Plan}",
            CreatedAtUtc = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return Ok(new { message = "Plan updated." });
    }

    [HttpPost("reset-usage")]
    public async Task<IActionResult> ResetUsage([FromQuery] int userId)
    {
        if (userId <= 0)
        {
            return BadRequest(new { message = "Valid userId is required." });
        }

        var records = _context.AiUsageRecords.Where(x => x.UserId == userId);
        _context.AiUsageRecords.RemoveRange(records);

        _context.BillingEventLogs.Add(new BillingEventLog
        {
            UserId = userId,
            EventType = "admin_reset_usage",
            Status = "success",
            Success = true,
            Message = "Usage reset",
            Metadata = "All AI usage records removed by admin",
            CreatedAtUtc = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return Ok(new { message = "Usage reset." });
    }

    [HttpPost("deactivate")]
    public async Task<IActionResult> Deactivate([FromQuery] int userId)
    {
        if (userId <= 0)
        {
            return BadRequest(new { message = "Valid userId is required." });
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        user.IsActive = false;

        _context.BillingEventLogs.Add(new BillingEventLog
        {
            UserId = userId,
            EventType = "admin_deactivate_user",
            Status = "success",
            Success = true,
            Message = "User deactivated",
            Metadata = "User account deactivated by admin",
            CreatedAtUtc = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return Ok(new { message = "User deactivated." });
    }

    [HttpGet("webhooks")]
public async Task<IActionResult> GetWebhookLogs(
    [FromQuery] string? type,
    [FromQuery] bool? processed)
{
    var query = _context.WebhookEventLogs.AsQueryable();

    if (!string.IsNullOrWhiteSpace(type))
    {
        var t = type.ToLower();
        query = query.Where(x => x.EventType.ToLower().Contains(t));
    }

    if (processed.HasValue)
    {
        query = query.Where(x => x.Processed == processed.Value);
    }

    var logs = await query
        .OrderByDescending(x => x.CreatedAtUtc)
        .Take(200)
        .ToListAsync();

    return Ok(logs);
}

[HttpGet("charts")]
public async Task<IActionResult> GetCharts()
{
    var usersByPlan = await _context.Users
        .GroupBy(x => x.Plan)
        .Select(g => new
        {
            plan = g.Key,
            count = g.Count()
        })
        .ToListAsync();

    var usageByUser = await _context.AiUsageRecords
        .GroupBy(x => x.UserId)
        .Select(g => new
        {
            userId = g.Key,
            count = g.Count()
        })
        .OrderByDescending(x => x.count)
        .Take(10)
        .ToListAsync();

    return Ok(new
    {
        usersByPlan,
        topUsersByUsage = usageByUser
    });
}

[HttpPost("webhooks/retry")]
public async Task<IActionResult> RetryWebhook([FromQuery] int id)
{
    var log = await _context.WebhookEventLogs.FindAsync(id);

    if (log == null)
        return NotFound(new { message = "Webhook not found." });

    log.Processed = false;
    log.Success = false;
    log.Message = "Marked for retry by admin";

    await _context.SaveChangesAsync();

    return Ok(new { message = "Webhook marked for retry." });
}


    [HttpGet("billing-log")]
    public async Task<IActionResult> GetBillingLogs()
    {
        var logs = await _context.BillingEventLogs
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(100)
            .ToListAsync();

        return Ok(logs);
    }
}