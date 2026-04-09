namespace ViralContentApi.DTOs;

public class BillingWebhookRequest
{
    public string RawBody { get; set; } = string.Empty;
    public string SignatureHeader { get; set; } = string.Empty;
    public bool SkipSignatureValidation { get; set; } = false;

    // Backward-compatible / optional fields
    public string Provider { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
}