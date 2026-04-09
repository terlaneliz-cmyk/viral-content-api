namespace ViralContentApi.DTOs;

public class ClearProcessedWebhookEventsResponse
{
    public int DeletedCount { get; set; }
    public string Message { get; set; } = string.Empty;
}