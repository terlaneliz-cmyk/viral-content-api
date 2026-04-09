namespace ViralContentApi.DTOs
{
    public class PlanComparisonResponse
    {
        public List<PublicPlanInfoResponse> Plans { get; set; } = new();
        public List<PlanComparisonFeatureResponse> Features { get; set; } = new();
    }
}