using ViralContentApi.DTOs;

namespace ViralContentApi.Services;

public class NotificationTemplateService : INotificationTemplateService
{
    public BillingEmailNotificationRequest BuildSubscriptionActivatedEmail(
        string toEmail,
        string? toName,
        string plan,
        DateTime? expiresAtUtc,
        int? userId = null)
    {
        var expiryText = expiresAtUtc.HasValue
            ? $"Your subscription is active until <strong>{expiresAtUtc.Value:yyyy-MM-dd HH:mm} UTC</strong>."
            : "Your subscription is now active.";

        return new BillingEmailNotificationRequest
        {
            ToEmail = toEmail,
            ToName = toName,
            UserId = userId,
            EventType = "subscription_activated",
            Subject = $"Your {plan} plan is now active",
            HtmlContent = $"""
                <h2>Subscription activated</h2>
                <p>Hi {GetSafeName(toName)},</p>
                <p>Your <strong>{Encode(plan)}</strong> plan has been activated.</p>
                <p>{expiryText}</p>
                <p>Thanks for using Viral Content API.</p>
                """,
            PlainTextContent = $"Your {plan} plan has been activated."
        };
    }

    public BillingEmailNotificationRequest BuildCancelRequestedEmail(
        string toEmail,
        string? toName,
        string plan,
        DateTime? endsAtUtc,
        int? userId = null)
    {
        var endText = endsAtUtc.HasValue
            ? $"Your access remains active until <strong>{endsAtUtc.Value:yyyy-MM-dd HH:mm} UTC</strong>."
            : "Your cancellation request has been recorded.";

        return new BillingEmailNotificationRequest
        {
            ToEmail = toEmail,
            ToName = toName,
            UserId = userId,
            EventType = "subscription_cancel_requested",
            Subject = $"Your {plan} cancellation request was received",
            HtmlContent = $"""
                <h2>Cancellation requested</h2>
                <p>Hi {GetSafeName(toName)},</p>
                <p>We received your cancellation request for the <strong>{Encode(plan)}</strong> plan.</p>
                <p>{endText}</p>
                <p>You can reactivate your subscription any time before it ends.</p>
                """,
            PlainTextContent = $"We received your cancellation request for the {plan} plan."
        };
    }

    public BillingEmailNotificationRequest BuildCancelledEmail(
        string toEmail,
        string? toName,
        string plan,
        int? userId = null)
    {
        return new BillingEmailNotificationRequest
        {
            ToEmail = toEmail,
            ToName = toName,
            UserId = userId,
            EventType = "subscription_cancelled",
            Subject = $"Your {plan} subscription has been cancelled",
            HtmlContent = $"""
                <h2>Subscription cancelled</h2>
                <p>Hi {GetSafeName(toName)},</p>
                <p>Your <strong>{Encode(plan)}</strong> subscription has been cancelled.</p>
                <p>Your account may now be on a lower plan depending on your subscription status.</p>
                """,
            PlainTextContent = $"Your {plan} subscription has been cancelled."
        };
    }

    public BillingEmailNotificationRequest BuildExpiredEmail(
        string toEmail,
        string? toName,
        string plan,
        int? userId = null)
    {
        return new BillingEmailNotificationRequest
        {
            ToEmail = toEmail,
            ToName = toName,
            UserId = userId,
            EventType = "subscription_expired",
            Subject = $"Your {plan} subscription expired",
            HtmlContent = $"""
                <h2>Subscription expired</h2>
                <p>Hi {GetSafeName(toName)},</p>
                <p>Your <strong>{Encode(plan)}</strong> subscription has expired.</p>
                <p>Renew at any time to restore paid features.</p>
                """,
            PlainTextContent = $"Your {plan} subscription has expired."
        };
    }

    public BillingEmailNotificationRequest BuildPastDueEmail(
        string toEmail,
        string? toName,
        string plan,
        int? userId = null)
    {
        return new BillingEmailNotificationRequest
        {
            ToEmail = toEmail,
            ToName = toName,
            UserId = userId,
            EventType = "subscription_past_due",
            Subject = $"Your {plan} subscription payment is past due",
            HtmlContent = $"""
                <h2>Payment issue</h2>
                <p>Hi {GetSafeName(toName)},</p>
                <p>We could not complete payment for your <strong>{Encode(plan)}</strong> subscription.</p>
                <p>Please update billing details to avoid interruption.</p>
                """,
            PlainTextContent = $"Payment for your {plan} subscription is past due."
        };
    }

    public BillingEmailNotificationRequest BuildSubscriptionDowngradedEmail(
        string toEmail,
        string? toName,
        string oldPlan,
        string newPlan,
        int? userId = null)
    {
        return new BillingEmailNotificationRequest
        {
            ToEmail = toEmail,
            ToName = toName,
            UserId = userId,
            EventType = "subscription_downgraded",
            Subject = $"Your plan changed from {oldPlan} to {newPlan}",
            HtmlContent = $"""
                <h2>Plan updated</h2>
                <p>Hi {GetSafeName(toName)},</p>
                <p>Your account was moved from <strong>{Encode(oldPlan)}</strong> to <strong>{Encode(newPlan)}</strong>.</p>
                <p>If this was unexpected, please contact support.</p>
                """,
            PlainTextContent = $"Your plan changed from {oldPlan} to {newPlan}."
        };
    }

    public BillingEmailNotificationRequest BuildTestEmail(
        string toEmail,
        string? toName,
        string? subject = null,
        string? message = null,
        int? userId = null)
    {
        var safeSubject = string.IsNullOrWhiteSpace(subject)
            ? "Viral Content API test email"
            : subject.Trim();

        var safeMessage = string.IsNullOrWhiteSpace(message)
            ? "This is a test email from the notification provider."
            : message.Trim();

        return new BillingEmailNotificationRequest
        {
            ToEmail = toEmail,
            ToName = toName,
            UserId = userId,
            EventType = "test_email",
            Subject = safeSubject,
            HtmlContent = $"""
                <h2>Test email</h2>
                <p>Hi {GetSafeName(toName)},</p>
                <p>{Encode(safeMessage)}</p>
                <p>If you received this email, the configured notification provider is working.</p>
                """,
            PlainTextContent = safeMessage
        };
    }

    public BillingEmailNotificationRequest BuildByType(
        string templateType,
        string toEmail,
        string? toName,
        string? plan = null,
        string? oldPlan = null,
        string? newPlan = null,
        DateTime? effectiveDateUtc = null,
        string? subject = null,
        string? message = null,
        int? userId = null)
    {
        var normalized = Normalize(templateType);
        var safePlan = string.IsNullOrWhiteSpace(plan) ? "Pro" : plan.Trim();
        var safeOldPlan = string.IsNullOrWhiteSpace(oldPlan) ? "Pro" : oldPlan.Trim();
        var safeNewPlan = string.IsNullOrWhiteSpace(newPlan) ? "Free" : newPlan.Trim();

        return normalized switch
        {
            "subscription_activated" or "activated"
                => BuildSubscriptionActivatedEmail(toEmail, toName, safePlan, effectiveDateUtc, userId),

            "subscription_cancel_requested" or "cancel_requested"
                => BuildCancelRequestedEmail(toEmail, toName, safePlan, effectiveDateUtc, userId),

            "subscription_cancelled" or "cancelled"
                => BuildCancelledEmail(toEmail, toName, safePlan, userId),

            "subscription_expired" or "expired"
                => BuildExpiredEmail(toEmail, toName, safePlan, userId),

            "subscription_past_due" or "past_due"
                => BuildPastDueEmail(toEmail, toName, safePlan, userId),

            "subscription_downgraded" or "downgraded"
                => BuildSubscriptionDowngradedEmail(toEmail, toName, safeOldPlan, safeNewPlan, userId),

            "test" or "test_email"
                => BuildTestEmail(toEmail, toName, subject, message, userId),

            _ => BuildTestEmail(
                toEmail,
                toName,
                subject ?? $"Unknown template type: {templateType}",
                message ?? "Fallback test email was used because the template type was not recognized.",
                userId)
        };
    }

    private static string Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim().ToLowerInvariant();
    }

    private static string GetSafeName(string? name)
    {
        return string.IsNullOrWhiteSpace(name)
            ? "there"
            : Encode(name);
    }

    private static string Encode(string value)
    {
        return System.Net.WebUtility.HtmlEncode(value);
    }
}