using ViralContentApi.DTOs;
using ViralContentApi.Models;

namespace ViralContentApi.Services;

public interface IUserSubscriptionService
{
    Task<UserSubscription?> GetByUserIdAsync(int userId);
    Task<UserSubscriptionResponse?> GetResponseByUserIdAsync(int userId);
    Task<UserSubscriptionResponse?> GetMySubscriptionAsync(int userId);

    Task<UserSubscriptionResponse> CreateOrUpdatePendingSubscriptionAsync(
        int userId,
        string planName,
        string billingCycle,
        string? externalSubscriptionId = null,
        string? externalCustomerId = null);

    Task<UserSubscriptionResponse> CreateOrUpdatePendingSubscriptionAsync(
        int userId,
        string planName,
        string billingCycle,
        decimal price,
        string currency,
        string stripePriceLookupKey,
        string externalCheckoutSessionId);

    Task<UserSubscriptionResponse> CreateOrUpdatePendingSubscriptionAsync(
        int userId,
        string planName,
        string billingCycle,
        string? externalSubscriptionId,
        string? externalCustomerId,
        DateTime? currentPeriodStartUtc,
        DateTime? currentPeriodEndUtc);

    Task<UserSubscriptionResponse?> ActivateSubscriptionAsync(
        int userId,
        string planName,
        string billingCycle = "monthly",
        string? externalSubscriptionId = null,
        string? externalCustomerId = null,
        DateTime? currentPeriodStartUtc = null,
        DateTime? currentPeriodEndUtc = null);

    Task<UserSubscriptionResponse?> ActivateReferralRewardTrialAsync(int userId);

    Task<UserSubscriptionResponse?> RequestCancelAtPeriodEndAsync(int userId);
    Task<UserSubscriptionResponse?> ReactivateSubscriptionAsync(int userId);
    Task<UserSubscriptionResponse?> CancelSubscriptionImmediatelyAsync(int userId);

    Task<int> ProcessExpiredSubscriptionsAsync();
    Task<int> ProcessExpirationsAsync();

    Task<UserSubscriptionResponse?> SyncSubscriptionStatusAsync(
        int userId,
        string status,
        string? externalSubscriptionId,
        DateTime? currentPeriodStartUtc,
        DateTime? currentPeriodEndUtc);

    Task<UserSubscriptionResponse?> SyncSubscriptionDetailsAsync(
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
        string? stripePriceLookupKey = null);
}