using Microsoft.EntityFrameworkCore;
using ViralContentApi.Data;
using ViralContentApi.DTOs;
using ViralContentApi.Models;

namespace ViralContentApi.Services;

public class ProcessedWebhookEventService : IProcessedWebhookEventService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ProcessedWebhookEventService> _logger;

    public ProcessedWebhookEventService(
        AppDbContext context,
        ILogger<ProcessedWebhookEventService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> HasBeenProcessedAsync(string provider, string externalEventId)
    {
        if (string.IsNullOrWhiteSpace(provider) || string.IsNullOrWhiteSpace(externalEventId))
        {
            return false;
        }

        return await _context.ProcessedWebhookEvents.AnyAsync(x =>
            x.Provider == provider &&
            x.ExternalEventId == externalEventId);
    }

    public async Task MarkAsProcessedAsync(string provider, string externalEventId)
    {
        if (string.IsNullOrWhiteSpace(provider) || string.IsNullOrWhiteSpace(externalEventId))
        {
            return;
        }

        var alreadyExists = await _context.ProcessedWebhookEvents.AnyAsync(x =>
            x.Provider == provider &&
            x.ExternalEventId == externalEventId);

        if (alreadyExists)
        {
            return;
        }

        _context.ProcessedWebhookEvents.Add(new ProcessedWebhookEvent
        {
            Provider = provider,
            ExternalEventId = externalEventId,
            ProcessedAtUtc = DateTime.UtcNow
        });

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogWarning(
                ex,
                "Processed webhook event insert raced with another request. Provider: {Provider}, ExternalEventId: {ExternalEventId}",
                provider,
                externalEventId);
        }
    }

    public async Task<List<ProcessedWebhookEventResponse>> GetRecentAsync(string? provider = null, int take = 50)
    {
        if (take <= 0)
        {
            take = 50;
        }

        if (take > 200)
        {
            take = 200;
        }

        IQueryable<ProcessedWebhookEvent> query = _context.ProcessedWebhookEvents.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(provider))
        {
            query = query.Where(x => x.Provider == provider);
        }

        return await query
            .OrderByDescending(x => x.ProcessedAtUtc)
            .Take(take)
            .Select(x => new ProcessedWebhookEventResponse
            {
                Id = x.Id,
                Provider = x.Provider,
                ExternalEventId = x.ExternalEventId,
                ProcessedAtUtc = x.ProcessedAtUtc
            })
            .ToListAsync();
    }

    public async Task<ClearProcessedWebhookEventsResponse> ClearAsync(string? provider = null, int? olderThanDays = null)
    {
        IQueryable<ProcessedWebhookEvent> query = _context.ProcessedWebhookEvents;

        if (!string.IsNullOrWhiteSpace(provider))
        {
            query = query.Where(x => x.Provider == provider);
        }

        if (olderThanDays.HasValue && olderThanDays.Value > 0)
        {
            var cutoffUtc = DateTime.UtcNow.AddDays(-olderThanDays.Value);
            query = query.Where(x => x.ProcessedAtUtc < cutoffUtc);
        }

        var items = await query.ToListAsync();

        if (items.Count == 0)
        {
            return new ClearProcessedWebhookEventsResponse
            {
                DeletedCount = 0,
                Message = "No processed webhook events matched the filter."
            };
        }

        _context.ProcessedWebhookEvents.RemoveRange(items);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Cleared processed webhook events. Count: {Count}, Provider: {Provider}, OlderThanDays: {OlderThanDays}",
            items.Count,
            provider,
            olderThanDays);

        return new ClearProcessedWebhookEventsResponse
        {
            DeletedCount = items.Count,
            Message = "Processed webhook events cleared successfully."
        };
    }
}