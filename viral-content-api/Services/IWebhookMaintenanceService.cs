using ViralContentApi.DTOs;

namespace ViralContentApi.Services;

public interface IWebhookMaintenanceService
{
    Task<WebhookMaintenanceRunResponse> RunCleanupAsync();
}