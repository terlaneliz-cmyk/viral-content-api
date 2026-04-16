using Microsoft.AspNetCore.Mvc;
using ViralContentApi.Data;
using ViralContentApi.Models;

namespace ViralContentApi.Controllers;

[ApiController]
[Route("api/auth-utils")]
public class AuthUtilityController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuthUtilityController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("request-password-reset")]
    public IActionResult RequestPasswordReset([FromBody] RequestDto request)
    {
        var user = _context.Users.FirstOrDefault(x => x.Email == request.Email);

        // Always return OK (no email enumeration)
        return Ok(new { message = "If account exists, reset email will be sent." });
    }

    [HttpPost("reset-password")]
    public IActionResult ResetPassword([FromBody] ResetDto request)
    {
        return Ok(new { message = "Password reset endpoint ready." });
    }

    [HttpPost("verify-email")]
    public IActionResult VerifyEmail([FromBody] VerifyDto request)
    {
        return Ok(new { message = "Email verification endpoint ready." });
    }

    public class RequestDto
    {
        public string Email { get; set; } = "";
    }

    public class ResetDto
    {
        public string Token { get; set; } = "";
        public string NewPassword { get; set; } = "";
    }

    public class VerifyDto
    {
        public string Token { get; set; } = "";
    }
}