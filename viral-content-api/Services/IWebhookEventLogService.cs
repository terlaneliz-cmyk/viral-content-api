using ViralContentApi.DTOs;

namespace ViralContentApi.Services;

public interface IWebhookEventLogService
{
    Task LogAsync(string provider, BillingWebhookResponse response);
    Task<List<WebhookEventLogResponse>> GetRecentAsync(string? provider = null, int take = 50);
    Task<ClearWebhookEventLogsResponse> ClearAsync(string? provider = null, bool? success = null, int? olderThanDays = null);
    Task<WebhookEventLogStatsResponse> GetStatsAsync(string? provider = null);
}