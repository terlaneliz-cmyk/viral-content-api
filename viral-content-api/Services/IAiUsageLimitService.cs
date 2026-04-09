using ViralContentApi.DTOs;

namespace ViralContentApi.Services
{
    public interface IAiUsageLimitService
    {
        Task<AiUsageStatusResponse> TryConsumeAsync(int userId);
        Task<AiUsageStatusResponse> GetUsageAsync(int userId);
        Task<AiUsageStatusResponse> GetUsageStatusAsync(int userId);
        Task<object> GetUsageHistoryAsync(int userId, int days);
        Task<bool> CanUseAsync(int userId);
        Task RecordUsageAsync(int userId);
        Task<AiPlanUpdateResponse> UpdateUserPlanAsync(int userId, string plan);
    }
}