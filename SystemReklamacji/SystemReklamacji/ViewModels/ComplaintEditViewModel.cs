using ReklamacjeSystem.Models;
using ReklamacjeSystem.Repositories;
using ReklamacjeSystem.Services;
using System;
using System.Collections.ObjectModel;
using System.Security;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

namespace ReklamacjeSystem.ViewModels
{
    // ViewModel dla formularza dodawania/edycji/podglądu reklamacji
    public class ComplaintEditViewModel : BaseViewModel
    {
        private readonly ComplaintRepository _complaintRepository;
        private readonly UserRepository _userRepository;
        private readonly PermissionService _permissionService;
        private readonly UserRole _currentUserRole;

        public Complaint OriginalComplaint { get; private set; } // Oryginalny obiekt reklamacji (jeśli edycja)
        public bool IsNewComplaint { get; private set; } // Flaga, czy to nowa reklamacja czy edycja

        private bool _isEditMode;
        // Właściwość kontrolująca, czy formularz jest w trybie edycji (true) czy podglądu (false)
        public bool IsEditMode
        {
            get => _isEditMode;
            set
            {
                _isEditMode = value;
                OnPropertyChanged(nameof(IsEditMode));
                OnPropertyChanged(nameof(IsViewMode)); // Poinformuj o zmianie IsViewMode
                OnPropertyChanged(nameof(WindowTitle)); // Zaktualizuj tytuł okna
                OnPropertyChanged(nameof(HeaderText));  // Zaktualizuj nagłówek
                // Bezpieczne wywołanie RaiseCanExecuteChanged (sprawdzamy, czy komendy nie są null)
                (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (EditCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        // Właściwość pomocnicza dla trybu podglądu
        public bool IsViewMode => !IsEditMode;

        // Właściwości formularza (dane reklamacji)
        private string _title;
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged(nameof(Title));
                ((RelayCommand)SaveCommand)?.RaiseCanExecuteChanged(); // Bezpieczne wywołanie
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

        // Dynamiczny tytuł okna (zależy od trybu i czy to nowa reklamacja)
        public string WindowTitle => IsNewComplaint ? "Dodaj Nową Reklamację" : (IsEditMode ? $"Edytuj Reklamację: {OriginalComplaint?.Title}" : $"Podgląd Reklamacji: {OriginalComplaint?.Title}");

        // Dynamiczny nagłówek w oknie
        public string HeaderText => IsNewComplaint ? "Dodaj Nową Reklamację" : (IsEditMode ? $"Edytuj Reklamację (ID: {OriginalComplaint?.Id})" : $"Szczegóły Reklamacji (ID: {OriginalComplaint?.Id})");


        // Komendy
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand EditCommand { get; } // Komenda do przełączania w tryb edycji
        public Action CloseAction { get; set; } // Akcja do zamknięcia okna

        // Domyślny konstruktor dla design-time (Visual Studio Designer)
        public ComplaintEditViewModel()
        {
            // ***** Poczatek inicjalizacji komend w konstruktorze domyślnym *****
            // Zapewnienie, że komendy są zainicjalizowane przed próbą wywołania RaiseCanExecuteChanged
            _permissionService = new PermissionService(); // Inicjalizacja dla designera, aby uniknąć null
            _currentUserRole = UserRole.Admin; // Domyślna rola dla designera preview

            SaveCommand = new RelayCommand(async obj => await SaveComplaint(), obj => CanSaveComplaint());
            CancelCommand = new RelayCommand(obj => Cancel());
            EditCommand = new RelayCommand(obj => IsEditMode = true, obj => IsViewMode && _permissionService.HasPermission(_currentUserRole, PermissionAction.EditComplaints));
            // ***** Koniec inicjalizacji komend w konstruktorze domyślnym *****

            // Ustawienie domyślnych wartości dla podglądu w designerze
            Title = "Nowa Reklamacja (podgląd)";
            Description = "Opis podglądu";
            Status = ComplaintStatus.New;
            Priority = ComplaintPriority.Medium;
            AvailableUsers = new ObservableCollection<User>();
            StatusMessage = "Wypełnij formularz.";
            IsNewComplaint = true; // Dla designera domyślnie jako 'nowa'
            IsEditMode = true; // Dla designera domyślnie w trybie edycji, żeby widział pola

            _complaintRepository = null;
            _userRepository = null;
        }


        // Konstruktor dla nowej reklamacji (używany w czasie działania aplikacji)
        public ComplaintEditViewModel(ComplaintRepository complaintRepository, UserRepository userRepository, PermissionService permissionService, UserRole currentUserRole)
        {
            _complaintRepository = complaintRepository;
            _userRepository = userRepository;
            _permissionService = permissionService;
            _currentUserRole = currentUserRole;

            // ***** Poczatek inicjalizacji komend w konstruktorze runtime *****
            SaveCommand = new RelayCommand(async obj => await SaveComplaint(), obj => CanSaveComplaint());
            CancelCommand = new RelayCommand(obj => Cancel());
            EditCommand = new RelayCommand(obj => IsEditMode = true, obj => IsViewMode && _permissionService.HasPermission(_currentUserRole, PermissionAction.EditComplaints));
            // ***** Koniec inicjalizacji komend w konstruktorze runtime *****

            IsNewComplaint = true;
            IsEditMode = true; // Nowa reklamacja zawsze zaczyna się w trybie edycji

            Title = string.Empty;
            Description = string.Empty;
            Status = ComplaintStatus.New;
            Priority = ComplaintPriority.Medium;

            // Asynchroniczne ładowanie dostępnych użytkowników
            if (_userRepository != null)
            {
                Task.Run(async () => await LoadAvailableUsersAsync()).ConfigureAwait(false);
            }
        }

        // Konstruktor dla edycji/podglądu istniejącej reklamacji (używany w czasie działania aplikacji)
        public ComplaintEditViewModel(Complaint complaint, ComplaintRepository complaintRepository, UserRepository userRepository, PermissionService permissionService, UserRole currentUserRole)
            : this(complaintRepository, userRepository, permissionService, currentUserRole) // Wywołaj konstruktor dla nowej reklamacji, aby zainicjować repozytoria i komendy
        {
            OriginalComplaint = complaint;
            IsNewComplaint = false;
            IsEditMode = false; // Istniejąca reklamacja zaczyna w trybie podglądu domyślnie

            Title = complaint.Title;
            Description = complaint.Description;
            Status = complaint.Status;
            Priority = complaint.Priority;
            AssignedUser = complaint.User; // Powinno być już załadowane z ComplaintListViewModel
        }

        // Ładowanie listy dostępnych użytkowników do przypisania
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

        // Logika zapisu reklamacji
        private async Task SaveComplaint()
        {
            if (!CanSaveComplaint())
            {
                StatusMessage = "Tytuł reklamacji nie może być pusty.";
                return;
            }

            // Sprawdzenie, czy repozytoria i serwisy są zainicjalizowane (nie null z konstruktora design-time)
            if (_complaintRepository == null || _permissionService == null)
            {
                MessageBox.Show("Błąd wewnętrzny: Serwisy danych nie są zainicjalizowane. Używasz wersji deweloperskiej ViewModelu.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                if (IsNewComplaint)
                {
                    // Sprawdzenie uprawnień do tworzenia reklamacji
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
                        Status = ComplaintStatus.New, // Nowa reklamacja zawsze zaczyna się ze statusem "New"
                        Priority = Priority,
                        CreatedAt = DateTime.Now,
                        UserId = AssignedUser?.Id // Przypisany użytkownik
                    };
                    await _complaintRepository.AddAsync(newComplaint);
                    StatusMessage = "Reklamacja dodana pomyślnie!";
                    IsNewComplaint = false; // Po zapisie już nie jest nową reklamacją
                    OriginalComplaint = newComplaint; // Ustaw oryginalną reklamację
                    IsEditMode = false; // Przełącz w tryb podglądu po dodaniu
                }
                else
                {
                    // Sprawdzenie uprawnień do edycji reklamacji
                    if (!_permissionService.HasPermission(_currentUserRole, PermissionAction.EditComplaints))
                    {
                        StatusMessage = "Brak uprawnień do edycji reklamacji.";
                        MessageBox.Show("Brak uprawnień do edycji reklamacji.", "Brak dostępu", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Aktualizacja oryginalnego obiektu reklamacji danymi z formularza
                    OriginalComplaint.Title = Title;
                    OriginalComplaint.Description = Description;
                    OriginalComplaint.Status = Status;
                    OriginalComplaint.Priority = Priority;
                    OriginalComplaint.UserId = AssignedUser?.Id;

                    await _complaintRepository.UpdateAsync(OriginalComplaint);
                    StatusMessage = "Reklamacja zaktualizowana pomyślnie!";
                    IsEditMode = false; // Przełącz w tryb podglądu po edycji
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Błąd zapisu reklamacji: {ex.Message}";
                MessageBox.Show($"Błąd zapisu reklamacji: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Warunek wykonania komendy SaveCommand
        private bool CanSaveComplaint()
        {
            return IsEditMode && !string.IsNullOrWhiteSpace(Title); // Zapisuj tylko w trybie edycji i gdy tytuł nie jest pusty
        }

        // Logika anulowania
        private void Cancel()
        {
            // Jeśli to nowa reklamacja, po prostu zamknij okno.
            // Jeśli to edycja, przywróć oryginalne wartości i przejdź w tryb podglądu.
            if (IsNewComplaint)
            {
                CloseAction?.Invoke();
            }
            else
            {
                // Przywróć oryginalne dane (opcjonalnie, jeśli chcesz, aby użytkownik widział stare dane po anulowaniu edycji)
                Title = OriginalComplaint.Title;
                Description = OriginalComplaint.Description;
                Status = OriginalComplaint.Status;
                Priority = OriginalComplaint.Priority;
                AssignedUser = OriginalComplaint.User; // Przypisany użytkownik

                IsEditMode = false; // Wróć do trybu podglądu
            }
        }
    }
}
