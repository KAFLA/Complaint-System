using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using ReklamacjeSystem.Models;
using ReklamacjeSystem.Repositories;
using ReklamacjeSystem.Services;
using System.Windows;
using ReklamacjeSystem.Views;
using System.Linq; // Dodaj to using do używania Linq

namespace ReklamacjeSystem.ViewModels
{
    // ViewModel dla listy reklamacji
    public class ComplaintListViewModel : BaseViewModel
    {
        private readonly ComplaintRepository _complaintRepository;
        private readonly UserRepository _userRepository;
        private readonly PermissionService _permissionService;
        private readonly UserRole _currentUserRole;

        private ObservableCollection<Complaint> _complaints;
        public ObservableCollection<Complaint> Complaints
        {
            get => _complaints;
            set
            {
                _complaints = value;
                OnPropertyChanged(nameof(Complaints));
            }
        }

        private Complaint _selectedComplaint;
        public Complaint SelectedComplaint
        {
            get => _selectedComplaint;
            set
            {
                _selectedComplaint = value;
                OnPropertyChanged(nameof(SelectedComplaint));
                (EditComplaintCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (DeleteComplaintCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (ChangeStatusCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (AssignComplaintCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public ICommand LoadComplaintsCommand { get; }
        public ICommand AddComplaintCommand { get; }
        public ICommand EditComplaintCommand { get; }
        public ICommand DeleteComplaintCommand { get; }
        public ICommand ChangeStatusCommand { get; }
        public ICommand AssignComplaintCommand { get; }

        public ComplaintListViewModel(ComplaintRepository complaintRepository, UserRepository userRepository, PermissionService permissionService, UserRole currentUserRole)
        {
            _complaintRepository = complaintRepository;
            _userRepository = userRepository;
            _permissionService = permissionService;
            _currentUserRole = currentUserRole;
            Complaints = new ObservableCollection<Complaint>();

            LoadComplaintsCommand = new RelayCommand(async obj => await LoadComplaintsAsync());
            AddComplaintCommand = new RelayCommand(obj => AddComplaint());
            EditComplaintCommand = new RelayCommand(obj => EditComplaint(), obj => SelectedComplaint != null && _permissionService.HasPermission(_currentUserRole, PermissionAction.EditComplaints));
            DeleteComplaintCommand = new RelayCommand(async obj => await DeleteComplaint(), obj => SelectedComplaint != null && _permissionService.CanDeleteComplaint(_currentUserRole));
            ChangeStatusCommand = new RelayCommand(async obj => await ChangeStatus(obj as ComplaintStatus?), obj => SelectedComplaint != null && _permissionService.HasPermission(_currentUserRole, PermissionAction.ChangeComplaintStatus));
            AssignComplaintCommand = new RelayCommand(async obj => await AssignComplaint(), obj => SelectedComplaint != null && _permissionService.CanAssignComplaint(_currentUserRole));

            // Ładowanie reklamacji natychmiast po utworzeniu ViewModelu
            // Używamy ConfigureAwait(false), aby zapobiec przechwyceniu kontekstu synchronizacji UI na początku zadania.
            Task.Run(async () => await LoadComplaintsAsync()).ConfigureAwait(false);
        }

        // Metoda do ładowania reklamacji z bazy danych
        public async Task LoadComplaintsAsync()
        {
            var allComplaints = await _complaintRepository.GetAllAsync(); // Pobierz wszystkie reklamacje (na wątku w tle)
            var users = (await _userRepository.GetAllAsync()).ToDictionary(u => u.Id); // Pobierz wszystkich użytkowników do słownika dla szybszego wyszukiwania

            var complaintsToDisplay = new ObservableCollection<Complaint>();

            foreach (var complaint in allComplaints)
            {
                if (complaint.UserId.HasValue && users.ContainsKey(complaint.UserId.Value))
                {
                    complaint.User = users[complaint.UserId.Value]; // Przypisz obiekt użytkownika
                }
                complaintsToDisplay.Add(complaint); // Dodaj reklamację do tymczasowej kolekcji
            }

            // Teraz, gdy wszystkie dane są gotowe, zaktualizuj kolekcję na wątku UI
            Application.Current.Dispatcher.Invoke(() =>
            {
                Complaints.Clear();
                foreach (var c in complaintsToDisplay)
                {
                    Complaints.Add(c);
                }
            });
        }

        private void AddComplaint()
        {
            if (!_permissionService.HasPermission(_currentUserRole, PermissionAction.CreateComplaint))
            {
                MessageBox.Show("Brak uprawnień do dodawania reklamacji.", "Brak dostępu", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ComplaintEditWindow addWindow = new ComplaintEditWindow(_complaintRepository, _userRepository, _permissionService, _currentUserRole);
            addWindow.ShowDialog();
            Task.Run(async () => await LoadComplaintsAsync()).ConfigureAwait(false); // Odśwież listę po zamknięciu okna
        }

        private void EditComplaint()
        {
            if (SelectedComplaint != null)
            {
                if (!_permissionService.HasPermission(_currentUserRole, PermissionAction.EditComplaints))
                {
                    MessageBox.Show("Brak uprawnień do edycji reklamacji.", "Brak dostępu", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                ComplaintEditWindow editWindow = new ComplaintEditWindow(SelectedComplaint, _complaintRepository, _userRepository, _permissionService, _currentUserRole);
                editWindow.ShowDialog();
                Task.Run(async () => await LoadComplaintsAsync()).ConfigureAwait(false); // Odśwież listę po zamknięciu okna
            }
            else
            {
                MessageBox.Show("Proszę wybrać reklamację do edycji.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async Task DeleteComplaint()
        {
            if (SelectedComplaint != null)
            {
                if (MessageBox.Show($"Czy na pewno chcesz usunąć reklamację '{SelectedComplaint.Title}'?", "Potwierdź usunięcie", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    try
                    {
                        if (_permissionService.CanDeleteComplaint(_currentUserRole))
                        {
                            await _complaintRepository.DeleteAsync(SelectedComplaint.Id);
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                Complaints.Remove(SelectedComplaint);
                            });
                            SelectedComplaint = null;
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

        private async Task ChangeStatus(ComplaintStatus? newStatus)
        {
            if (SelectedComplaint != null && newStatus.HasValue)
            {
                try
                {
                    if (!_permissionService.HasPermission(_currentUserRole, PermissionAction.ChangeComplaintStatus))
                    {
                        MessageBox.Show("Brak uprawnień do zmiany statusu reklamacji.", "Brak dostępu", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    SelectedComplaint.Status = newStatus.Value;
                    await _complaintRepository.UpdateAsync(SelectedComplaint);
                    MessageBox.Show($"Status reklamacji zmieniono na: {newStatus.Value}.", "Sukces");
                    await LoadComplaintsAsync();
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

        private async Task AssignComplaint()
        {
            if (SelectedComplaint != null)
            {
                MessageBox.Show($"Funkcjonalność przypisywania reklamacji (ID: {SelectedComplaint.Id}) zostanie zaimplementowana.", "Informacja");
            }
            else
            {
                MessageBox.Show("Proszę wybrać reklamację do przypisania.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
