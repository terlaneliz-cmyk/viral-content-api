using Microsoft.Extensions.Options;
using ViralContentApi.Models;

namespace ViralContentApi.Services;

public class SubscriptionExpirationBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly SubscriptionProcessingSettings _settings;
    private readonly ILogger<SubscriptionExpirationBackgroundService> _logger;

    public SubscriptionExpirationBackgroundService(
        IServiceProvider serviceProvider,
        IOptions<SubscriptionProcessingSettings> options,
        ILogger<SubscriptionExpirationBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _settings = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_settings.Enabled)
        {
            _logger.LogInformation("Subscription expiration background service is disabled.");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var subscriptionService = scope.ServiceProvider.GetRequiredService<IUserSubscriptionService>();

                var processedCount = await subscriptionService.ProcessExpiredSubscriptionsAsync();

                if (processedCount > 0)
                {
                    _logger.LogInformation(
                        "Subscription expiration background service processed {ProcessedCount} expired subscriptions.",
                        processedCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Subscription expiration background service failed.");
            }

            await Task.Delay(TimeSpan.FromMinutes(60), stoppingToken);
        }
    }
}