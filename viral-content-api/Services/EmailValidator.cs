namespace ViralContentApi.Services;

public static class EmailValidator
{
    private static readonly string[] BlockedDomains =
    {
        "mailinator.com",
        "10minutemail.com",
        "tempmail.com",
        "guerrillamail.com",
        "yopmail.com"
    };

    public static bool IsFake(string email)
    {
        var domain = email.Split('@').LastOrDefault()?.ToLower();
        if (string.IsNullOrWhiteSpace(domain)) return true;

        return BlockedDomains.Contains(domain);
    }
}