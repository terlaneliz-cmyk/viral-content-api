using System.Net.Mail;

namespace ViralContentApi.Services;

public static class EmailValidationHelper
{
    private static readonly HashSet<string> DisposableDomains = new(StringComparer.OrdinalIgnoreCase)
    {
        "mailinator.com",
        "10minutemail.com",
        "tempmail.com",
        "guerrillamail.com",
        "yopmail.com",
        "trashmail.com",
        "throwawaymail.com",
        "dispostable.com",
        "fakeinbox.com",
        "sharklasers.com",
        "guerrillamailblock.com",
        "pokemail.net",
        "spam4.me",
        "bccto.me",
        "chacuo.net",
        "getnada.com",
        "mintemail.com",
        "mohmal.com",
        "emailondeck.com",
        "tmpmail.org"
    };

    public static bool IsValidFormat(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        try
        {
            var parsed = new MailAddress(email.Trim());
            return parsed.Address.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    public static bool IsDisposable(string? email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            return true;
        }

        var domain = email.Split('@').LastOrDefault()?.Trim();
        if (string.IsNullOrWhiteSpace(domain))
        {
            return true;
        }

        return DisposableDomains.Contains(domain);
    }
}