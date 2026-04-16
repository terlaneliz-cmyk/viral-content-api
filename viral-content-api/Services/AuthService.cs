using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ViralContentApi.Data;
using ViralContentApi.DTOs;
using ViralContentApi.Models;

namespace ViralContentApi.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IUserSubscriptionService _userSubscriptionService;

    public AuthService(
        AppDbContext context,
        IConfiguration configuration,
        IUserSubscriptionService userSubscriptionService)
    {
        _context = context;
        _configuration = configuration;
        _userSubscriptionService = userSubscriptionService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, string? referralCode = null)
    {
        var email = request.Email?.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email))
            throw new Exception("Invalid email format.");

        if (!IsStrongPassword(request.Password))
            throw new Exception("Password must be 8+ chars, include uppercase, lowercase, number.");

        var exists = await _context.Users.AnyAsync(x => x.Email == email);
        if (exists)
            throw new Exception("Email already registered.");

        var effectiveReferralCode = NormalizeReferralCode(
            !string.IsNullOrWhiteSpace(request.ReferralCode)
                ? request.ReferralCode
                : referralCode);

        User? referrer = null;

        if (!string.IsNullOrWhiteSpace(effectiveReferralCode))
        {
            referrer = await _context.Users.FirstOrDefaultAsync(x => x.ReferralCode == effectiveReferralCode);

            if (referrer == null)
                throw new Exception("Referral code not found.");
        }

        var user = new User
        {
            Email = email,
            Username = email.Split("@")[0],
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = "User",
            Plan = "Free",
            IsActive = true,
            ReferralCode = GenerateReferralCode(email),
            ReferralCodeCreatedAtUtc = DateTime.UtcNow
        };

        if (referrer != null)
        {
            user.ReferredByUserId = referrer.Id;
            referrer.ReferralSignupCount += 1;
        }

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        if (referrer != null)
        {
            _context.BillingEventLogs.Add(new BillingEventLog
            {
                UserId = user.Id,
                EventType = "referral_signup_registered",
                Status = "success",
                Success = true,
                Message = $"Referral applied during signup: {effectiveReferralCode}",
                Metadata = $"ReferrerUserId={referrer.Id}; ReferralCode={effectiveReferralCode}; NewUserId={user.Id}",
                CreatedAtUtc = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            if (referrer.ReferralSignupCount >= 3)
            {
                await _userSubscriptionService.ActivateReferralRewardTrialAsync(referrer.Id);
            }
        }

        return GenerateAuthResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var email = request.Email?.Trim().ToLowerInvariant();

        var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new Exception("Invalid email or password.");

        if (!user.IsActive)
            throw new Exception("Account is disabled.");

        return GenerateAuthResponse(user);
    }

    private AuthResponse GenerateAuthResponse(User user)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is missing.");
        var jwtIssuer = _configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer is missing.");
        var jwtAudience = _configuration["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience is missing.");

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds);

        return new AuthResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token)
        };
    }

    private static string? NormalizeReferralCode(string? referralCode)
    {
        if (string.IsNullOrWhiteSpace(referralCode))
            return null;

        return referralCode.Trim().ToUpperInvariant();
    }

    private static string GenerateReferralCode(string email)
    {
        var basePart = new string(
            (email ?? "user")
                .Split('@')[0]
                .Where(char.IsLetterOrDigit)
                .Take(8)
                .ToArray());

        if (string.IsNullOrWhiteSpace(basePart))
        {
            basePart = "USER";
        }

        var randomPart = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
        return $"{basePart.ToUpperInvariant()}{randomPart}";
    }

    private bool IsValidEmail(string email)
    {
        return email.Contains("@") && email.Contains(".");
    }

    private bool IsStrongPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8) return false;

        return password.Any(char.IsUpper)
            && password.Any(char.IsLower)
            && password.Any(char.IsDigit);
    }
}