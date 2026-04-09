namespace ViralContentApi.DTOs;

public class AdminStripeWebhookSimulationRequest
{
    public string RawBody { get; set; } = string.Empty;
    public string SignatureHeader { get; set; } = string.Empty;
    public bool SkipSignatureValidation { get; set; } = false;
}