using ViralContentApi.DTOs;

namespace ViralContentApi.Services;

public interface INotificationHealthService
{
    Task<NotificationHealthResponse> GetHealthAsync();
    Task<NotificationHealthResponse> GetHealth();
}