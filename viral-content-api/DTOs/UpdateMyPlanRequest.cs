using System.ComponentModel.DataAnnotations;

namespace ViralContentApi.DTOs
{
    public class UpdateMyPlanRequest
    {
        [Required]
        public string Plan { get; set; } = string.Empty;
    }
}