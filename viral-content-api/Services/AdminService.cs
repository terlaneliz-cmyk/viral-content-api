using Microsoft.EntityFrameworkCore;
using ViralContentApi.Data;
using ViralContentApi.DTOs;

namespace ViralContentApi.Services
{
    public class AdminService : IAdminService
    {
        private readonly AppDbContext _context;
        private readonly IAiUsageLimitService _aiUsageLimitService;

        public AdminService(AppDbContext context, IAiUsageLimitService aiUsageLimitService)
        {
            _context = context;
            _aiUsageLimitService = aiUsageLimitService;
        }

        public async Task<List<AdminUserResponse>> GetUsersAsync()
        {
            var users = await _context.Users
                .AsNoTracking()
                .OrderBy(x => x.Id)
                .Select(x => new AdminUserResponse
                {
                    Id = x.Id,
                    Username = x.Username,
                    Email = x.Email,
                    Plan = string.IsNullOrWhiteSpace(x.Plan) ? "Free" : x.Plan,
                    Role = string.IsNullOrWhiteSpace(x.Role) ? "User" : x.Role,
                    IsActive = x.IsActive,
                    CreatedAt = x.CreatedAt,
                    TotalPosts = x.Posts.Count,
                    AiPosts = x.Posts.Count(p => p.IsAiGenerated)
                })
                .ToListAsync();

            return users;
        }

        public async Task<AiPlanUpdateResponse> UpdateUserPlanAsync(int userId, string plan)
        {
            return await _aiUsageLimitService.UpdateUserPlanAsync(userId, plan);
        }

        public async Task<AdminDashboardResponse> GetDashboardAsync()
        {
            var todayUtc = DateTime.UtcNow.Date;
            var last7DaysStartUtc = todayUtc.AddDays(-6);

            var users = await _context.Users
                .AsNoTracking()
                .ToListAsync();

            var posts = await _context.Posts
                .AsNoTracking()
                .Where(x => x.DeletedAt == null)
                .ToListAsync();

            var usageRecords = await _context.AiUsageRecords
                .AsNoTracking()
                .Where(x => x.UsageDateUtc >= last7DaysStartUtc)
                .ToListAsync();

            var topUsageToday = await _context.AiUsageRecords
                .AsNoTracking()
                .Where(x => x.UsageDateUtc == todayUtc && x.UsedCount > 0)
                .Join(
                    _context.Users.AsNoTracking(),
                    usage => usage.UserId,
                    user => user.Id,
                    (usage, user) => new AdminTopAiUserResponse
                    {
                        UserId = user.Id,
                        Username = user.Username,
                        Email = user.Email,
                        Plan = NormalizePlan(user.Plan),
                        UsedToday = usage.UsedCount
                    })
                .OrderByDescending(x => x.UsedToday)
                .ThenBy(x => x.UserId)
                .Take(10)
                .ToListAsync();

            return new AdminDashboardResponse
            {
                TotalUsers = users.Count,
                FreeUsers = users.Count(x => NormalizePlan(x.Plan) == "Free"),
                ProUsers = users.Count(x => NormalizePlan(x.Plan) == "Pro"),
                AgencyUsers = users.Count(x => NormalizePlan(x.Plan) == "Agency"),
                AdminUsers = users.Count(x => NormalizeRole(x.Role) == "Admin"),
                RegularUsers = users.Count(x => NormalizeRole(x.Role) == "User"),
                TotalPosts = posts.Count,
                TotalAiPosts = posts.Count(x => x.IsAiGenerated),
                AiGenerationsToday = usageRecords.Where(x => x.UsageDateUtc == todayUtc).Sum(x => x.UsedCount),
                AiGenerationsLast7Days = usageRecords.Sum(x => x.UsedCount),
                TopAiUsersToday = topUsageToday
            };
        }

        public async Task<AdminUserRoleUpdateResponse> UpdateUserRoleAsync(int userId, string role)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            user.Role = NormalizeRole(role);

            await _context.SaveChangesAsync();

            return new AdminUserRoleUpdateResponse
            {
                UserId = user.Id,
                Role = user.Role
            };
        }

        public async Task<AdminAiAnalyticsResponse> GetAiAnalyticsAsync(DateTime? from, DateTime? to)
        {
            var toDate = to?.Date ?? DateTime.UtcNow.Date;
            var fromDate = from?.Date ?? toDate.AddDays(-6);

            if (fromDate > toDate)
            {
                (fromDate, toDate) = (toDate, fromDate);
            }

            var maxRangeDays = 366;
            if ((toDate - fromDate).TotalDays >= maxRangeDays)
            {
                fromDate = toDate.AddDays(-(maxRangeDays - 1));
            }

            var usageRecords = await _context.AiUsageRecords
                .AsNoTracking()
                .Where(x => x.UsageDateUtc >= fromDate && x.UsageDateUtc <= toDate)
                .ToListAsync();

            var users = await _context.Users
                .AsNoTracking()
                .ToListAsync();

            var usersById = users.ToDictionary(x => x.Id, x => x);

            var dailyUsage = new List<AdminAiAnalyticsDailyItemResponse>();
            for (var date = fromDate; date <= toDate; date = date.AddDays(1))
            {
                dailyUsage.Add(new AdminAiAnalyticsDailyItemResponse
                {
                    Date = date,
                    UsedCount = usageRecords
                        .Where(x => x.UsageDateUtc.Date == date)
                        .Sum(x => x.UsedCount)
                });
            }

            var usageByPlan = usageRecords
                .GroupBy(x =>
                {
                    if (usersById.TryGetValue(x.UserId, out var user))
                    {
                        return NormalizePlan(user.Plan);
                    }

                    return "Free";
                })
                .Select(g => new AdminAiAnalyticsPlanItemResponse
                {
                    Plan = g.Key,
                    UsedCount = g.Sum(x => x.UsedCount)
                })
                .OrderByDescending(x => x.UsedCount)
                .ThenBy(x => x.Plan)
                .ToList();

            var topUsers = usageRecords
                .GroupBy(x => x.UserId)
                .Select(g =>
                {
                    usersById.TryGetValue(g.Key, out var user);

                    return new AdminTopAiUserResponse
                    {
                        UserId = g.Key,
                        Username = user?.Username ?? "Unknown",
                        Email = user?.Email ?? string.Empty,
                        Plan = NormalizePlan(user?.Plan),
                        UsedToday = g.Sum(x => x.UsedCount)
                    };
                })
                .OrderByDescending(x => x.UsedToday)
                .ThenBy(x => x.UserId)
                .Take(10)
                .ToList();

            return new AdminAiAnalyticsResponse
            {
                FromDate = fromDate,
                ToDate = toDate,
                TotalGenerations = usageRecords.Sum(x => x.UsedCount),
                DailyUsage = dailyUsage,
                UsageByPlan = usageByPlan,
                TopUsers = topUsers
            };
        }

        public async Task<AdminUserActiveStatusUpdateResponse> UpdateUserActiveStatusAsync(int userId, bool isActive)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            user.IsActive = isActive;
            await _context.SaveChangesAsync();

            return new AdminUserActiveStatusUpdateResponse
            {
                UserId = user.Id,
                IsActive = user.IsActive
            };
        }

        private static string NormalizePlan(string? plan)
        {
            if (string.IsNullOrWhiteSpace(plan))
            {
                return "Free";
            }

            return plan.Trim().ToLowerInvariant() switch
            {
                "free" => "Free",
                "pro" => "Pro",
                "agency" => "Agency",
                _ => "Free"
            };
        }

        private static string NormalizeRole(string? role)
        {
            if (string.IsNullOrWhiteSpace(role))
            {
                return "User";
            }

            return role.Trim().ToLowerInvariant() switch
            {
                "admin" => "Admin",
                _ => "User"
            };
        }
    }
}