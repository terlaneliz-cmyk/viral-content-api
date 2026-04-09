using Microsoft.EntityFrameworkCore;
using ViralContentApi.Data;
using ViralContentApi.DTOs;
using ViralContentApi.Models;

namespace ViralContentApi.Services;

public class WebhookEventLogService : IWebhookEventLogService
{
    private readonly AppDbContext _context;

    public WebhookEventLogService(AppDbContext context)
    {
        _context = context;
    }

    public async Task LogAsync(string provider, BillingWebhookResponse response)
    {
        var log = new WebhookEventLog
        {
            Provider = provider ?? string.Empty,
            EventType = response.EventType ?? string.Empty,
            ExternalEventId = response.ExternalEventId ?? string.Empty,
            Success = response.Success,
            Message = response.Message ?? string.Empty,
            UserId = response.UserId,
            CustomerId = response.CustomerId ?? string.Empty,
            SubscriptionId = response.SubscriptionId ?? string.Empty,
            CheckoutSessionId = response.CheckoutSessionId ?? string.Empty,
            SubscriptionStatus = response.SubscriptionStatus ?? string.Empty,
            PlanName = response.PlanName ?? string.Empty,
            BillingCycle = response.BillingCycle ?? string.Empty,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.WebhookEventLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    public async Task<List<WebhookEventLogResponse>> GetRecentAsync(string? provider = null, int take = 50)
    {
        if (take <= 0)
        {
            take = 50;
        }

        if (take > 200)
        {
            take = 200;
        }

        IQueryable<WebhookEventLog> query = _context.WebhookEventLogs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(provider))
        {
            query = query.Where(x => x.Provider == provider);
        }

        return await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(take)
            .Select(x => new WebhookEventLogResponse
            {
                Id = x.Id,
                Provider = x.Provider,
                EventType = x.EventType,
                ExternalEventId = x.ExternalEventId,
                Success = x.Success,
                Message = x.Message,
                UserId = x.UserId,
                CustomerId = x.CustomerId,
                SubscriptionId = x.SubscriptionId,
                CheckoutSessionId = x.CheckoutSessionId,
                SubscriptionStatus = x.SubscriptionStatus,
                PlanName = x.PlanName,
                BillingCycle = x.BillingCycle,
                CreatedAtUtc = x.CreatedAtUtc
            })
            .ToListAsync();
    }

    public async Task<ClearWebhookEventLogsResponse> ClearAsync(string? provider = null, bool? success = null, int? olderThanDays = null)
    {
        IQueryable<WebhookEventLog> query = _context.WebhookEventLogs;

        if (!string.IsNullOrWhiteSpace(provider))
        {
            query = query.Where(x => x.Provider == provider);
        }

        if (success.HasValue)
        {
            query = query.Where(x => x.Success == success.Value);
        }

        if (olderThanDays.HasValue && olderThanDays.Value > 0)
        {
            var cutoffUtc = DateTime.UtcNow.AddDays(-olderThanDays.Value);
            query = query.Where(x => x.CreatedAtUtc < cutoffUtc);
        }

        var items = await query.ToListAsync();

        if (items.Count == 0)
        {
            return new ClearWebhookEventLogsResponse
            {
                DeletedCount = 0,
                Message = "No webhook event logs matched the filter."
            };
        }

        _context.WebhookEventLogs.RemoveRange(items);
        await _context.SaveChangesAsync();

        return new ClearWebhookEventLogsResponse
        {
            DeletedCount = items.Count,
            Message = "Webhook event logs cleared successfully."
        };
    }

    public async Task<WebhookEventLogStatsResponse> GetStatsAsync(string? provider = null)
    {
        IQueryable<WebhookEventLog> query = _context.WebhookEventLogs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(provider))
        {
            query = query.Where(x => x.Provider == provider);
        }

        var nowUtc = DateTime.UtcNow;
        var sinceUtc = nowUtc.AddHours(-24);

        return new WebhookEventLogStatsResponse
        {
            Provider = provider,
            TotalCount = await query.CountAsync(),
            SuccessCount = await query.CountAsync(x => x.Success),
            FailureCount = await query.CountAsync(x => !x.Success),
            StripeCount = await _context.WebhookEventLogs.AsNoTracking().CountAsync(x => x.Provider == "Stripe"),
            Recent24HoursCount = await query.CountAsync(x => x.CreatedAtUtc >= sinceUtc)
        };
    }
}