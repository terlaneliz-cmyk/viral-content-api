using ViralContentApi.DTOs;

namespace ViralContentApi.Services;

public interface IProcessedWebhookEventService
{
    Task<bool> HasBeenProcessedAsync(string provider, string externalEventId);
    Task MarkAsProcessedAsync(string provider, string externalEventId);
    Task<List<ProcessedWebhookEventResponse>> GetRecentAsync(string? provider = null, int take = 50);
    Task<ClearProcessedWebhookEventsResponse> ClearAsync(string? provider = null, int? olderThanDays = null);
}