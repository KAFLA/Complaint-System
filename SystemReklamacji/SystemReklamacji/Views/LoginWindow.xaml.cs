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
            string connectionString = "server=127.0.0.1;port=3306;database=complaintsystem;user=reklamacje_user;password=zaq1@WSX;SslMode=None;";
            // ZMIEŃ 'twoje_haslo' NA PRAWDZIWE HASŁO DO BAZY DANYCH!
            // W produkcyjnych aplikacjach, connection stringi nie powinny być zakodowane na stałe.

            // Tworzenie instancji repozytorium i serwisu
            UserRepository userRepository = new UserRepository(connectionString);
            AuthService authService = new AuthService(userRepository);

            // Ustawienie DataContext dla tego okna na nową instancję LoginViewModel
            LoginViewModel viewModel = new LoginViewModel(authService);
            this.DataContext = viewModel;

            // NOWO DODANA LINIA: Przekazanie akcji zamknięcia okna do ViewModelu
            viewModel.CloseAction = () => this.Close();
        }
    }
}
