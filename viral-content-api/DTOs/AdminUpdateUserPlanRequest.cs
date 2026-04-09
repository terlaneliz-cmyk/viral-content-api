using System.ComponentModel.DataAnnotations;

namespace ViralContentApi.DTOs
{
    public class AdminUpdateUserPlanRequest
    {
        [Required]
        public string Plan { get; set; } = string.Empty;
    }
}