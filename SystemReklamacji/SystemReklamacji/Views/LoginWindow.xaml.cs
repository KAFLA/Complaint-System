using ReklamacjeSystem.ViewModels;
using ReklamacjeSystem.Services;
using System.Windows;
using ReklamacjeSystem.Repositories;

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
            // WAŻNE: ZMIEŃ 'twoje_haslo' NA PRAWDZIWE HASŁO DO UŻYTKOWNIKA BAZY DANYCH!
            string connectionString = "server=localhost;port=3306;database=reklamacje_db;user=kaflisz;password=zaq1@WSX;SslMode=None;";
            // Jeśli używasz użytkownika 'reklamacje_user' stworzonego skryptem SQL, hasło to 'zaq1@WSX':
            // string connectionString = "server=localhost;port=3306;database=reklamacje_db;user=reklamacje_user;password=zaq1@WSX;SslMode=None;"; 

            // Tworzenie instancji repozytorium i serwisu
            UserRepository userRepository = new UserRepository(connectionString);
            AuthService authService = new AuthService(userRepository);

            // Ustawienie DataContext dla tego okna na nową instancję LoginViewModel
            LoginViewModel viewModel = new LoginViewModel(authService);
            this.DataContext = viewModel;
            viewModel.CloseAction = () => this.Close(); // Przekazujemy akcję zamknięcia okna
        }
    }
}
