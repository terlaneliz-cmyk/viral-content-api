using Microsoft.EntityFrameworkCore;
using ViralContentApi.Data;
using ViralContentApi.DTOs;
using ViralContentApi.Models;

namespace ViralContentApi.Services
{
    public class AiUsageLimitService : IAiUsageLimitService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AiUsageLimitService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<AiUsageStatusResponse> TryConsumeAsync(int userId)
        {
            var currentStatus = await GetUsageAsync(userId);

            if (currentStatus.RemainingToday <= 0)
            {
                currentStatus.Allowed = false;
                currentStatus.CanUse = false;
                return currentStatus;
            }

            var utcToday = DateTime.UtcNow.Date;

            var record = await _context.AiUsageRecords
                .FirstOrDefaultAsync(x => x.UserId == userId && x.UsageDateUtc == utcToday);

            if (record == null)
            {
                record = new AiUsageRecord
                {
                    UserId = userId,
                    UsageDateUtc = utcToday,
                    UsedCount = 1
                };

                _context.AiUsageRecords.Add(record);
            }
            else
            {
                record.UsedCount++;
            }

            await _context.SaveChangesAsync();

            var updatedStatus = await GetUsageAsync(userId);
            updatedStatus.Allowed = true;
            updatedStatus.CanUse = true;

            return updatedStatus;
        }

        public async Task<AiUsageStatusResponse> GetUsageAsync(int userId)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == userId);

            var plan = string.IsNullOrWhiteSpace(user?.Plan) ? "Free" : user!.Plan;
            var baseDailyLimit = GetDailyLimitForPlan(plan);

            var referralSignupCount = Math.Max(0, user?.ReferralSignupCount ?? 0);
            var referralBonus = referralSignupCount * 3;

            var streakBonus = 0;
            if ((user?.CurrentStreak ?? 0) >= 14)
            {
                streakBonus = 2;
            }
            else if ((user?.CurrentStreak ?? 0) >= 7)
            {
                streakBonus = 1;
            }

            var totalDailyLimit = baseDailyLimit + referralBonus + streakBonus;
            var utcToday = DateTime.UtcNow.Date;

            var usageRecord = await _context.AiUsageRecords
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == userId && x.UsageDateUtc == utcToday);

            var usedToday = usageRecord?.UsedCount ?? 0;
            var remainingToday = Math.Max(0, totalDailyLimit - usedToday);
            var allowed = remainingToday > 0;

            return new AiUsageStatusResponse
            {
                Plan = plan,
                DailyLimit = totalDailyLimit,
                UsedToday = usedToday,
                RemainingToday = remainingToday,
                Allowed = allowed,
                CanUse = allowed,
                ReferralBonus = referralBonus
            };
        }

        public async Task<AiUsageStatusResponse> GetUsageStatusAsync(int userId)
        {
            return await GetUsageAsync(userId);
        }

        public async Task<object> GetUsageHistoryAsync(int userId, int days)
        {
            if (days <= 0)
            {
                days = 30;
            }

            if (days > 365)
            {
                days = 365;
            }

            var utcToday = DateTime.UtcNow.Date;
            var fromDate = utcToday.AddDays(-(days - 1));

            var records = await _context.AiUsageRecords
                .AsNoTracking()
                .Where(x => x.UserId == userId && x.UsageDateUtc >= fromDate)
                .OrderBy(x => x.UsageDateUtc)
                .ToListAsync();

            var history = Enumerable.Range(0, days)
                .Select(offset =>
                {
                    var day = fromDate.AddDays(offset);
                    var record = records.FirstOrDefault(x => x.UsageDateUtc == day);

                    return new
                    {
                        date = day.ToString("yyyy-MM-dd"),
                        count = record?.UsedCount ?? 0
                    };
                })
                .ToList();

            return new
            {
                days,
                history
            };
        }

        public async Task<bool> CanUseAsync(int userId)
        {
            var status = await GetUsageAsync(userId);
            return status.RemainingToday > 0;
        }

        public async Task RecordUsageAsync(int userId)
        {
            var utcToday = DateTime.UtcNow.Date;

            var record = await _context.AiUsageRecords
                .FirstOrDefaultAsync(x => x.UserId == userId && x.UsageDateUtc == utcToday);

            if (record == null)
            {
                record = new AiUsageRecord
                {
                    UserId = userId,
                    UsageDateUtc = utcToday,
                    UsedCount = 1
                };

                _context.AiUsageRecords.Add(record);
            }
            else
            {
                record.UsedCount++;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<AiPlanUpdateResponse> UpdateUserPlanAsync(int userId, string plan)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
            {
                return new AiPlanUpdateResponse
                {
                    UserId = userId,
                    Plan = "Free",
                    DailyLimit = GetDailyLimitForPlan("Free")
                };
            }

            var normalizedPlan = string.IsNullOrWhiteSpace(plan) ? "Free" : plan.Trim();

            user.Plan = normalizedPlan;
            await _context.SaveChangesAsync();

            return new AiPlanUpdateResponse
            {
                UserId = user.Id,
                Plan = normalizedPlan,
                DailyLimit = GetDailyLimitForPlan(normalizedPlan)
            };
        }

        private int GetDailyLimitForPlan(string? plan)
        {
            var normalizedPlan = string.IsNullOrWhiteSpace(plan) ? "Free" : plan.Trim();
            var value = _configuration[$"AiUsageLimits:Plans:{normalizedPlan}"];

            return int.TryParse(value, out var parsedLimit) ? parsedLimit : 10;
        }
    }
}