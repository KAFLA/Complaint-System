using System;
using System.Security;
using System.Windows;
using System.Windows.Input;
using ReklamacjeSystem.Models;
using ReklamacjeSystem.Services;
using ReklamacjeSystem.Views;
using System.Diagnostics; // Dodaj to dla Debug.WriteLine
using System.Threading.Tasks; // Dodaj to dla Task

namespace ReklamacjeSystem.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private string _username;
        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
            }
        }

        private SecureString _password;
        public SecureString Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        private string _email;
        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged(nameof(Email));
            }
        }

        private UserRole _selectedRole;
        public UserRole SelectedRole
        {
            get => _selectedRole;
            set
            {
                _selectedRole = value;
                OnPropertyChanged(nameof(SelectedRole));
            }
        }

        public UserRole[] AvailableRoles { get; } = (UserRole[])Enum.GetValues(typeof(UserRole));

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
                Debug.WriteLine($"StatusMessage: {value}"); // Debug
            }
        }

        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }
        public Action CloseAction { get; set; } // Akcja do zamknięcia okna

        private readonly IAuthService _authService;

        public LoginViewModel(IAuthService authService)
        {
            _authService = authService;
            LoginCommand = new RelayCommand(async (param) => await Login(param));
            RegisterCommand = new RelayCommand(async (param) => await Register(param));
            SelectedRole = UserRole.Employee;
        }

        private async Task Login(object parameter)
        {
            if (string.IsNullOrWhiteSpace(Username) || Password == null || Password.Length == 0)
            {
                StatusMessage = "Wprowadź nazwę użytkownika i hasło.";
                return;
            }

            try
            {
                string plainPassword = new System.Net.NetworkCredential(string.Empty, Password).Password;
                Debug.WriteLine($"LoginViewModel: Attempting login for '{Username}' with plain password (from SecureString): '{plainPassword}'"); // Debug

                User loggedInUser = await _authService.Login(Username, plainPassword);

                if (loggedInUser != null)
                {
                    StatusMessage = $"Zalogowano pomyślnie jako {loggedInUser.Username} ({loggedInUser.Role})!";

                    // Zamiast Application.Current.MainWindow.Hide/Close, używamy CloseAction
                    // Upewnij się, że LoginWindow.xaml.cs ustawia viewModel.CloseAction = () => this.Close();
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MainWindow main = new MainWindow(loggedInUser); // Utwórz nowe główne okno
                        main.Show(); // Pokaż główne okno
                        CloseAction?.Invoke(); // Zamknij okno logowania
                    });
                }
                else
                {
                    StatusMessage = "Błąd logowania: nieprawidłowa nazwa użytkownika lub hasło.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Wystąpił błąd podczas logowania: {ex.Message}";
                Debug.WriteLine($"Login exception in LoginViewModel for user '{Username}': {ex.Message}"); // Debug
            }
        }

        private async Task Register(object parameter)
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Email) || Password == null || Password.Length == 0)
            {
                StatusMessage = "Wypełnij wszystkie pola rejestracji.";
                return;
            }

            try
            {
                string plainPassword = new System.Net.NetworkCredential(string.Empty, Password).Password;
                Debug.WriteLine($"LoginViewModel: Attempting registration for '{Username}' with email '{Email}' and plain password: '{plainPassword}'"); // Debug

                User registeredUser = await _authService.Register(Username, Email, plainPassword, SelectedRole);

                if (registeredUser != null)
                {
                    StatusMessage = $"Użytkownik {registeredUser.Username} ({registeredUser.Role}) zarejestrowany pomyślnie!";
                    // Można opcjonalnie wyczyścić pola formularza tutaj
                    Username = string.Empty;
                    Email = string.Empty;
                    Password = new SecureString(); // Wyczyść SecureString
                }
                else
                {
                    StatusMessage = "Błąd rejestracji.";
                }
            }
            catch (ArgumentException ex)
            {
                StatusMessage = $"Błąd rejestracji: {ex.Message}";
                Debug.WriteLine($"Registration ArgumentException: {ex.Message}"); // Debug
            }
            catch (Exception ex)
            {
                StatusMessage = $"Wystąpił błąd podczas rejestracji: {ex.Message}";
                Debug.WriteLine($"Registration Exception: {ex.Message}"); // Debug
            }
        }
    }
}
