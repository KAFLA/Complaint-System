using ReklamacjeSystem.ViewModels;
using ReklamacjeSystem.Services; // Potrzebne do AuthService
using System.Windows;
using ReklamacjeSystem.Repositories; // Potrzebne do UserRepository

namespace ReklamacjeSystem.Views
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();

            // Konfiguracja stringa połączenia do bazy danych MySQL
            // Pamiętaj, aby dostosować te dane do swojej konfiguracji MySQL
            string connectionString = "server=localhost;port=3306;database=reklamacje_db;user=root;password=twoje_haslo;";
            // ZMIEŃ 'twoje_haslo' NA PRAWDZIWE HASŁO DO BAZY DANYCH!
            // W produkcyjnych aplikacjach, connection stringi nie powinny być zakodowane na stałe.

            // Tworzenie instancji repozytorium i serwisu
            UserRepository userRepository = new UserRepository(connectionString);
            AuthService authService = new AuthService(userRepository);

            // Ustawienie DataContext dla tego okna na nową instancję LoginViewModel
            this.DataContext = new LoginViewModel(authService);
        }
    }
}
