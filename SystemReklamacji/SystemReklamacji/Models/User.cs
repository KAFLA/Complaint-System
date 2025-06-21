namespace ReklamacjeSystem.Models
{
    public enum UserRole
    {
        Admin,
        Manager,
        Employee
    }

    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; } // Tu będzie przechowywany zahashowany hasło
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}