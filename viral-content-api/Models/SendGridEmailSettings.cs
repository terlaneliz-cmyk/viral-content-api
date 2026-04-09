namespace ViralContentApi.Models;

public class SendGridEmailSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "Viral Content API";
    public bool SandboxMode { get; set; } = false;
}