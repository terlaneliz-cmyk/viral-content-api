using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ViralContentApi.DTOs;
using ViralContentApi.Services;

namespace ViralContentApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly IBillingEventLogService _billingEventLogService;
        private readonly IBillingHealthService _billingHealthService;
        private readonly INotificationHealthService _notificationHealthService;

        public AdminController(
            IAdminService adminService,
            IBillingEventLogService billingEventLogService,
            IBillingHealthService billingHealthService,
            INotificationHealthService notificationHealthService)
        {
            _adminService = adminService;
            _billingEventLogService = billingEventLogService;
            _billingHealthService = billingHealthService;
            _notificationHealthService = notificationHealthService;
        }

        [HttpGet("dashboard")]
        public async Task<ActionResult<AdminDashboardResponse>> GetDashboard()
        {
            var dashboard = await _adminService.GetDashboardAsync();
            return Ok(dashboard);
        }

        [HttpGet("ai-analytics")]
        public async Task<ActionResult<AdminAiAnalyticsResponse>> GetAiAnalytics([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var result = await _adminService.GetAiAnalyticsAsync(from, to);
            return Ok(result);
        }

        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<AdminUserResponse>>> GetUsers()
        {
            var users = await _adminService.GetUsersAsync();
            return Ok(users);
        }

        [HttpGet("billing-events")]
        public async Task<ActionResult<IEnumerable<BillingEventLogResponse>>> GetBillingEvents([FromQuery] int count = 100)
        {
            var result = await _billingEventLogService.GetRecentAsync(count);
            return Ok(result);
        }

        [HttpGet("billing-health")]
        public ActionResult<BillingHealthResponse> GetBillingHealth()
        {
            var result = _billingHealthService.GetHealth();
            return Ok(result);
        }

        [HttpGet("notification-health")]
        public ActionResult<NotificationHealthResponse> GetNotificationHealth()
        {
            var result = _notificationHealthService.GetHealth();
            return Ok(result);
        }

        [HttpPut("users/{id}/plan")]
        public async Task<IActionResult> UpdateUserPlan(int id, [FromBody] AdminUpdateUserPlanRequest request)
        {
            var result = await _adminService.UpdateUserPlanAsync(id, request.Plan);

            return Ok(new
            {
                message = "User plan updated successfully.",
                userId = result.UserId,
                plan = result.Plan,
                dailyLimit = result.DailyLimit
            });
        }

        [HttpPut("users/{id}/role")]
        public async Task<IActionResult> UpdateUserRole(int id, [FromBody] AdminUpdateUserRoleRequest request)
        {
            var result = await _adminService.UpdateUserRoleAsync(id, request.Role);

            return Ok(new
            {
                message = "User role updated successfully.",
                userId = result.UserId,
                role = result.Role
            });
        }

        [HttpPut("users/{id}/active-status")]
        public async Task<IActionResult> UpdateUserActiveStatus(int id, [FromBody] AdminUpdateUserActiveStatusRequest request)
        {
            var result = await _adminService.UpdateUserActiveStatusAsync(id, request.IsActive);

            return Ok(new
            {
                message = "User active status updated successfully.",
                userId = result.UserId,
                isActive = result.IsActive
            });
        }
    }
}