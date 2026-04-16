using ViralContentApi.DTOs;

namespace ViralContentApi.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, string? referralCode = null);
    Task<AuthResponse> LoginAsync(LoginRequest request);
}