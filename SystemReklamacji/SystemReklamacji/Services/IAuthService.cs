using ReklamacjeSystem.Models;
using System.Threading.Tasks;

namespace ReklamacjeSystem.Services
{
    // Interfejs dla usług uwierzytelniania i autoryzacji
    public interface IAuthService
    {
        // Metoda do rejestracji nowego użytkownika
        Task<User> Register(string username, string email, string password, UserRole role);

        // Metoda do logowania użytkownika
        Task<User> Login(string username, string password);
    }
}
