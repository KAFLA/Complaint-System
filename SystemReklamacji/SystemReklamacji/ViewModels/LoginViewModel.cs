using System;
using System.Security;
using System.Windows;
using System.Windows.Input;
using ReklamacjeSystem.Models;
using ReklamacjeSystem.Services; // Potrzebne do AuthService
using ReklamacjeSystem.Views; // Potrzebne do MainWindow

namespace ReklamacjeSystem.ViewModels
{
    // ViewModel dla okna logowania/rejestracji
    public class LoginViewModel : BaseViewModel // BaseViewModel zapewni INotifyPropertyChanged
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

        // Kolekcja dostępnych ról dla ComboBoxa w rejestracji
        public UserRole[] AvailableRoles { get; } = (UserRole[])Enum.GetValues(typeof(UserRole));

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }

        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }

        private readonly IAuthService _authService;

        // WŁAŚCIWOŚĆ: Akcja do zamknięcia okna logowania
        public Action CloseAction { get; set; }


        // Konstruktor LoginViewModel
        public LoginViewModel(IAuthService authService)
        {
            _authService = authService;
            LoginCommand = new RelayCommand(async (param) => await Login(param));
            RegisterCommand = new RelayCommand(async (param) => await Register(param));

            // Domyślna rola dla rejestracji (np. Employee)
            SelectedRole = UserRole.Employee;
        }

        // Metoda do obsługi logowania
        private async Task Login(object parameter)
        {
            if (string.IsNullOrWhiteSpace(Username) || Password == null || Password.Length == 0)
            {
                StatusMessage = "Wprowadź nazwę użytkownika i hasło.";
                return;
            }

            try
            {
                // Konwersja SecureString na zwykły string dla celów uwierzytelniania
                string plainPassword = new System.Net.NetworkCredential(string.Empty, Password).Password;

                User loggedInUser = await _authService.Login(Username, plainPassword);

                if (loggedInUser != null)
                {
                    StatusMessage = $"Zalogowano pomyślnie jako {loggedInUser.Username} ({loggedInUser.Role})!";

                    // Stwórz i pokaż główne okno aplikacji
                    MainWindow main = new MainWindow(loggedInUser); // Przekaż zalogowanego użytkownika
                    main.Show();

                    // Bezpośrednie użycie CloseAction do zamknięcia okna logowania
                    CloseAction?.Invoke(); // Użyj operatora ?. na wypadek, gdyby CloseAction było null (choć nie powinno)
                }
                else
                {
                    StatusMessage = "Błąd logowania: nieprawidłowa nazwa użytkownika lub hasło.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Wystąpił błąd podczas logowania: {ex.Message}";
            }
        }

        // Metoda do obsługi rejestracji
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

                User registeredUser = await _authService.Register(Username, Email, plainPassword, SelectedRole);

                if (registeredUser != null)
                {
                    StatusMessage = $"Użytkownik {registeredUser.Username} ({registeredUser.Role}) zarejestrowany pomyślnie!";
                    // Można automatycznie zalogować lub wyświetlić komunikat i poprosić o zalogowanie
                }
                else
                {
                    StatusMessage = "Błąd rejestracji.";
                }
            }
            catch (ArgumentException ex)
            {
                StatusMessage = $"Błąd rejestracji: {ex.Message}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Wystąpił błąd podczas rejestracji: {ex.Message}";
            }
        }
    }

    // Prosta klasa bazowa dla ViewModeli, implementująca INotifyPropertyChanged
    // To jest kluczowe dla działania Data Bindingu w WPF
    public class BaseViewModel : System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}
