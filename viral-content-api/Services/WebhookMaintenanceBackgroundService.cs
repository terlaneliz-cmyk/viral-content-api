using Microsoft.Extensions.Options;
using ViralContentApi.Models;

namespace ViralContentApi.Services;

public class WebhookMaintenanceBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly WebhookMaintenanceSettings _settings;
    private readonly ILogger<WebhookMaintenanceBackgroundService> _logger;

    public WebhookMaintenanceBackgroundService(
        IServiceProvider serviceProvider,
        IOptions<WebhookMaintenanceSettings> settings,
        ILogger<WebhookMaintenanceBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _settings = settings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_settings.Enabled)
        {
            _logger.LogInformation("Webhook maintenance background service is disabled.");
            return;
        }

        var intervalMinutes = _settings.RunIntervalMinutes <= 0
            ? 60
            : _settings.RunIntervalMinutes;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var maintenanceService = scope.ServiceProvider.GetRequiredService<IWebhookMaintenanceService>();

                await maintenanceService.RunCleanupAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Webhook maintenance background cleanup failed.");
            }

            await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), stoppingToken);
        }
    }
}