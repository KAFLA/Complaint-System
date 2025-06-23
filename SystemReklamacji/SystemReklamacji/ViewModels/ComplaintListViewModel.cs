using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using ReklamacjeSystem.Models;
using ReklamacjeSystem.Repositories;
using ReklamacjeSystem.Services;
using System.Windows; // Do wyświetlania MessageBox

namespace ReklamacjeSystem.ViewModels
{
    // ViewModel dla listy reklamacji
    public class ComplaintListViewModel : BaseViewModel
    {
        private readonly ComplaintRepository _complaintRepository; // Repozytorium do zarządzania reklamacjami
        private readonly UserRepository _userRepository; // Repozytorium do pobierania nazw użytkowników
        private readonly PermissionService _permissionService; // Usługa do sprawdzania uprawnień
        private readonly UserRole _currentUserRole; // Rola aktualnie zalogowanego użytkownika

        private ObservableCollection<Complaint> _complaints;
        // Kolekcja reklamacji wyświetlanych w widoku. Używamy ObservableCollection,
        // aby UI automatycznie aktualizowało się po dodaniu/usunięciu/zmianie elementów.
        public ObservableCollection<Complaint> Complaints
        {
            get => _complaints;
            set
            {
                _complaints = value;
                OnPropertyChanged(nameof(Complaints)); // Powiadamiamy UI o zmianie wartości
            }
        }

        private Complaint _selectedComplaint;
        // Wybrana reklamacja z listy (np. z DataGrid).
        public Complaint SelectedComplaint
        {
            get => _selectedComplaint;
            set
            {
                _selectedComplaint = value;
                OnPropertyChanged(nameof(SelectedComplaint)); // Powiadamiamy UI o zmianie wartości
                // Aktualizuj stan `CanExecute` komend po zmianie zaznaczenia,
                // aby przyciski akcji włączały się lub wyłączały dynamicznie.
                ((RelayCommand)EditComplaintCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DeleteComplaintCommand).RaiseCanExecuteChanged();
                ((RelayCommand)ChangeStatusCommand).RaiseCanExecuteChanged();
                ((RelayCommand)AssignComplaintCommand).RaiseCanExecuteChanged();
            }
        }

        // Komendy do obsługi interakcji użytkownika z widokiem
        public ICommand LoadComplaintsCommand { get; }
        public ICommand AddComplaintCommand { get; }
        public ICommand EditComplaintCommand { get; }
        public ICommand DeleteComplaintCommand { get; }
        public ICommand ChangeStatusCommand { get; }
        public ICommand AssignComplaintCommand { get; }

        // Konstruktor ViewModelu
        public ComplaintListViewModel(ComplaintRepository complaintRepository, UserRepository userRepository, PermissionService permissionService, UserRole currentUserRole)
        {
            _complaintRepository = complaintRepository;
            _userRepository = userRepository;
            _permissionService = permissionService;
            _currentUserRole = currentUserRole;
            Complaints = new ObservableCollection<Complaint>();

            // Inicjalizacja komend
            LoadComplaintsCommand = new RelayCommand(async obj => await LoadComplaintsAsync());
            AddComplaintCommand = new RelayCommand(obj => AddComplaint());
            // Komenda Edytuj jest aktywna tylko, gdy coś jest wybrane i użytkownik ma uprawnienia
            EditComplaintCommand = new RelayCommand(obj => EditComplaint(), obj => SelectedComplaint != null && _permissionService.HasPermission(_currentUserRole, PermissionAction.EditComplaints));
            // Komenda Usuń jest aktywna tylko, gdy coś jest wybrane i użytkownik ma uprawnienia do usuwania
            DeleteComplaintCommand = new RelayCommand(async obj => await DeleteComplaint(), obj => SelectedComplaint != null && _permissionService.CanDeleteComplaint(_currentUserRole));
            // Komenda Zmień Status jest aktywna, gdy coś jest wybrane i użytkownik ma uprawnienia do zmiany statusu
            ChangeStatusCommand = new RelayCommand(async obj => await ChangeStatus(obj as ComplaintStatus?), obj => SelectedComplaint != null && _permissionService.HasPermission(_currentUserRole, PermissionAction.ChangeComplaintStatus));
            // Komenda Przypisz jest aktywna, gdy coś jest wybrane i użytkownik ma uprawnienia do przypisywania
            AssignComplaintCommand = new RelayCommand(async obj => await AssignComplaint(), obj => SelectedComplaint != null && _permissionService.CanAssignComplaint(_currentUserRole));

            // Ładowanie reklamacji natychmiast po utworzeniu ViewModelu (asynchronicznie, aby nie blokować UI)
            Task.Run(async () => await LoadComplaintsAsync());
        }

        // Metoda do ładowania reklamacji z bazy danych
        public async Task LoadComplaintsAsync()
        {
            Complaints.Clear(); // Wyczyść istniejącą listę
            var allComplaints = await _complaintRepository.GetAllAsync(); // Pobierz wszystkie reklamacje
            foreach (var complaint in allComplaints)
            {
                // Próba załadowania powiązanego użytkownika dla celów wyświetlania, jeśli UserId istnieje
                if (complaint.UserId.HasValue)
                {
                    complaint.User = await _userRepository.GetByIdAsync(complaint.UserId.Value);
                }
                Complaints.Add(complaint); // Dodaj reklamację do ObservableCollection
            }
        }

        // Obsługa dodawania nowej reklamacji (na razie placeholder)
        private void AddComplaint()
        {
            // TODO: Otwórz nowe okno/dialog dla dodawania reklamacji
            MessageBox.Show("Funkcjonalność dodawania reklamacji zostanie zaimplementowana.", "Informacja");
        }

        // Obsługa edycji wybranej reklamacji (na razie placeholder)
        private void EditComplaint()
        {
            if (SelectedComplaint != null)
            {
                // TODO: Otwórz nowe okno/dialog dla edycji wybranej reklamacji
                MessageBox.Show($"Funkcjonalność edycji reklamacji (ID: {SelectedComplaint.Id}) zostanie zaimplementowana.", "Informacja");
            }
        }

        // Obsługa usuwania wybranej reklamacji
        private async Task DeleteComplaint()
        {
            if (SelectedComplaint != null)
            {
                // Potwierdzenie usunięcia
                if (MessageBox.Show($"Czy na pewno chcesz usunąć reklamację '{SelectedComplaint.Title}'?", "Potwierdź usunięcie", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Sprawdzenie uprawnień po stronie UI (dodatkowe zabezpieczenie, choć serwis też sprawdza)
                        if (_permissionService.CanDeleteComplaint(_currentUserRole))
                        {
                            await _complaintRepository.DeleteAsync(SelectedComplaint.Id); // Usuń z bazy
                            Complaints.Remove(SelectedComplaint); // Usuń z ObservableCollection
                            SelectedComplaint = null; // Wyczyść zaznaczenie
                            MessageBox.Show("Reklamacja usunięta pomyślnie.", "Sukces");
                        }
                        else
                        {
                            MessageBox.Show("Brak uprawnień do usuwania reklamacji.", "Brak dostępu", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Błąd podczas usuwania reklamacji: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Proszę wybrać reklamację do usunięcia.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Obsługa zmiany statusu wybranej reklamacji
        private async Task ChangeStatus(ComplaintStatus? newStatus)
        {
            if (SelectedComplaint != null && newStatus.HasValue)
            {
                try
                {
                    // Walidacja uprawnień do zmiany statusu
                    if (!_permissionService.HasPermission(_currentUserRole, PermissionAction.ChangeComplaintStatus))
                    {
                        MessageBox.Show("Brak uprawnień do zmiany statusu reklamacji.", "Brak dostępu", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Zaktualizuj status w obiekcie i w bazie danych
                    SelectedComplaint.Status = newStatus.Value;
                    await _complaintRepository.UpdateAsync(SelectedComplaint);
                    MessageBox.Show($"Status reklamacji zmieniono na: {newStatus.Value}.", "Sukces");
                    await LoadComplaintsAsync(); // Odśwież listę, aby odzwierciedlić zmiany (np. kolory statusów)
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd podczas zmiany statusu: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Proszę wybrać reklamację i nowy status.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Obsługa przypisywania reklamacji (na razie placeholder)
        private async Task AssignComplaint()
        {
            if (SelectedComplaint != null)
            {
                // TODO: Otwórz dialog do wyboru użytkownika i przypisania reklamacji
                MessageBox.Show($"Funkcjonalność przypisywania reklamacji (ID: {SelectedComplaint.Id}) zostanie zaimplementowana.", "Informacja");
            }
            else
            {
                MessageBox.Show("Proszę wybrać reklamację do przypisania.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
