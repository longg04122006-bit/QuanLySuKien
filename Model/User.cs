namespace Model
{
    public class User
    {
        public int UserId { get; set; }

        public string Username { get; set; } = "";

        public string PasswordHash { get; set; } = "";

        public string FullName { get; set; } = "";

        public string Email { get; set; } = "";

        public string Phone { get; set; } = "";

        public int RoleId { get; set; }

        public bool IsActive { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}