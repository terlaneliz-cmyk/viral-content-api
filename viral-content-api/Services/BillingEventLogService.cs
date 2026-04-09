using Microsoft.EntityFrameworkCore;
using ViralContentApi.Data;
using ViralContentApi.DTOs;
using ViralContentApi.Models;

namespace ViralContentApi.Services
{
    public class BillingEventLogService : IBillingEventLogService
    {
        private readonly AppDbContext _context;

        public BillingEventLogService(AppDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(
            int? userId,
            string eventType,
            string status,
            string? externalCheckoutSessionId,
            string payloadJson,
            bool success,
            string message)
        {
            var log = new BillingEventLog
            {
                UserId = userId,
                EventType = eventType ?? string.Empty,
                Status = status ?? string.Empty,
                ExternalCheckoutSessionId = externalCheckoutSessionId ?? string.Empty,
                PayloadJson = payloadJson ?? string.Empty,
                Success = success,
                Message = message ?? string.Empty,
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.BillingEventLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        public async Task<List<BillingEventLogResponse>> GetRecentAsync(int count = 100)
        {
            if (count <= 0)
            {
                count = 100;
            }

            if (count > 500)
            {
                count = 500;
            }

            return await _context.BillingEventLogs
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAtUtc)
                .Take(count)
                .Select(x => new BillingEventLogResponse
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    EventType = x.EventType,
                    Status = x.Status,
                    ExternalCheckoutSessionId = x.ExternalCheckoutSessionId,
                    Success = x.Success,
                    Message = x.Message,
                    CreatedAtUtc = x.CreatedAtUtc
                })
                .ToListAsync();
        }
    }
}