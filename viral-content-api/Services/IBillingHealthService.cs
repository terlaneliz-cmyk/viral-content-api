using ViralContentApi.DTOs;

namespace ViralContentApi.Services;

public interface IBillingHealthService
{
    Task<BillingHealthResponse> GetHealthAsync();
    Task<BillingHealthResponse> GetHealth();
}