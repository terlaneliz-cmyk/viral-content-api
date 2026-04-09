using Microsoft.Extensions.Options;
using ViralContentApi.DTOs;
using ViralContentApi.Models;

namespace ViralContentApi.Services;

public class WebhookMaintenanceService : IWebhookMaintenanceService
{
    private readonly IProcessedWebhookEventService _processedWebhookEventService;
    private readonly IWebhookEventLogService _webhookEventLogService;
    private readonly WebhookMaintenanceSettings _settings;
    private readonly ILogger<WebhookMaintenanceService> _logger;

    public WebhookMaintenanceService(
        IProcessedWebhookEventService processedWebhookEventService,
        IWebhookEventLogService webhookEventLogService,
        IOptions<WebhookMaintenanceSettings> settings,
        ILogger<WebhookMaintenanceService> logger)
    {
        _processedWebhookEventService = processedWebhookEventService;
        _webhookEventLogService = webhookEventLogService;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<WebhookMaintenanceRunResponse> RunCleanupAsync()
    {
        var processedRetentionDays = _settings.ProcessedWebhookEventsRetentionDays <= 0
            ? 30
            : _settings.ProcessedWebhookEventsRetentionDays;

        var logsRetentionDays = _settings.WebhookEventLogsRetentionDays <= 0
            ? 30
            : _settings.WebhookEventLogsRetentionDays;

        var processedResult = await _processedWebhookEventService.ClearAsync(
            provider: null,
            olderThanDays: processedRetentionDays);

        var logsResult = await _webhookEventLogService.ClearAsync(
            provider: null,
            success: null,
            olderThanDays: logsRetentionDays);

        var response = new WebhookMaintenanceRunResponse
        {
            DeletedProcessedWebhookEventsCount = processedResult.DeletedCount,
            DeletedWebhookEventLogsCount = logsResult.DeletedCount,
            ExecutedAtUtc = DateTime.UtcNow,
            Message = "Webhook maintenance cleanup completed."
        };

        _logger.LogInformation(
            "Webhook maintenance cleanup completed. DeletedProcessedWebhookEventsCount: {ProcessedCount}, DeletedWebhookEventLogsCount: {LogCount}",
            response.DeletedProcessedWebhookEventsCount,
            response.DeletedWebhookEventLogsCount);

        return response;
    }
}