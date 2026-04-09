using ViralContentApi.DTOs;

namespace ViralContentApi.Services;

public class StubBillingEmailService : IBillingEmailService
{
    private readonly ILogger<StubBillingEmailService> _logger;

    public StubBillingEmailService(ILogger<StubBillingEmailService> logger)
    {
        _logger = logger;
    }

    public Task<BillingEmailNotificationResponse> SendAsync(BillingEmailNotificationRequest request)
    {
        _logger.LogInformation(
            "STUB billing email sent. To: {ToEmail}, Subject: {Subject}, EventType: {EventType}, UserId: {UserId}",
            request.ToEmail,
            request.Subject,
            request.EventType,
            request.UserId);

        return Task.FromResult(new BillingEmailNotificationResponse
        {
            Success = true,
            Provider = "Stub",
            Message = "Stub email logged successfully."
        });
    }
}