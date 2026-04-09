using Microsoft.Extensions.Options;
using ViralContentApi.DTOs;
using ViralContentApi.Models;

namespace ViralContentApi.Services;

public class NotificationHealthService : INotificationHealthService
{
    private readonly IConfiguration _configuration;
    private readonly SendGridEmailSettings _sendGridSettings;

    public NotificationHealthService(
        IConfiguration configuration,
        IOptions<SendGridEmailSettings> sendGridSettings)
    {
        _configuration = configuration;
        _sendGridSettings = sendGridSettings.Value;
    }

    public Task<NotificationHealthResponse> GetHealthAsync()
    {
        var selectedProvider = _configuration["NotificationProvider:Provider"] ?? "Stub";

        var response = new NotificationHealthResponse
        {
            Provider = selectedProvider,
            IsHealthy = true,
            Message = "Notification provider health evaluated.",
            SendGrid = new SendGridNotificationHealthResponse
            {
                ApiKeyConfigured = !string.IsNullOrWhiteSpace(_sendGridSettings.ApiKey),
                FromEmailConfigured = !string.IsNullOrWhiteSpace(_sendGridSettings.FromEmail),
                FromNameConfigured = !string.IsNullOrWhiteSpace(_sendGridSettings.FromName),
                SandboxMode = _sendGridSettings.SandboxMode
            }
        };

        if (selectedProvider.Equals("SendGrid", StringComparison.OrdinalIgnoreCase))
        {
            response.IsHealthy =
                !string.IsNullOrWhiteSpace(_sendGridSettings.ApiKey) &&
                !string.IsNullOrWhiteSpace(_sendGridSettings.FromEmail);

            response.Message = response.IsHealthy
                ? "SendGrid configuration looks ready."
                : "SendGrid provider selected but required settings are missing.";
        }

        return Task.FromResult(response);
    }

    public Task<NotificationHealthResponse> GetHealth()
    {
        return GetHealthAsync();
    }
}