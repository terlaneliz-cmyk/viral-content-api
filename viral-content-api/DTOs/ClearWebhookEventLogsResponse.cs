namespace ViralContentApi.DTOs;

public class ClearWebhookEventLogsResponse
{
    public int DeletedCount { get; set; }
    public string Message { get; set; } = string.Empty;
}