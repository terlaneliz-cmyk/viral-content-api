using System.Security.Claims;
using ViralContentApi.Data;

namespace ViralContentApi.Middleware;

public class ActiveUserMiddleware
{
    private readonly RequestDelegate _next;

    public ActiveUserMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext db)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrWhiteSpace(userId))
        {
            var user = await db.Users.FindAsync(int.Parse(userId));

            if (user == null || !user.IsActive)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new
                {
                    message = "Session expired or account inactive."
                });
                return;
            }
        }

        await _next(context);
    }
}