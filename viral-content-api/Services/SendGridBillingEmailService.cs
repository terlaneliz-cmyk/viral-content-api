using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using ViralContentApi.DTOs;
using ViralContentApi.Models;

namespace ViralContentApi.Services;

public class SendGridBillingEmailService : IBillingEmailService
{
    private readonly SendGridEmailSettings _settings;
    private readonly ILogger<SendGridBillingEmailService> _logger;

    public SendGridBillingEmailService(
        IOptions<SendGridEmailSettings> settings,
        ILogger<SendGridBillingEmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<BillingEmailNotificationResponse> SendAsync(BillingEmailNotificationRequest request)
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            return new BillingEmailNotificationResponse
            {
                Success = false,
                Provider = "SendGrid",
                Message = "SendGrid API key is missing."
            };
        }

        if (string.IsNullOrWhiteSpace(_settings.FromEmail))
        {
            return new BillingEmailNotificationResponse
            {
                Success = false,
                Provider = "SendGrid",
                Message = "SendGrid FromEmail is missing."
            };
        }

        if (string.IsNullOrWhiteSpace(request.ToEmail))
        {
            return new BillingEmailNotificationResponse
            {
                Success = false,
                Provider = "SendGrid",
                Message = "Recipient email is required."
            };
        }

        if (string.IsNullOrWhiteSpace(request.Subject))
        {
            return new BillingEmailNotificationResponse
            {
                Success = false,
                Provider = "SendGrid",
                Message = "Email subject is required."
            };
        }

        var client = new SendGridClient(_settings.ApiKey);

        var from = new EmailAddress(_settings.FromEmail, _settings.FromName);
        var to = new EmailAddress(request.ToEmail, request.ToName);

        var plainText = string.IsNullOrWhiteSpace(request.PlainTextContent)
            ? StripHtml(request.HtmlContent)
            : request.PlainTextContent;

        var html = string.IsNullOrWhiteSpace(request.HtmlContent)
            ? $"<p>{System.Net.WebUtility.HtmlEncode(request.PlainTextContent ?? string.Empty)}</p>"
            : request.HtmlContent;

        var message = MailHelper.CreateSingleEmail(
            from,
            to,
            request.Subject,
            plainText,
            html);

        if (request.Metadata is not null)
        {
            foreach (var item in request.Metadata)
            {
                if (!string.IsNullOrWhiteSpace(item.Key) && !string.IsNullOrWhiteSpace(item.Value))
                {
                    message.AddCustomArg(item.Key, item.Value);
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(request.EventType))
        {
            message.AddCategory(request.EventType);
        }

        if (_settings.SandboxMode)
        {
            message.MailSettings = new MailSettings
            {
                SandboxMode = new SandboxMode
                {
                    Enable = true
                }
            };
        }

        try
        {
            var response = await client.SendEmailAsync(message);

            var responseBody = await response.Body.ReadAsStringAsync();
            var statusCode = (int)response.StatusCode;

            var success = statusCode is >= 200 and < 300;

            if (success)
            {
                _logger.LogInformation(
                    "SendGrid email sent successfully. To: {ToEmail}, Subject: {Subject}, EventType: {EventType}, Status: {StatusCode}",
                    request.ToEmail,
                    request.Subject,
                    request.EventType,
                    statusCode);
            }
            else
            {
                _logger.LogWarning(
                    "SendGrid email failed. To: {ToEmail}, Subject: {Subject}, EventType: {EventType}, Status: {StatusCode}, Body: {Body}",
                    request.ToEmail,
                    request.Subject,
                    request.EventType,
                    statusCode,
                    responseBody);
            }

            IEnumerable<string>? messageIds = null;
            response.Headers?.TryGetValues("X-Message-Id", out messageIds);

            return new BillingEmailNotificationResponse
            {
                Success = success,
                Provider = "SendGrid",
                Message = success
                    ? "Email sent successfully."
                    : $"SendGrid returned status {statusCode}.",
                StatusCode = statusCode,
                ProviderMessageId = messageIds?.FirstOrDefault()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "SendGrid email threw exception. To: {ToEmail}, Subject: {Subject}, EventType: {EventType}",
                request.ToEmail,
                request.Subject,
                request.EventType);

            return new BillingEmailNotificationResponse
            {
                Success = false,
                Provider = "SendGrid",
                Message = $"SendGrid exception: {ex.Message}"
            };
        }
    }

    private static string StripHtml(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        var withoutTags = System.Text.RegularExpressions.Regex.Replace(input, "<.*?>", " ");
        return System.Net.WebUtility.HtmlDecode(withoutTags).Trim();
    }
}