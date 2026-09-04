namespace banhmihanhphuc.Models
{
    public class User
    {
        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string? FullName { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public List<UserRole> UserRoles { get; set; } = new();

        public List<Order> Orders { get; set; } = new();
    }
}