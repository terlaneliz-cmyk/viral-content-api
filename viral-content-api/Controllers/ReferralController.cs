using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ViralContentApi.Data;
using ViralContentApi.DTOs;
using ViralContentApi.Models;
using ViralContentApi.Services;

namespace ViralContentApi.Controllers;

[ApiController]
[Route("api/referrals")]
[Authorize]
public class ReferralController : ControllerBase
{
    private const int ReferralTrialThreshold = 3;

    private readonly AppDbContext _context;
    private readonly IUserSubscriptionService _userSubscriptionService;

    public ReferralController(
        AppDbContext context,
        IUserSubscriptionService userSubscriptionService)
    {
        _context = context;
        _userSubscriptionService = userSubscriptionService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyReferralInfo()
    {
        var userId = GetUserId();
        if (userId <= 0)
        {
            return Unauthorized(new { message = "User id not found in token." });
        }

        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        if (string.IsNullOrWhiteSpace(user.ReferralCode))
        {
            user.ReferralCode = GenerateReferralCode(user.Email, user.Id);
            user.ReferralCodeCreatedAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        var referredByEmail = await _context.Users
            .Where(x => x.Id == user.ReferredByUserId)
            .Select(x => x.Email)
            .FirstOrDefaultAsync();

        var hasEarnedReferralTrial = await HasEarnedReferralTrialAsync(userId);
        var rank = await GetRankForUserAsync(userId);
        var now = DateTime.UtcNow;

        return Ok(new
        {
            referralCode = user.ReferralCode,
            referralInviteCount = user.ReferralInviteCount,
            referralSignupCount = user.ReferralSignupCount,
            referredByUserId = user.ReferredByUserId,
            referredByEmail,
            referralLink = $"{Request.Scheme}://{Request.Host}/?ref={Uri.EscapeDataString(user.ReferralCode)}",
            referralTrialThreshold = ReferralTrialThreshold,
            hasEarnedReferralTrial,
            currentUserRank = rank,
            isReferralTrialActive = user.ReferralTrialEndsAtUtc.HasValue && user.ReferralTrialEndsAtUtc.Value > now,
            referralTrialEndsAtUtc = user.ReferralTrialEndsAtUtc
        });
    }

    [HttpGet("leaderboard")]
    [AllowAnonymous]
    public async Task<IActionResult> GetLeaderboard()
    {
        var leaderboardUsers = await _context.Users
            .AsNoTracking()
            .Where(x => x.ReferralSignupCount > 0)
            .OrderByDescending(x => x.ReferralSignupCount)
            .ThenByDescending(x => x.ReferralInviteCount)
            .ThenBy(x => x.Email)
            .Take(50)
            .ToListAsync();

        var data = leaderboardUsers
            .Select((x, index) => new
            {
                userId = x.Id,
                email = x.Email,
                referrals = x.ReferralSignupCount,
                shares = x.ReferralInviteCount,
                rank = index + 1,
                shareToSignupRatio = x.ReferralInviteCount <= 0
                    ? 0
                    : Math.Round((double)x.ReferralSignupCount / x.ReferralInviteCount * 100, 1)
            })
            .ToList();

        return Ok(data);
    }

    [HttpGet("my-rank")]
    public async Task<IActionResult> GetMyRank()
    {
        var userId = GetUserId();
        if (userId <= 0)
        {
            return Unauthorized(new { message = "User id not found in token." });
        }

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userId);

        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        var rank = await GetRankForUserAsync(userId);

        return Ok(new
        {
            userId = user.Id,
            email = user.Email,
            rank,
            referrals = user.ReferralSignupCount,
            shares = user.ReferralInviteCount
        });
    }

    [HttpGet("admin/stats")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetStats()
    {
        var now = DateTime.UtcNow;

        var totalUsers = await _context.Users.CountAsync();
        var totalInvites = await _context.Users.SumAsync(x => x.ReferralInviteCount);
        var totalSignups = await _context.Users.SumAsync(x => x.ReferralSignupCount);

        var rewardUsers = await _context.Users
            .CountAsync(x => x.ReferralSignupCount >= ReferralTrialThreshold);

        var trialActiveUsers = await _context.Users
            .CountAsync(x => x.ReferralTrialEndsAtUtc.HasValue && x.ReferralTrialEndsAtUtc.Value > now);

        var rewardClaimedUsers = await _context.BillingEventLogs
            .AsNoTracking()
            .Where(x =>
                x.EventType == "referral_reward_trial_granted" &&
                x.Success)
            .Select(x => x.UserId)
            .Distinct()
            .CountAsync();

        var conversionRate = totalInvites <= 0
            ? 0
            : Math.Round((double)totalSignups / totalInvites * 100, 1);

        var rewardClaimRate = totalUsers <= 0
            ? 0
            : Math.Round((double)rewardClaimedUsers / totalUsers * 100, 1);

        var top = await _context.Users
            .AsNoTracking()
            .Where(x => x.ReferralInviteCount > 0 || x.ReferralSignupCount > 0)
            .OrderByDescending(x => x.ReferralSignupCount)
            .ThenByDescending(x => x.ReferralInviteCount)
            .ThenBy(x => x.Email)
            .Take(10)
            .Select(x => new
            {
                x.Email,
                x.ReferralSignupCount,
                x.ReferralInviteCount
            })
            .ToListAsync();

        var topWithRatios = top.Select(x => new
        {
            x.Email,
            x.ReferralSignupCount,
            x.ReferralInviteCount,
            ShareToSignupRatio = x.ReferralInviteCount <= 0
                ? 0
                : Math.Round((double)x.ReferralSignupCount / x.ReferralInviteCount * 100, 1)
        });

        return Ok(new
        {
            totalUsers,
            totalInvites,
            totalSignups,
            rewardUsers,
            trialActiveUsers,
            rewardClaimedUsers,
            conversionRate,
            rewardClaimRate,
            top = topWithRatios
        });
    }

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateOrRefreshCode()
    {
        var userId = GetUserId();
        if (userId <= 0)
        {
            return Unauthorized(new { message = "User id not found in token." });
        }

        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        user.ReferralCode = GenerateReferralCode(user.Email, user.Id);
        user.ReferralCodeCreatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            referralCode = user.ReferralCode,
            referralLink = $"{Request.Scheme}://{Request.Host}/?ref={Uri.EscapeDataString(user.ReferralCode)}"
        });
    }

    [HttpPost("apply")]
    public async Task<IActionResult> ApplyReferralCode([FromBody] ApplyReferralRequest request)
    {
        var userId = GetUserId();
        if (userId <= 0)
        {
            return Unauthorized(new { message = "User id not found in token." });
        }

        if (request == null || string.IsNullOrWhiteSpace(request.Code))
        {
            return BadRequest(new { message = "Referral code is required." });
        }

        var normalizedCode = request.Code.Trim().ToUpperInvariant();

        var currentUser = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (currentUser == null)
        {
            return NotFound(new { message = "User not found." });
        }

        if (currentUser.ReferredByUserId.HasValue)
        {
            return BadRequest(new { message = "Referral code already applied." });
        }

        var referrer = await _context.Users.FirstOrDefaultAsync(x => x.ReferralCode == normalizedCode);
        if (referrer == null)
        {
            return NotFound(new { message = "Referral code not found." });
        }

        if (referrer.Id == currentUser.Id)
        {
            return BadRequest(new { message = "You cannot use your own referral code." });
        }

        currentUser.ReferredByUserId = referrer.Id;
        referrer.ReferralSignupCount += 1;

        _context.BillingEventLogs.Add(new BillingEventLog
        {
            UserId = currentUser.Id,
            EventType = "referral_code_applied",
            Status = "success",
            Success = true,
            Message = $"Referral code applied: {normalizedCode}",
            Metadata = $"ReferrerUserId={referrer.Id}; ReferralCode={normalizedCode}",
            CreatedAtUtc = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        UserSubscriptionResponse? rewardTrial = null;

        if (referrer.ReferralSignupCount >= ReferralTrialThreshold)
        {
            rewardTrial = await _userSubscriptionService.ActivateReferralRewardTrialAsync(referrer.Id);
        }

        return Ok(new
        {
            message = rewardTrial is not null
                ? "Referral code applied successfully. Referrer earned a free Pro trial."
                : "Referral code applied successfully.",
            referrerEmail = referrer.Email,
            referrerTrialRewardGranted = rewardTrial is not null
        });
    }

    [HttpPost("track-share")]
    public async Task<IActionResult> TrackShare()
    {
        var userId = GetUserId();
        if (userId <= 0)
        {
            return Unauthorized(new { message = "User id not found in token." });
        }

        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        user.ReferralInviteCount += 1;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Referral share tracked." });
    }

    private async Task<bool> HasEarnedReferralTrialAsync(int userId)
    {
        return await _context.BillingEventLogs
            .AsNoTracking()
            .AnyAsync(x =>
                x.UserId == userId &&
                x.EventType == "referral_reward_trial_granted" &&
                x.Success);
    }

    private async Task<int> GetRankForUserAsync(int userId)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userId);

        if (user == null)
        {
            return 0;
        }

        var higherCount = await _context.Users
            .AsNoTracking()
            .CountAsync(x =>
                x.ReferralSignupCount > user.ReferralSignupCount ||
                (x.ReferralSignupCount == user.ReferralSignupCount && x.ReferralInviteCount > user.ReferralInviteCount) ||
                (x.ReferralSignupCount == user.ReferralSignupCount && x.ReferralInviteCount == user.ReferralInviteCount && string.CompareOrdinal(x.Email, user.Email) < 0));

        return higherCount + 1;
    }

    private int GetUserId()
    {
        var userIdRaw =
            User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            User.FindFirstValue("sub");

        return int.TryParse(userIdRaw, out var userId) ? userId : 0;
    }

    private static string GenerateReferralCode(string email, int userId)
    {
        var basePart = (email ?? "user")
            .Split('@')[0]
            .ToUpperInvariant()
            .Where(char.IsLetterOrDigit)
            .Take(8)
            .ToArray();

        var prefix = new string(basePart);
        if (string.IsNullOrWhiteSpace(prefix))
        {
            prefix = "USER";
        }

        var randomPart = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
        return $"{prefix}{userId}{randomPart}";
    }

    public class ApplyReferralRequest
    {
        public string Code { get; set; } = "";
    }
}