using ReklamacjeSystem.Models;
using ReklamacjeSystem.Repositories;
using ReklamacjeSystem.Services;
using System;
using System.Collections.ObjectModel; // Dodaj to using
using System.Security;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows; // Do MessageBox.Show

namespace ReklamacjeSystem.ViewModels
{
    // ViewModel dla formularza dodawania/edycji reklamacji
    public class ComplaintEditViewModel : BaseViewModel
    {
        private readonly ComplaintRepository _complaintRepository;
        private readonly UserRepository _userRepository;
        private readonly PermissionService _permissionService;
        private readonly UserRole _currentUserRole;

        public Complaint OriginalComplaint { get; private set; }
        public bool IsNewComplaint { get; private set; }

        // Właściwości formularza
        private string _title;
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged(nameof(Title));
                // Sprawdź, czy SaveCommand jest zainicjowane przed wywołaniem RaiseCanExecuteChanged
                (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        private string _description;
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        private ComplaintStatus _status;
        public ComplaintStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        private ComplaintPriority _priority;
        public ComplaintPriority Priority
        {
            get => _priority;
            set
            {
                _priority = value;
                OnPropertyChanged(nameof(Priority));
            }
        }

        private User _assignedUser;
        public User AssignedUser
        {
            get => _assignedUser;
            set
            {
                _assignedUser = value;
                OnPropertyChanged(nameof(AssignedUser));
            }
        }

        public ObservableCollection<User> AvailableUsers { get; set; } = new ObservableCollection<User>();
        public ComplaintStatus[] AvailableStatuses { get; } = (ComplaintStatus[])Enum.GetValues(typeof(ComplaintStatus));
        public ComplaintPriority[] AvailablePriorities { get; } = (ComplaintPriority[])Enum.GetValues(typeof(ComplaintPriority));

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

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public Action CloseAction { get; set; }

        // ***** NOWO DODANY BEZPARAMETROWY KONSTRUKTOR DLA DESIGNERA I DOMYŚLNEGO ŁADOWANIA *****
        public ComplaintEditViewModel()
        {
            // Inicjalizacja komend, aby uniknąć NullReferenceException w czasie projektowania
            SaveCommand = new RelayCommand(async obj => await SaveComplaint(), obj => CanSaveComplaint());
            CancelCommand = new RelayCommand(obj => Cancel());

            // Ustaw wartości domyślne, aby projektant XAML widział coś sensownego
            Title = "Nowa Reklamacja (podgląd)";
            Description = "Opis podglądu";
            Status = ComplaintStatus.New;
            Priority = ComplaintPriority.Medium;
            AvailableUsers = new ObservableCollection<User>(); // Pusta lista użytkowników dla podglądu
            StatusMessage = "Wypełnij formularz.";

            // Repozytoria i serwisy będą null, jeśli ten konstruktor jest używany przez designera,
            // ale to jest akceptowalne dla celów projektowania.
            _complaintRepository = null;
            _userRepository = null;
            _permissionService = null;
            _currentUserRole = UserRole.Employee; // Domyślna rola dla podglądu
        }


        // Konstruktor dla nowej reklamacji (używany w czasie działania aplikacji)
        public ComplaintEditViewModel(ComplaintRepository complaintRepository, UserRepository userRepository, PermissionService permissionService, UserRole currentUserRole)
        {
            _complaintRepository = complaintRepository;
            _userRepository = userRepository;
            _permissionService = permissionService;
            _currentUserRole = currentUserRole;
            IsNewComplaint = true;

            Title = string.Empty;
            Description = string.Empty;
            Status = ComplaintStatus.New;
            Priority = ComplaintPriority.Medium;

            SaveCommand = new RelayCommand(async obj => await SaveComplaint(), obj => CanSaveComplaint());
            CancelCommand = new RelayCommand(obj => Cancel());

            // Ładuj użytkowników tylko jeśli repozytorium jest dostępne
            if (_userRepository != null)
            {
                Task.Run(async () => await LoadAvailableUsersAsync());
            }
        }

        // Konstruktor dla edycji istniejącej reklamacji (używany w czasie działania aplikacji)
        public ComplaintEditViewModel(Complaint complaint, ComplaintRepository complaintRepository, UserRepository userRepository, PermissionService permissionService, UserRole currentUserRole)
            : this(complaintRepository, userRepository, permissionService, currentUserRole) // Wywołaj konstruktor dla nowej reklamacji, aby zainicjować repozytoria i komendy
        {
            OriginalComplaint = complaint;
            IsNewComplaint = false;

            Title = complaint.Title;
            Description = complaint.Description;
            Status = complaint.Status;
            Priority = complaint.Priority;
            AssignedUser = complaint.User; // Powinno być już załadowane z ComplaintListViewModel
        }

        private async Task LoadAvailableUsersAsync()
        {
            if (_userRepository != null)
            {
                AvailableUsers.Clear();
                var users = await _userRepository.GetAllAsync();
                foreach (var user in users)
                {
                    AvailableUsers.Add(user);
                }
            }
        }

        private async Task SaveComplaint()
        {
            if (!CanSaveComplaint())
            {
                StatusMessage = "Tytuł reklamacji nie może być pusty.";
                return;
            }

            // Sprawdź, czy repozytoria są zainicjalizowane (nie null z domyślnego konstruktora)
            if (_complaintRepository == null || _permissionService == null)
            {
                MessageBox.Show("Błąd wewnętrzny: Serwisy danych nie są zainicjalizowane. Używasz wersji deweloperskiej ViewModelu.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                if (IsNewComplaint)
                {
                    if (!_permissionService.HasPermission(_currentUserRole, PermissionAction.CreateComplaint))
                    {
                        StatusMessage = "Brak uprawnień do dodawania reklamacji.";
                        MessageBox.Show("Brak uprawnień do dodawania reklamacji.", "Brak dostępu", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var newComplaint = new Complaint
                    {
                        Title = Title,
                        Description = Description,
                        Status = ComplaintStatus.New,
                        Priority = Priority,
                        CreatedAt = DateTime.Now,
                        UserId = AssignedUser?.Id
                    };
                    await _complaintRepository.AddAsync(newComplaint);
                    StatusMessage = "Reklamacja dodana pomyślnie!";
                }
                else
                {
                    if (!_permissionService.HasPermission(_currentUserRole, PermissionAction.EditComplaints))
                    {
                        StatusMessage = "Brak uprawnień do edycji reklamacji.";
                        MessageBox.Show("Brak uprawnień do edycji reklamacji.", "Brak dostępu", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    OriginalComplaint.Title = Title;
                    OriginalComplaint.Description = Description;
                    OriginalComplaint.Status = Status;
                    OriginalComplaint.Priority = Priority;
                    OriginalComplaint.UserId = AssignedUser?.Id;

                    await _complaintRepository.UpdateAsync(OriginalComplaint);
                    StatusMessage = "Reklamacja zaktualizowana pomyślnie!";
                }
                CloseAction?.Invoke();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Błąd zapisu reklamacji: {ex.Message}";
                MessageBox.Show($"Błąd zapisu reklamacji: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanSaveComplaint()
        {
            return !string.IsNullOrWhiteSpace(Title);
        }

        private void Cancel()
        {
            CloseAction?.Invoke();
        }
    }
}
