using Microsoft.AspNetCore.Mvc;
using ViralContentApi.DTOs;
using ViralContentApi.Services;

namespace ViralContentApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = GetModelErrors();
            return BadRequest(new
            {
                message = errors.FirstOrDefault() ?? "Invalid registration data.",
                errors
            });
        }

        var normalizedEmail = request.Email.Trim();

        if (!EmailValidationHelper.IsValidFormat(normalizedEmail))
        {
            return BadRequest(new
            {
                message = "Invalid email format."
            });
        }

        if (EmailValidationHelper.IsDisposable(normalizedEmail))
        {
            return BadRequest(new
            {
                message = "Disposable or fake email addresses are not allowed."
            });
        }

        request.Email = normalizedEmail;

        var referralCode =
            Request.Query["ref"].FirstOrDefault() ??
            Request.Headers["X-Referral-Code"].FirstOrDefault();

        var result = await _authService.RegisterAsync(request, referralCode);

        if (result == null || string.IsNullOrWhiteSpace(result.Token))
        {
            return BadRequest(new
            {
                message = "Registration failed."
            });
        }

        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = GetModelErrors();
            return BadRequest(new
            {
                message = errors.FirstOrDefault() ?? "Invalid login data.",
                errors
            });
        }

        var normalizedEmail = request.Email.Trim();

        if (!EmailValidationHelper.IsValidFormat(normalizedEmail))
        {
            return BadRequest(new
            {
                message = "Invalid email format."
            });
        }

        request.Email = normalizedEmail;

        var result = await _authService.LoginAsync(request);

        if (result == null || string.IsNullOrWhiteSpace(result.Token))
        {
            return Unauthorized(new
            {
                message = "Invalid email or password."
            });
        }

        return Ok(result);
    }

    private List<string> GetModelErrors()
    {
        return ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToList();
    }
}