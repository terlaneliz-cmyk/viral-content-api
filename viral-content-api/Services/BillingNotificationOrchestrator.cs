using ViralContentApi.DTOs;

namespace ViralContentApi.Services;

public class BillingNotificationOrchestrator
{
    private readonly IBillingEmailService _billingEmailService;
    private readonly INotificationTemplateService _templateService;
    private readonly ILogger<BillingNotificationOrchestrator> _logger;

    public BillingNotificationOrchestrator(
        IBillingEmailService billingEmailService,
        INotificationTemplateService templateService,
        ILogger<BillingNotificationOrchestrator> logger)
    {
        _billingEmailService = billingEmailService;
        _templateService = templateService;
        _logger = logger;
    }

    public async Task<BillingEmailNotificationResponse> SendAsync(BillingEmailNotificationRequest request)
    {
        var result = await _billingEmailService.SendAsync(request);

        if (result.Success)
        {
            _logger.LogInformation(
                "Billing notification processed successfully via {Provider}. To: {ToEmail}, EventType: {EventType}",
                result.Provider,
                request.ToEmail,
                request.EventType);
        }
        else
        {
            _logger.LogWarning(
                "Billing notification failed via {Provider}. To: {ToEmail}, EventType: {EventType}, Message: {Message}",
                result.Provider,
                request.ToEmail,
                request.EventType,
                result.Message);
        }

        return result;
    }

    public Task<BillingEmailNotificationResponse> SendSubscriptionActivatedAsync(
        string toEmail,
        string? toName,
        string plan,
        DateTime? expiresAtUtc,
        int? userId = null)
    {
        var request = _templateService.BuildSubscriptionActivatedEmail(
            toEmail,
            toName,
            plan,
            expiresAtUtc,
            userId);

        return SendAsync(request);
    }

    public Task<BillingEmailNotificationResponse> SendCancelRequestedAsync(
        string toEmail,
        string? toName,
        string plan,
        DateTime? endsAtUtc,
        int? userId = null)
    {
        var request = _templateService.BuildCancelRequestedEmail(
            toEmail,
            toName,
            plan,
            endsAtUtc,
            userId);

        return SendAsync(request);
    }

    public Task<BillingEmailNotificationResponse> SendCancelledAsync(
        string toEmail,
        string? toName,
        string plan,
        int? userId = null)
    {
        var request = _templateService.BuildCancelledEmail(
            toEmail,
            toName,
            plan,
            userId);

        return SendAsync(request);
    }

    public Task<BillingEmailNotificationResponse> SendExpiredAsync(
        string toEmail,
        string? toName,
        string plan,
        int? userId = null)
    {
        var request = _templateService.BuildExpiredEmail(
            toEmail,
            toName,
            plan,
            userId);

        return SendAsync(request);
    }

    public Task<BillingEmailNotificationResponse> SendPastDueAsync(
        string toEmail,
        string? toName,
        string plan,
        int? userId = null)
    {
        var request = _templateService.BuildPastDueEmail(
            toEmail,
            toName,
            plan,
            userId);

        return SendAsync(request);
    }

    public Task<BillingEmailNotificationResponse> SendTestAsync(
        string toEmail,
        string? toName = null,
        string? subject = null,
        string? message = null,
        int? userId = null)
    {
        var request = _templateService.BuildTestEmail(
            toEmail,
            toName,
            subject,
            message,
            userId);

        return SendAsync(request);
    }

    public Task<BillingEmailNotificationResponse> SendSubscriptionActivatedAsync(
        string toEmail,
        string? toName)
    {
        return SendSubscriptionActivatedAsync(toEmail, toName, "current", null, null);
    }

    public Task<BillingEmailNotificationResponse> SendCancelRequestedAsync(
        string toEmail,
        string? toName)
    {
        return SendCancelRequestedAsync(toEmail, toName, "current", null, null);
    }

    public Task<BillingEmailNotificationResponse> SendCancelledAsync(
        string toEmail,
        string? toName)
    {
        return SendCancelledAsync(toEmail, toName, "current", null);
    }

    public Task<BillingEmailNotificationResponse> SendExpiredAsync(
        string toEmail,
        string? toName)
    {
        return SendExpiredAsync(toEmail, toName, "current", null);
    }

    public Task<BillingEmailNotificationResponse> SendPastDueAsync(
        string toEmail,
        string? toName)
    {
        return SendPastDueAsync(toEmail, toName, "current", null);
    }

    public Task<BillingEmailNotificationResponse> SendSubscriptionActivatedAsync(
        string toEmail,
        string? toName,
        DateTime? expiresAtUtc,
        int? userId = null)
    {
        return SendSubscriptionActivatedAsync(toEmail, toName, "current", expiresAtUtc, userId);
    }

    public Task<BillingEmailNotificationResponse> SendCancelRequestedAsync(
        string toEmail,
        string? toName,
        DateTime? endsAtUtc,
        int? userId = null)
    {
        return SendCancelRequestedAsync(toEmail, toName, "current", endsAtUtc, userId);
    }

    public Task<BillingEmailNotificationResponse> SendCancelledAsync(
        string toEmail,
        string? toName,
        int? userId = null)
    {
        return SendCancelledAsync(toEmail, toName, "current", userId);
    }

    public Task<BillingEmailNotificationResponse> SendExpiredAsync(
        string toEmail,
        string? toName,
        int? userId = null)
    {
        return SendExpiredAsync(toEmail, toName, "current", userId);
    }

    public Task<BillingEmailNotificationResponse> SendPastDueAsync(
        string toEmail,
        string? toName,
        int? userId = null)
    {
        return SendPastDueAsync(toEmail, toName, "current", userId);
    }

    public Task<BillingEmailNotificationResponse> SendSubscriptionActivatedAsync(
        int userId,
        string toEmail)
    {
        return SendSubscriptionActivatedAsync(toEmail, null, "current", null, userId);
    }

    public Task<BillingEmailNotificationResponse> SendSubscriptionActivatedAsync(
        int userId,
        string toEmail,
        string? toName)
    {
        return SendSubscriptionActivatedAsync(toEmail, toName, "current", null, userId);
    }

    public Task<BillingEmailNotificationResponse> SendSubscriptionActivatedAsync(
        int userId,
        string toEmail,
        string? toName,
        DateTime? expiresAtUtc)
    {
        return SendSubscriptionActivatedAsync(toEmail, toName, "current", expiresAtUtc, userId);
    }

    public Task<BillingEmailNotificationResponse> SendCancelRequestedAsync(
        int userId,
        string toEmail)
    {
        return SendCancelRequestedAsync(toEmail, null, "current", null, userId);
    }

    public Task<BillingEmailNotificationResponse> SendCancelRequestedAsync(
        int userId,
        string toEmail,
        string? toName)
    {
        return SendCancelRequestedAsync(toEmail, toName, "current", null, userId);
    }

    public Task<BillingEmailNotificationResponse> SendCancelRequestedAsync(
        int userId,
        string toEmail,
        string? toName,
        DateTime? endsAtUtc)
    {
        return SendCancelRequestedAsync(toEmail, toName, "current", endsAtUtc, userId);
    }

    public Task<BillingEmailNotificationResponse> SendCancelledAsync(
        int userId,
        string toEmail)
    {
        return SendCancelledAsync(toEmail, null, "current", userId);
    }

    public Task<BillingEmailNotificationResponse> SendCancelledAsync(
        int userId,
        string toEmail,
        string? toName)
    {
        return SendCancelledAsync(toEmail, toName, "current", userId);
    }

    public Task<BillingEmailNotificationResponse> SendExpiredAsync(
        int userId,
        string toEmail)
    {
        return SendExpiredAsync(toEmail, null, "current", userId);
    }

    public Task<BillingEmailNotificationResponse> SendExpiredAsync(
        int userId,
        string toEmail,
        string? toName)
    {
        return SendExpiredAsync(toEmail, toName, "current", userId);
    }

    public Task<BillingEmailNotificationResponse> SendPastDueAsync(
        int userId,
        string toEmail)
    {
        return SendPastDueAsync(toEmail, null, "current", userId);
    }

    public Task<BillingEmailNotificationResponse> SendPastDueAsync(
        int userId,
        string toEmail,
        string? toName)
    {
        return SendPastDueAsync(toEmail, toName, "current", userId);
    }
}