using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using ReklamacjeSystem.Models;
using ReklamacjeSystem.Repositories;
using ReklamacjeSystem.Services;
using System.Windows; // Do wyświetlania MessageBox

namespace ReklamacjeSystem.ViewModels
{
    // ViewModel dla listy rozwiązań
    public class SolutionListViewModel : BaseViewModel
    {
        private readonly SolutionRepository _solutionRepository; // Repozytorium do zarządzania rozwiązaniami
        private readonly PermissionService _permissionService; // Usługa do sprawdzania uprawnień
        private readonly UserRole _currentUserRole; // Rola aktualnie zalogowanego użytkownika

        private ObservableCollection<Solution> _solutions;
        // Kolekcja rozwiązań wyświetlanych w widoku.
        public ObservableCollection<Solution> Solutions
        {
            get => _solutions;
            set
            {
                _solutions = value;
                OnPropertyChanged(nameof(Solutions)); // Powiadamiamy UI o zmianie wartości
            }
        }

        private Solution _selectedSolution;
        // Wybrane rozwiązanie z listy (np. z DataGrid).
        public Solution SelectedSolution
        {
            get => _selectedSolution;
            set
            {
                _selectedSolution = value;
                OnPropertyChanged(nameof(SelectedSolution)); // Powiadamiamy UI o zmianie wartości
                // Aktualizuj stan `CanExecute` komend po zmianie zaznaczenia.
                ((RelayCommand)EditSolutionCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DeleteSolutionCommand).RaiseCanExecuteChanged();
            }
        }

        // Komendy do obsługi interakcji użytkownika z widokiem
        public ICommand LoadSolutionsCommand { get; }
        public ICommand AddSolutionCommand { get; }
        public ICommand EditSolutionCommand { get; }
        public ICommand DeleteSolutionCommand { get; }

        // Konstruktor ViewModelu
        public SolutionListViewModel(SolutionRepository solutionRepository, PermissionService permissionService, UserRole currentUserRole)
        {
            _solutionRepository = solutionRepository;
            _permissionService = permissionService;
            _currentUserRole = currentUserRole;
            Solutions = new ObservableCollection<Solution>();

            // Inicjalizacja komend
            LoadSolutionsCommand = new RelayCommand(async obj => await LoadSolutionsAsync());
            AddSolutionCommand = new RelayCommand(obj => AddSolution());
            // Edycja jest dozwolona tylko jeśli coś jest wybrane i użytkownik ma uprawnienia
            EditSolutionCommand = new RelayCommand(obj => EditSolution(), obj => SelectedSolution != null && _permissionService.HasPermission(_currentUserRole, PermissionAction.EditSolutions));
            // Usuwanie jest dozwolone tylko jeśli coś jest wybrane i użytkownik ma uprawnienia
            DeleteSolutionCommand = new RelayCommand(async obj => await DeleteSolution(), obj => SelectedSolution != null && _permissionService.HasPermission(_currentUserRole, PermissionAction.DeleteSolution));

            // Ładowanie rozwiązań natychmiast po utworzeniu ViewModelu
            Task.Run(async () => await LoadSolutionsAsync());
        }

        // Metoda do ładowania rozwiązań z bazy danych
        public async Task LoadSolutionsAsync()
        {
            Solutions.Clear(); // Wyczyść istniejącą listę
            var allSolutions = await _solutionRepository.GetAllAsync(); // Pobierz wszystkie rozwiązania
            foreach (var solution in allSolutions)
            {
                Solutions.Add(solution); // Dodaj rozwiązanie do ObservableCollection
            }
        }

        // Obsługa dodawania nowego rozwiązania (na razie placeholder)
        private void AddSolution()
        {
            // TODO: Otwórz nowe okno/dialog dla dodawania rozwiązania
            MessageBox.Show("Funkcjonalność dodawania rozwiązania zostanie zaimplementowana.", "Informacja");
        }

        // Obsługa edycji wybranego rozwiązania (na razie placeholder)
        private void EditSolution()
        {
            if (SelectedSolution != null)
            {
                // TODO: Otwórz nowe okno/dialog dla edycji wybranego rozwiązania
                MessageBox.Show($"Funkcjonalność edycji rozwiązania (ID: {SelectedSolution.Id}) zostanie zaimplementowana.", "Informacja");
            }
        }

        // Obsługa usuwania wybranego rozwiązania
        private async Task DeleteSolution()
        {
            if (SelectedSolution != null)
            {
                if (MessageBox.Show($"Czy na pewno chcesz usunąć rozwiązanie '{SelectedSolution.Title}'?", "Potwierdź usunięcie", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    try
                    {
                        if (_permissionService.HasPermission(_currentUserRole, PermissionAction.DeleteSolution))
                        {
                            await _solutionRepository.DeleteAsync(SelectedSolution.Id); // Usuń z bazy
                            Solutions.Remove(SelectedSolution); // Usuń z ObservableCollection
                            SelectedSolution = null; // Wyczyść zaznaczenie
                            MessageBox.Show("Rozwiązanie usunięte pomyślnie.", "Sukces");
                        }
                        else
                        {
                            MessageBox.Show("Brak uprawnień do usuwania rozwiązań.", "Brak dostępu", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Błąd podczas usuwania rozwiązania: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Proszę wybrać rozwiązanie do usunięcia.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
