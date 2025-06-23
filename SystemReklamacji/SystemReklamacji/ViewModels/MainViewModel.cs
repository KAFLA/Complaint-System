using ReklamacjeSystem.Models;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Windows;

namespace ReklamacjeSystem.ViewModels
{
    // ViewModel dla głównego okna aplikacji
    public class MainViewModel : BaseViewModel
    {
        private User _currentUser;
        public User CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                OnPropertyChanged(nameof(CurrentUser));
                OnPropertyChanged(nameof(IsAdmin)); // Aktualizuj właściwości uprawnień
                OnPropertyChanged(nameof(IsManager));
                OnPropertyChanged(nameof(IsEmployee));
            }
        }

        public bool IsAdmin => CurrentUser?.Role == UserRole.Admin;
        public bool IsManager => CurrentUser?.Role == UserRole.Manager;
        public bool IsEmployee => CurrentUser?.Role == UserRole.Employee;

        private BaseViewModel _currentViewModel;
        public BaseViewModel CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                _currentViewModel = value;
                OnPropertyChanged(nameof(CurrentViewModel));
            }
        }

        public ICommand ShowComplaintsCommand { get; }
        public ICommand ShowUsersCommand { get; }
        public ICommand ShowSolutionsCommand { get; } // Nowy command dla rozwiązań
        public ICommand LogoutCommand { get; }

        // Zależności do repozytoriów/serwisów (potrzebne do inicjalizacji innych ViewModeli)
        private readonly string _connectionString;
        private readonly Services.PermissionService _permissionService; // Dodajemy PermissionService

        public MainViewModel(User currentUser, string connectionString)
        {
            CurrentUser = currentUser;
            _connectionString = connectionString;
            _permissionService = new Services.PermissionService(); // Inicjalizacja PermissionService

            ShowComplaintsCommand = new RelayCommand(async obj => await ShowComplaints());
            ShowUsersCommand = new RelayCommand(obj => ShowUsers(), obj => _permissionService.CanEditUsers(CurrentUser.Role)); // Sprawdzenie uprawnień
            ShowSolutionsCommand = new RelayCommand(obj => ShowSolutions()); // Dodajemy komendę
            LogoutCommand = new RelayCommand(obj => Logout());

            // Domyślnie pokaż listę reklamacji po zalogowaniu
            Task.Run(async () => await ShowComplaints());
        }

        private async Task ShowComplaints()
        {
            // Pamiętaj, aby przekazać odpowiednie zależności do ComplaintListViewModel
            var complaintRepository = new Repositories.ComplaintRepository(_connectionString);
            var userRepository = new Repositories.UserRepository(_connectionString); // Potrzebne do pobrania nazw użytkowników
            CurrentViewModel = new ComplaintListViewModel(complaintRepository, userRepository, _permissionService, CurrentUser.Role);
            await ((ComplaintListViewModel)CurrentViewModel).LoadComplaintsAsync(); // Załaduj dane
        }

        private void ShowUsers()
        {
            if (_permissionService.CanEditUsers(CurrentUser.Role)) // Podwójne sprawdzenie uprawnień
            {
                var userRepository = new Repositories.UserRepository(_connectionString);
                CurrentViewModel = new UserManagementViewModel(userRepository, _permissionService, CurrentUser.Role);
                // UserManagementViewModel będzie musiał mieć metodę LoadUsersAsync()
                // której teraz nie ma, więc dodamy ją w przyszłości.
            }
            else
            {
                // Opcjonalnie: wyświetl komunikat o braku uprawnień
                System.Windows.MessageBox.Show("Brak uprawnień do zarządzania użytkownikami.", "Brak dostępu", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ShowSolutions()
        {
            var solutionRepository = new Repositories.SolutionRepository(_connectionString);
            CurrentViewModel = new SolutionListViewModel(solutionRepository, _permissionService, CurrentUser.Role);
            // Podobnie jak wyżej, SolutionListViewModel będzie potrzebował metody LoadSolutionsAsync()
            // którą stworzymy w późniejszym etapie.
        }

        private void Logout()
        {
            // Logika wylogowania
            // Np. pokaż ponownie okno logowania i zamknij bieżące
            System.Windows.Application.Current.MainWindow.Hide();
            Views.LoginWindow loginWindow = new Views.LoginWindow();
            loginWindow.Show();
            System.Windows.Application.Current.MainWindow.Close();
        }
    }
}
