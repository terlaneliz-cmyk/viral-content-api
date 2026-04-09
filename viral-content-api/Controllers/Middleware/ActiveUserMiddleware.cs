using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using ViralContentApi.Data;

namespace ViralContentApi.Middleware
{
    public class ActiveUserMiddleware
    {
        private readonly RequestDelegate _next;

        public ActiveUserMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                  ?? context.User.FindFirst("sub")?.Value;

                if (int.TryParse(userIdClaim, out var userId))
                {
                    var user = await dbContext.Users
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == userId);

                    if (user == null || !user.IsActive)
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        await context.Response.WriteAsJsonAsync(new
                        {
                            message = "Your account is disabled."
                        });
                        return;
                    }
                }
            }

            await _next(context);
        }
    }
}