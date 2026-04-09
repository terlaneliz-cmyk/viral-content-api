namespace ViralContentApi.DTOs;

public class StripeBillingSettingsHealthResponse
{
    public bool SecretKeyConfigured { get; set; }
    public bool PublishableKeyConfigured { get; set; }
    public bool WebhookSecretConfigured { get; set; }
}