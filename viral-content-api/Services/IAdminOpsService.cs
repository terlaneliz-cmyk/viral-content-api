using ViralContentApi.DTOs;

namespace ViralContentApi.Services;

public interface IAdminOpsService
{
    Task<AdminOpsSummaryResponse> GetSummaryAsync(int recentWebhookTake = 20);
}