using ViralContentApi.DTOs;

namespace ViralContentApi.Services
{
    public interface IPlansService
    {
        List<PublicPlanInfoResponse> GetPlans();
        PublicPlanInfoResponse? GetPlanByName(string name);
        PlanComparisonResponse GetComparison();
        PlanCheckoutSummaryResponse? GetCheckoutSummary(string name, string? billingCycle);
        Task<MyPlanResponse?> GetMyPlanAsync(int userId);
        Task<PlanUpgradePreviewResponse?> GetUpgradePreviewAsync(int userId, PlanUpgradePreviewRequest request);
        Task<CreateCheckoutSessionResponse?> CreateCheckoutSessionAsync(int userId, CreateCheckoutSessionRequest request);
    }
}