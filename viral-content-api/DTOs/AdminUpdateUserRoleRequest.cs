using System.ComponentModel.DataAnnotations;

namespace ViralContentApi.DTOs
{
    public class AdminUpdateUserRoleRequest
    {
        [Required]
        public string Role { get; set; } = string.Empty;
    }
}