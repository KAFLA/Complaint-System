using ReklamacjeSystem.Models;
using System;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Windows;

namespace ReklamacjeSystem.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        // Delegat do zamknięcia okna
        public Action CloseAction { get; set; }

        private User _currentUser;
        public User CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                OnPropertyChanged(nameof(CurrentUser));
                OnPropertyChanged(nameof(IsAdmin));
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
        public ICommand ShowSolutionsCommand { get; }
        public ICommand LogoutCommand { get; }

        private readonly string _connectionString;
        private readonly Services.PermissionService _permissionService;

        public MainViewModel(User currentUser, string connectionString)
        {
            CurrentUser = currentUser;
            _connectionString = connectionString;
            _permissionService = new Services.PermissionService();

            ShowComplaintsCommand = new RelayCommand(async obj => await ShowComplaints());
            ShowUsersCommand = new RelayCommand(obj => ShowUsers(), obj => _permissionService.CanEditUsers(CurrentUser.Role));
            ShowSolutionsCommand = new RelayCommand(obj => ShowSolutions());
            LogoutCommand = new RelayCommand(obj => Logout());

            Task.Run(async () => await ShowComplaints());
        }

        private async Task ShowComplaints()
        {
            var complaintRepository = new Repositories.ComplaintRepository(_connectionString);
            var userRepository = new Repositories.UserRepository(_connectionString);
            CurrentViewModel = new ComplaintListViewModel(complaintRepository, userRepository, _permissionService, CurrentUser.Role);
            await ((ComplaintListViewModel)CurrentViewModel).LoadComplaintsAsync();
        }

        private void ShowUsers()
        {
            if (_permissionService.CanEditUsers(CurrentUser.Role))
            {
                var userRepository = new Repositories.UserRepository(_connectionString);
                CurrentViewModel = new UserManagementViewModel(userRepository, _permissionService, CurrentUser.Role);
            }
            else
            {
                MessageBox.Show("Brak uprawnień do zarządzania użytkownikami.", "Brak dostępu", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ShowSolutions()
        {
            var solutionRepository = new Repositories.SolutionRepository(_connectionString);
            CurrentViewModel = new SolutionListViewModel(solutionRepository, _permissionService, CurrentUser.Role);
        }

        private void Logout()
        {
            // Otwórz okno logowania
            Views.LoginWindow loginWindow = new Views.LoginWindow();
            loginWindow.Show();

            // Zamknij MainWindow (obecne okno)
            CloseAction?.Invoke();
        }
    }
}
