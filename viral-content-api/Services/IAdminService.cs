using ViralContentApi.DTOs;

namespace ViralContentApi.Services
{
    public interface IAdminService
    {
        Task<List<AdminUserResponse>> GetUsersAsync();
        Task<AiPlanUpdateResponse> UpdateUserPlanAsync(int userId, string plan);
        Task<AdminDashboardResponse> GetDashboardAsync();
        Task<AdminUserRoleUpdateResponse> UpdateUserRoleAsync(int userId, string role);
        Task<AdminAiAnalyticsResponse> GetAiAnalyticsAsync(DateTime? from, DateTime? to);
        Task<AdminUserActiveStatusUpdateResponse> UpdateUserActiveStatusAsync(int userId, bool isActive);
    }

    public class AdminUserRoleUpdateResponse
    {
        public int UserId { get; set; }
        public string Role { get; set; } = "User";
    }

    public class AdminUserActiveStatusUpdateResponse
    {
        public int UserId { get; set; }
        public bool IsActive { get; set; }
    }
}