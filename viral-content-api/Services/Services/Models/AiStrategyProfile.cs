namespace ViralContentApi.Services.Models
{
    public class AiStrategyProfile
    {
        public string PrimaryAngle { get; set; } = "clarity";
        public string BodyEmphasis { get; set; } = "practical value";
        public string HookStyle { get; set; } = "direct";
        public string CtaMode { get; set; } = "conversation";

        public bool UsesCuriosity { get; set; }
        public bool UsesCredibility { get; set; }
        public bool UsesConversionIntent { get; set; }
        public bool UsesEmotionalRelatability { get; set; }
    }
}