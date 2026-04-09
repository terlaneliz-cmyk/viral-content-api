namespace ViralContentApi.DTOs
{
    public class AdminUserResponse
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Plan { get; set; } = "Free";
        public string Role { get; set; } = "User";
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TotalPosts { get; set; }
        public int AiPosts { get; set; }
    }
}