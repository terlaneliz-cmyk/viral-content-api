using ViralContentApi.DTOs;

namespace ViralContentApi.Services;

public interface INotificationTemplateService
{
    BillingEmailNotificationRequest BuildSubscriptionActivatedEmail(
        string toEmail,
        string? toName,
        string plan,
        DateTime? expiresAtUtc,
        int? userId = null);

    BillingEmailNotificationRequest BuildCancelRequestedEmail(
        string toEmail,
        string? toName,
        string plan,
        DateTime? endsAtUtc,
        int? userId = null);

    BillingEmailNotificationRequest BuildCancelledEmail(
        string toEmail,
        string? toName,
        string plan,
        int? userId = null);

    BillingEmailNotificationRequest BuildExpiredEmail(
        string toEmail,
        string? toName,
        string plan,
        int? userId = null);

    BillingEmailNotificationRequest BuildPastDueEmail(
        string toEmail,
        string? toName,
        string plan,
        int? userId = null);

    BillingEmailNotificationRequest BuildSubscriptionDowngradedEmail(
        string toEmail,
        string? toName,
        string oldPlan,
        string newPlan,
        int? userId = null);

    BillingEmailNotificationRequest BuildTestEmail(
        string toEmail,
        string? toName,
        string? subject = null,
        string? message = null,
        int? userId = null);

    BillingEmailNotificationRequest BuildByType(
        string templateType,
        string toEmail,
        string? toName,
        string? plan = null,
        string? oldPlan = null,
        string? newPlan = null,
        DateTime? effectiveDateUtc = null,
        string? subject = null,
        string? message = null,
        int? userId = null);
}