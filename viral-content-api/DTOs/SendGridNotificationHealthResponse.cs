namespace ViralContentApi.DTOs;

public class SendGridNotificationHealthResponse
{
    public bool ApiKeyConfigured { get; set; }
    public bool FromEmailConfigured { get; set; }
    public bool FromNameConfigured { get; set; }
    public bool SandboxMode { get; set; }
}