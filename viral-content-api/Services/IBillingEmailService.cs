using ViralContentApi.DTOs;

namespace ViralContentApi.Services;

public interface IBillingEmailService
{
    Task<BillingEmailNotificationResponse> SendAsync(BillingEmailNotificationRequest request);
}