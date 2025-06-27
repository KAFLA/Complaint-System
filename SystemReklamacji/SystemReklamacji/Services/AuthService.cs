using ReklamacjeSystem.Models;
using ReklamacjeSystem.Repositories;
using System.Threading.Tasks;
using BCrypt.Net;
using System.Diagnostics; // Dodaj to dla Debug.WriteLine

namespace ReklamacjeSystem.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserRepository _userRepository;

        public AuthService(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

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
            Debug.WriteLine($"AuthService - Register: Registering user '{username}'. Hashed password: '{passwordHash}'"); // Debug

            var newUser = new User
            {
                Username = username,
                Email = email,
                PasswordHash = passwordHash,
                Role = role,
                CreatedAt = System.DateTime.Now // Upewnij się, że DateTime.Now jest obsługiwane przez MySQL
            };

            await _userRepository.AddAsync(newUser);
            Debug.WriteLine($"AuthService - Register: User '{username}' added to database successfully."); // Debug
            return newUser;
        }

        public async Task<User> Login(string username, string password)
        {
            Debug.WriteLine($"AuthService - Login: Attempting login for username: '{username}'"); // Debug
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
                Debug.WriteLine($"AuthService - Login: User '{username}' not found in database."); // Debug
                return null;
            }

            Debug.WriteLine($"AuthService - Login: User '{username}' found. Stored hash: '{user.PasswordHash}'. Attempting to verify provided password."); // Debug

            try
            {
                if (BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    Debug.WriteLine($"AuthService - Login: Login successful for user '{username}'. Password matched."); // Debug
                    return user;
                }
                else
                {
                    Debug.WriteLine($"AuthService - Login: Incorrect password for user '{username}'. Provided plain password (for debug): '{password}'. Stored hash: '{user.PasswordHash}'"); // Debug
                    return null;
                }
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine($"AuthService - Login: BCrypt verification error for user '{username}': {ex.Message}. StackTrace: {ex.StackTrace}"); // Debug
                return null;
            }
        }
    }
}
