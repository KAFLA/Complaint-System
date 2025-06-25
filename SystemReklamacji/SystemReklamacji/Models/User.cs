using System; // For DateTime

namespace ReklamacjeSystem.Models
{
    // Enum definiujący role użytkowników
    public enum UserRole
    {
        Admin,
        Manager,
        Employee
    }

    // Klasa reprezentująca encję użytkownika w systemie
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; } // Zapisujemy zahashowane hasło
        public UserRole Role { get; set; } // Rola użytkownika (enum)
        public DateTime CreatedAt { get; set; } // Data utworzenia konta
    }
}
