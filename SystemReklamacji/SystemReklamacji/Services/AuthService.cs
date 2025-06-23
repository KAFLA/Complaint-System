using ReklamacjeSystem.Models;
using ReklamacjeSystem.Repositories;
using System.Threading.Tasks;
using BCrypt.Net; // Do hashowania haseł

namespace ReklamacjeSystem.Services
{
    // Implementacja usługi uwierzytelniania
    public class AuthService : IAuthService
    {
        private readonly UserRepository _userRepository; // Repozytorium do zarządzania użytkownikami

        // Konstruktor przyjmujący instancję UserRepository
        public AuthService(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        // Metoda rejestracji użytkownika
        public async Task<User> Register(string username, string email, string password, UserRole role)
        {
            // Sprawdź, czy użytkownik o podanej nazwie lub emailu już istnieje
            var existingUsers = await _userRepository.GetAllAsync();
            foreach (var user in existingUsers)
            {
                if (user.Username == username)
                {
                    throw new System.ArgumentException("Nazwa użytkownika już istnieje.");
                }
                if (user.Email == email)
                {
                    throw new System.ArgumentException("Adres email już istnieje.");
                }
            }

            // Zahashuj hasło przed zapisaniem do bazy danych
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            // Stwórz nowego użytkownika
            var newUser = new User
            {
                Username = username,
                Email = email,
                PasswordHash = passwordHash,
                Role = role,
                CreatedAt = System.DateTime.Now
            };

            await _userRepository.AddAsync(newUser); // Dodaj użytkownika do bazy danych
            return newUser;
        }

        // Metoda logowania użytkownika
        public async Task<User> Login(string username, string password)
        {
            var users = await _userRepository.GetAllAsync();
            User user = null;
            foreach (var u in users)
            {
                if (u.Username == username)
                {
                    user = u;
                    break;
                }
            }

            if (user == null)
            {
                return null; // Użytkownik nie znaleziony
            }

            // Zweryfikuj hasło
            if (BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return user; // Logowanie pomyślne
            }
            else
            {
                return null; // Niepoprawne hasło
            }
        }
    }
}
