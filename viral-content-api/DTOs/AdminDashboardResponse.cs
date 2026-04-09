namespace ViralContentApi.DTOs
{
    public class AdminDashboardResponse
    {
        public int TotalUsers { get; set; }
        public int FreeUsers { get; set; }
        public int ProUsers { get; set; }
        public int AgencyUsers { get; set; }

        public int AdminUsers { get; set; }
        public int RegularUsers { get; set; }

        public int TotalPosts { get; set; }
        public int TotalAiPosts { get; set; }

        public int AiGenerationsToday { get; set; }
        public int AiGenerationsLast7Days { get; set; }

        public List<AdminTopAiUserResponse> TopAiUsersToday { get; set; } = new();
    }

    public class AdminTopAiUserResponse
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Plan { get; set; } = "Free";
        public int UsedToday { get; set; }
    }
}