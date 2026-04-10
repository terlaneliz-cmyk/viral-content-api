using Microsoft.EntityFrameworkCore;
using ViralContentApi.Data;
using ViralContentApi.DTOs;
using ViralContentApi.Models;

namespace ViralContentApi.Services;

public class UserSubscriptionService : IUserSubscriptionService
{
    private readonly AppDbContext _context;
    private readonly IAiUsageLimitService _aiUsageLimitService;

    public UserSubscriptionService(
        AppDbContext context,
        IAiUsageLimitService aiUsageLimitService)
    {
        _context = context;
        _aiUsageLimitService = aiUsageLimitService;
    }

    public async Task<UserSubscription?> GetByUserIdAsync(int userId)
    {
        return await _context.UserSubscriptions
            .FirstOrDefaultAsync(x => x.UserId == userId);
    }

    public async Task<UserSubscriptionResponse?> GetResponseByUserIdAsync(int userId)
    {
        var subscription = await GetByUserIdAsync(userId);
        return subscription == null ? null : MapToResponse(subscription);
    }

    public async Task<UserSubscriptionResponse?> GetMySubscriptionAsync(int userId)
    {
        return await GetResponseByUserIdAsync(userId);
    }

    public async Task<UserSubscriptionResponse> CreateOrUpdatePendingSubscriptionAsync(
        int userId,
        string planName,
        string billingCycle,
        string? externalSubscriptionId = null,
        string? externalCustomerId = null)
    {
        return await CreateOrUpdatePendingSubscriptionAsync(
            userId,
            planName,
            billingCycle,
            externalSubscriptionId,
            externalCustomerId,
            null,
            null);
    }

    public async Task<UserSubscriptionResponse> CreateOrUpdatePendingSubscriptionAsync(
        int userId,
        string planName,
        string billingCycle,
        decimal price,
        string currency,
        string stripePriceLookupKey,
        string externalCheckoutSessionId)
    {
        var normalizedPlanName = NormalizePlanName(planName);
        var normalizedBillingCycle = NormalizeBillingCycle(billingCycle);

        var subscription = await _context.UserSubscriptions
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (subscription == null)
        {
            subscription = new UserSubscription
            {
                UserId = userId,
                PlanName = normalizedPlanName,
                BillingCycle = normalizedBillingCycle,
                Status = "pending",
                Price = price,
                Currency = string.IsNullOrWhiteSpace(currency) ? "usd" : currency.Trim(),
                StripePriceLookupKey = stripePriceLookupKey ?? string.Empty,
                ExternalCheckoutSessionId = externalCheckoutSessionId ?? string.Empty,
                CancelAtPeriodEnd = false,
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.UserSubscriptions.Add(subscription);
        }
        else
        {
            subscription.PlanName = normalizedPlanName;
            subscription.BillingCycle = normalizedBillingCycle;
            subscription.Status = "pending";
            subscription.Price = price;
            subscription.Currency = string.IsNullOrWhiteSpace(currency) ? "usd" : currency.Trim();
            subscription.StripePriceLookupKey = stripePriceLookupKey ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(externalCheckoutSessionId))
            {
                subscription.ExternalCheckoutSessionId = externalCheckoutSessionId;
            }

            subscription.CancelAtPeriodEnd = false;
        }

        await _context.SaveChangesAsync();
        return MapToResponse(subscription);
    }

    public async Task<UserSubscriptionResponse> CreateOrUpdatePendingSubscriptionAsync(
        int userId,
        string planName,
        string billingCycle,
        string? externalSubscriptionId,
        string? externalCustomerId,
        DateTime? currentPeriodStartUtc,
        DateTime? currentPeriodEndUtc)
    {
        var normalizedPlanName = NormalizePlanName(planName);
        var normalizedBillingCycle = NormalizeBillingCycle(billingCycle);

        var subscription = await _context.UserSubscriptions
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (subscription == null)
        {
            subscription = new UserSubscription
            {
                UserId = userId,
                PlanName = normalizedPlanName,
                BillingCycle = normalizedBillingCycle,
                Status = "pending",
                CurrentPeriodStartUtc = currentPeriodStartUtc,
                CurrentPeriodEndUtc = currentPeriodEndUtc,
                CancelAtPeriodEnd = false,
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.UserSubscriptions.Add(subscription);
        }
        else
        {
            subscription.PlanName = normalizedPlanName;
            subscription.BillingCycle = normalizedBillingCycle;
            subscription.Status = "pending";

            if (currentPeriodStartUtc.HasValue)
            {
                subscription.CurrentPeriodStartUtc = currentPeriodStartUtc;
            }

            if (currentPeriodEndUtc.HasValue)
            {
                subscription.CurrentPeriodEndUtc = currentPeriodEndUtc;
            }
        }

        await _context.SaveChangesAsync();
        return MapToResponse(subscription);
    }

    public async Task<UserSubscriptionResponse?> ActivateSubscriptionAsync(
        int userId,
        string planName,
        string billingCycle = "monthly",
        string? externalSubscriptionId = null,
        string? externalCustomerId = null,
        DateTime? currentPeriodStartUtc = null,
        DateTime? currentPeriodEndUtc = null)
    {
        var normalizedPlanName = NormalizePlanName(planName);
        var normalizedBillingCycle = NormalizeBillingCycle(billingCycle);

        var subscription = await _context.UserSubscriptions
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (subscription == null)
        {
            subscription = new UserSubscription
            {
                UserId = userId,
                PlanName = normalizedPlanName,
                BillingCycle = normalizedBillingCycle,
                Status = "active",
                CurrentPeriodStartUtc = currentPeriodStartUtc,
                CurrentPeriodEndUtc = currentPeriodEndUtc,
                CancelAtPeriodEnd = false,
                ActivatedAtUtc = DateTime.UtcNow,
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.UserSubscriptions.Add(subscription);
        }
        else
        {
            subscription.PlanName = normalizedPlanName;
            subscription.BillingCycle = normalizedBillingCycle;
            subscription.Status = "active";

            if (currentPeriodStartUtc.HasValue)
            {
                subscription.CurrentPeriodStartUtc = currentPeriodStartUtc;
            }

            if (currentPeriodEndUtc.HasValue)
            {
                subscription.CurrentPeriodEndUtc = currentPeriodEndUtc;
            }

            subscription.CancelAtPeriodEnd = false;
            subscription.ActivatedAtUtc ??= DateTime.UtcNow;
        }

        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user != null)
        {
            user.Plan = normalizedPlanName;
            await _aiUsageLimitService.UpdateUserPlanAsync(userId, normalizedPlanName);
        }

        await _context.SaveChangesAsync();
        return MapToResponse(subscription);
    }

    public async Task<UserSubscriptionResponse?> RequestCancelAtPeriodEndAsync(int userId)
    {
        var subscription = await _context.UserSubscriptions
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (subscription == null)
        {
            return null;
        }

        subscription.CancelAtPeriodEnd = true;
        subscription.CancelRequestedAtUtc = DateTime.UtcNow;

        if (string.Equals(subscription.Status, "active", StringComparison.OrdinalIgnoreCase))
        {
            subscription.Status = "canceling";
        }

        await _context.SaveChangesAsync();
        return MapToResponse(subscription);
    }

    public async Task<UserSubscriptionResponse?> CancelSubscriptionImmediatelyAsync(int userId)
    {
        var subscription = await _context.UserSubscriptions
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (subscription == null)
        {
            return null;
        }

        subscription.Status = "canceled";
        subscription.CancelAtPeriodEnd = false;
        subscription.CurrentPeriodEndUtc = DateTime.UtcNow;

        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user != null)
        {
            user.Plan = "Free";
            await _aiUsageLimitService.UpdateUserPlanAsync(userId, "Free");
        }

        await _context.SaveChangesAsync();
        return MapToResponse(subscription);
    }

    public async Task<int> ProcessExpiredSubscriptionsAsync()
    {
        return await ProcessExpirationsAsync();
    }

    public async Task<int> ProcessExpirationsAsync()
    {
        var now = DateTime.UtcNow;

        var subscriptions = await _context.UserSubscriptions
            .Where(x =>
                x.CurrentPeriodEndUtc.HasValue &&
                x.CurrentPeriodEndUtc.Value <= now &&
                (
                    x.Status == "active" ||
                    x.Status == "trialing" ||
                    x.Status == "canceling" ||
                    x.CancelAtPeriodEnd
                ))
            .ToListAsync();

        if (subscriptions.Count == 0)
        {
            return 0;
        }

        var userIds = subscriptions.Select(x => x.UserId).Distinct().ToList();

        var users = await _context.Users
            .Where(x => userIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id);

        foreach (var subscription in subscriptions)
        {
            subscription.Status = "expired";
            subscription.CancelAtPeriodEnd = false;

            if (users.TryGetValue(subscription.UserId, out var user))
            {
                user.Plan = "Free";
                await _aiUsageLimitService.UpdateUserPlanAsync(user.Id, "Free");
            }
        }

        await _context.SaveChangesAsync();
        return subscriptions.Count;
    }

    public async Task<UserSubscriptionResponse?> SyncSubscriptionStatusAsync(
        int userId,
        string status,
        string? externalSubscriptionId,
        DateTime? currentPeriodStartUtc,
        DateTime? currentPeriodEndUtc)
    {
        return await SyncSubscriptionDetailsAsync(
            userId,
            status,
            externalSubscriptionId,
            null,
            null,
            null,
            currentPeriodStartUtc,
            currentPeriodEndUtc,
            null,
            null,
            null,
            null,
            null,
            null);
    }

    public async Task<UserSubscriptionResponse?> SyncSubscriptionDetailsAsync(
        int userId,
        string status,
        string? externalSubscriptionId,
        string? externalCustomerId,
        string? planName,
        string? billingCycle,
        DateTime? currentPeriodStartUtc,
        DateTime? currentPeriodEndUtc,
        bool? cancelAtPeriodEnd,
        DateTime? canceledAtUtc,
        string? externalCheckoutSessionId = null,
        decimal? price = null,
        string? currency = null,
        string? stripePriceLookupKey = null)
    {
        var normalizedStatus = string.IsNullOrWhiteSpace(status)
            ? "unknown"
            : status.Trim();

        var normalizedPlanName = string.IsNullOrWhiteSpace(planName)
            ? null
            : NormalizePlanName(planName);

        var normalizedBillingCycle = string.IsNullOrWhiteSpace(billingCycle)
            ? null
            : NormalizeBillingCycle(billingCycle);

        var subscription = await _context.UserSubscriptions
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (subscription == null)
        {
            subscription = new UserSubscription
            {
                UserId = userId,
                Status = normalizedStatus,
                PlanName = normalizedPlanName ?? "Free",
                BillingCycle = normalizedBillingCycle ?? "monthly",
                ExternalCheckoutSessionId = externalCheckoutSessionId ?? string.Empty,
                Price = price ?? 0,
                Currency = string.IsNullOrWhiteSpace(currency) ? string.Empty : currency.Trim(),
                StripePriceLookupKey = stripePriceLookupKey ?? string.Empty,
                CurrentPeriodStartUtc = currentPeriodStartUtc,
                CurrentPeriodEndUtc = currentPeriodEndUtc,
                CancelAtPeriodEnd = cancelAtPeriodEnd ?? false,
                ActivatedAtUtc = IsActivatedLikeStatus(normalizedStatus) ? DateTime.UtcNow : null,
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.UserSubscriptions.Add(subscription);
        }
        else
        {
            subscription.Status = normalizedStatus;

            if (!string.IsNullOrWhiteSpace(normalizedPlanName))
            {
                subscription.PlanName = normalizedPlanName;
            }

            if (!string.IsNullOrWhiteSpace(normalizedBillingCycle))
            {
                subscription.BillingCycle = normalizedBillingCycle;
            }

            if (!string.IsNullOrWhiteSpace(externalCheckoutSessionId))
            {
                subscription.ExternalCheckoutSessionId = externalCheckoutSessionId;
            }

            if (price.HasValue)
            {
                subscription.Price = price.Value;
            }

            if (!string.IsNullOrWhiteSpace(currency))
            {
                subscription.Currency = currency.Trim();
            }

            if (!string.IsNullOrWhiteSpace(stripePriceLookupKey))
            {
                subscription.StripePriceLookupKey = stripePriceLookupKey;
            }

            if (currentPeriodStartUtc.HasValue)
            {
                subscription.CurrentPeriodStartUtc = currentPeriodStartUtc;
            }

            if (currentPeriodEndUtc.HasValue)
            {
                subscription.CurrentPeriodEndUtc = currentPeriodEndUtc;
            }

            if (cancelAtPeriodEnd.HasValue)
            {
                subscription.CancelAtPeriodEnd = cancelAtPeriodEnd.Value;
            }

            if (subscription.CancelAtPeriodEnd && !subscription.CancelRequestedAtUtc.HasValue)
            {
                subscription.CancelRequestedAtUtc = DateTime.UtcNow;
            }

            if (IsActivatedLikeStatus(normalizedStatus))
            {
                subscription.ActivatedAtUtc ??= DateTime.UtcNow;
            }

            if (IsCanceledLikeStatus(normalizedStatus) && canceledAtUtc.HasValue && !subscription.CurrentPeriodEndUtc.HasValue)
            {
                subscription.CurrentPeriodEndUtc = canceledAtUtc;
            }
        }

        var effectivePlanName = !string.IsNullOrWhiteSpace(normalizedPlanName)
            ? normalizedPlanName
            : subscription.PlanName;

        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);

        if (user != null && IsPaidOrActiveLikeStatus(normalizedStatus) && !string.IsNullOrWhiteSpace(effectivePlanName))
        {
            user.Plan = effectivePlanName;
        }

        if (user != null && IsFreeDowngradeStatus(normalizedStatus))
        {
            user.Plan = "Free";
        }

        await _context.SaveChangesAsync();
        return MapToResponse(subscription);
    }

    private static UserSubscriptionResponse MapToResponse(UserSubscription subscription)
    {
        return new UserSubscriptionResponse
        {
            UserId = subscription.UserId,
            PlanName = subscription.PlanName,
            BillingCycle = subscription.BillingCycle,
            Status = subscription.Status,
            Price = subscription.Price,
            Currency = subscription.Currency,
            StripePriceLookupKey = subscription.StripePriceLookupKey,
            ExternalCheckoutSessionId = subscription.ExternalCheckoutSessionId,
            CreatedAtUtc = subscription.CreatedAtUtc,
            ActivatedAtUtc = subscription.ActivatedAtUtc,
            CurrentPeriodStartUtc = subscription.CurrentPeriodStartUtc,
            CurrentPeriodEndUtc = subscription.CurrentPeriodEndUtc,
            CancelAtPeriodEnd = subscription.CancelAtPeriodEnd,
            CancelRequestedAtUtc = subscription.CancelRequestedAtUtc
        };
    }

    private static bool IsActivatedLikeStatus(string status)
    {
        return status.Equals("active", StringComparison.OrdinalIgnoreCase) ||
               status.Equals("trialing", StringComparison.OrdinalIgnoreCase) ||
               status.Equals("past_due", StringComparison.OrdinalIgnoreCase) ||
               status.Equals("canceling", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsPaidOrActiveLikeStatus(string status)
    {
        return status.Equals("active", StringComparison.OrdinalIgnoreCase) ||
               status.Equals("trialing", StringComparison.OrdinalIgnoreCase) ||
               status.Equals("canceling", StringComparison.OrdinalIgnoreCase) ||
               status.Equals("past_due", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsCanceledLikeStatus(string status)
    {
        return status.Equals("canceled", StringComparison.OrdinalIgnoreCase) ||
               status.Equals("cancelled", StringComparison.OrdinalIgnoreCase) ||
               status.Equals("expired", StringComparison.OrdinalIgnoreCase) ||
               status.Equals("unpaid", StringComparison.OrdinalIgnoreCase) ||
               status.Equals("incomplete_expired", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsFreeDowngradeStatus(string status)
    {
        return IsCanceledLikeStatus(status);
    }

    private static string NormalizePlanName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "Free";
        }

        var normalized = value.Trim();

        return normalized.ToLowerInvariant() switch
        {
            "free" => "Free",
            "pro" => "Pro",
            "agency" => "Agency",
            _ => normalized
        };
    }

    private static string NormalizeBillingCycle(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "monthly";
        }

        var normalized = value.Trim().ToLowerInvariant();

        return normalized switch
        {
            "month" => "monthly",
            "monthly" => "monthly",
            "year" => "yearly",
            "yearly" => "yearly",
            "annual" => "yearly",
            "annually" => "yearly",
            _ => normalized
        };
    }
}