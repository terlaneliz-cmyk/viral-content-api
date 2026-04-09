using ViralContentApi.DTOs;

namespace ViralContentApi.Services
{
    public interface IBillingEventLogService
    {
        Task LogAsync(
            int? userId,
            string eventType,
            string status,
            string? externalCheckoutSessionId,
            string payloadJson,
            bool success,
            string message);

        Task<List<BillingEventLogResponse>> GetRecentAsync(int count = 100);
    }
}