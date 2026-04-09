namespace ViralContentApi.DTOs
{
    public class PlanComparisonFeatureResponse
    {
        public string FeatureKey { get; set; } = string.Empty;
        public string FeatureName { get; set; } = string.Empty;
        public Dictionary<string, bool> AvailabilityByPlan { get; set; } = new();
    }
}