# Analiza Paradygmatu Programowania Obiektowego w Systemie Reklamacji

## Spis Treści
1. [Wprowadzenie](#wprowadzenie)
2. [Enkapsulacja](#enkapsulacja)
3. [Dziedziczenie](#dziedziczenie)
4. [Polimorfizm](#polimorfizm)
5. [Abstrakcja](#abstrakcja)
6. [Wzorce Projektowe](#wzorce-projektowe)
7. [Zasady SOLID](#zasady-solid)
8. [Struktura Architektury](#struktura-architektury)
9. [Podsumowanie](#podsumowanie)

## Wprowadzenie

System obsługi reklamacji został zbudowany w oparciu o paradygmat programowania obiektowego (OOP), wykorzystując wszystkie jego fundamentalne zasady. Aplikacja demonstruje profesjonalne podejście do architektury oprogramowania z wykorzystaniem wzorców projektowych i zasad SOLID.

## Enkapsulacja

Enkapsulacja to ukrywanie szczegółów implementacji i kontrolowanie dostępu do danych obiektu.

### Przykład 1: Klasa User
```csharp
public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; } // Enkapsulacja bezpieczeństwa
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

**Zastosowanie:**
- Hasło jest przechowywane jako `PasswordHash`, nie jako zwykły tekst
- Kontrolowany dostęp przez właściwości (properties)
- Ukrycie szczegółów implementacji przechowywania danych

### Przykład 2: BaseViewModel
```csharp
public class BaseViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName) // protected - kontrola dostępu
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
```

**Zastosowanie:**
- `protected` - dostęp tylko dla klas dziedziczących
- Enkapsulacja mechanizmu powiadamiania o zmianach
- Ukrycie szczegółów implementacji bindingu danych

### Przykład 3: ComplaintEditViewModel
```csharp
public class ComplaintEditViewModel : BaseViewModel
{
    private string _title; // private field
    public string Title    // public property
    {
        get => _title;
        set
        {
            _title = value;
            OnPropertyChanged(nameof(Title));
            ((RelayCommand)SaveCommand)?.RaiseCanExecuteChanged();
        }
    }

    private bool _isEditMode;
    public bool IsEditMode
    {
        get => _isEditMode;
        set
        {
            _isEditMode = value;
            OnPropertyChanged(nameof(IsEditMode));
            OnPropertyChanged(nameof(IsViewMode));
            // Kontrolowana logika przy zmianie stanu
        }
    }
}
```

## Dziedziczenie

Dziedziczenie pozwala na tworzenie nowych klas w oparciu o istniejące, dziedzicząc ich funkcjonalność.

### Hierarchia ViewModeli
```csharp
// Klasa bazowa
public class BaseViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName) { /* implementacja */ }
}

// Klasy dziedziczące
public class LoginViewModel : BaseViewModel { }
public class MainViewModel : BaseViewModel { }
public class ComplaintListViewModel : BaseViewModel { }
public class ComplaintEditViewModel : BaseViewModel { }
public class SolutionListViewModel : BaseViewModel { }
```

**Korzyści:**
- Wspólna funkcjonalność w klasie bazowej
- Konsystentna implementacja INotifyPropertyChanged
- Łatwość dodawania nowych ViewModeli

### Hierarchia Repozytoriów
```csharp
// Abstrakcyjna klasa bazowa
public abstract class BaseRepository<T> : IRepository<T> where T : class, new()
{
    protected readonly string _connectionString;
    protected readonly string _tableName;

    // Wspólne metody CRUD
    public async Task<IEnumerable<T>> GetAllAsync() { /* implementacja */ }
    public async Task<T> GetByIdAsync(int id) { /* implementacja */ }
    public async Task AddAsync(T entity) { /* implementacja */ }
    public async Task UpdateAsync(T entity) { /* implementacja */ }
    public async Task DeleteAsync(int id) { /* implementacja */ }

    // Abstrakcyjna metoda - musi być zaimplementowana w klasach dziedziczących
    protected abstract T MapToEntity(MySqlDataReader reader);
}

// Konkretne implementacje
public class UserRepository : BaseRepository<User>
{
    public UserRepository(string connectionString) : base(connectionString, "Users") { }
    
    protected override User MapToEntity(MySqlDataReader reader)
    {
        return new User
        {
            Id = reader.GetInt32("Id"),
            Username = reader.GetString("Username"),
            Email = reader.GetString("Email"),
            PasswordHash = reader.GetString("PasswordHash"),
            Role = (UserRole)Enum.Parse(typeof(UserRole), reader.GetString("Role")),
            CreatedAt = reader.GetDateTime("CreatedAt")
        };
    }
}

public class ComplaintRepository : BaseRepository<Complaint> { /* podobna implementacja */ }
public class SolutionRepository : BaseRepository<Solution> { /* podobna implementacja */ }
```

## Polimorfizm

Polimorfizm pozwala na różne implementacje tej samej funkcjonalności.

### Przykład 1: Abstrakcyjna metoda MapToEntity
```csharp
// W UserRepository
protected override User MapToEntity(MySqlDataReader reader)
{
    return new User
    {
        Id = reader.GetInt32("Id"),
        Username = reader.GetString("Username"),
        // ... mapowanie specyficzne dla User
    };
}

// W ComplaintRepository
protected override Complaint MapToEntity(MySqlDataReader reader)
{
    return new Complaint
    {
        Id = reader.GetInt32("Id"),
        Title = reader.GetString("Title"),
        Status = (ComplaintStatus)Enum.Parse(typeof(ComplaintStatus), reader.GetString("Status"), true),
        // ... mapowanie specyficzne dla Complaint
    };
}
```

### Przykład 2: Polimorfizm komend
```csharp
// Wspólny interfejs
public interface ICommand
{
    bool CanExecute(object parameter);
    void Execute(object parameter);
    event EventHandler CanExecuteChanged;
}

// Implementacja
public class RelayCommand : ICommand
{
    private readonly Action<object> _execute;
    private readonly Predicate<object> _canExecute;

    public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;
    public void Execute(object parameter) => _execute(parameter);
}
```

## Abstrakcja

Abstrakcja ukrywa złożoność implementacji za prostymi interfejsami.

### Przykład 1: IRepository Interface
```csharp
public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> GetByIdAsync(int id);
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
}
```

**Korzyści:**
- Definiuje kontrakt dla wszystkich repozytoriów
- Umożliwia łatwe testowanie (mock objects)
- Ukrywa szczegóły implementacji bazy danych

### Przykład 2: IAuthService Interface
```csharp
public interface IAuthService
{
    Task<User> Register(string username, string email, string password, UserRole role);
    Task<User> Login(string username, string password);
}

public class AuthService : IAuthService
{
    public async Task<User> Login(string username, string password)
    {
        // Złożona logika hashowania, weryfikacji, etc.
        // Ukryta za prostym interfejsem
    }
}
```

## Wzorce Projektowe

### 1. Repository Pattern
```csharp
// Oddzielenie logiki dostępu do danych od logiki biznesowej
public class ComplaintRepository : BaseRepository<Complaint>
{
    // Metody specyficzne dla Complaint
    // Ukrywa szczegóły SQL i mapowania danych
}
```

### 2. MVVM (Model-View-ViewModel) Pattern
```csharp
// Model - dane
public class Complaint
{
    public int Id { get; set; }
    public string Title { get; set; }
    // ... inne właściwości
}

// ViewModel - logika prezentacji
public class ComplaintListViewModel : BaseViewModel
{
    private ObservableCollection<Complaint> _complaints;
    public ObservableCollection<Complaint> Complaints { get; set; }
    public ICommand LoadComplaintsCommand { get; }
    public ICommand AddComplaintCommand { get; }
}

// View - interfejs użytkownika (XAML)
// <DataGrid ItemsSource="{Binding Complaints}"/>
// <Button Command="{Binding AddComplaintCommand}"/>
```

### 3. Command Pattern
```csharp
public class RelayCommand : ICommand
{
    private readonly Action<object> _execute;
    private readonly Predicate<object> _canExecute;
    
    public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public void Execute(object parameter) => _execute(parameter);
    public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;
}
```

### 4. Service Layer Pattern
```csharp
public class PermissionService
{
    public bool HasPermission(UserRole userRole, PermissionAction action)
    {
        // Centralizacja logiki uprawnień
        switch (userRole)
        {
            case UserRole.Admin: return true;
            case UserRole.Manager: /* logika dla managera */
            case UserRole.Employee: /* logika dla pracownika */
            default: return false;
        }
    }
}
```

## Zasady SOLID

### Single Responsibility Principle (SRP)
```csharp
// Każda klasa ma jedną odpowiedzialność
public class UserRepository : BaseRepository<User>  // Tylko dostęp do danych użytkowników
public class AuthService : IAuthService             // Tylko autoryzacja
public class PermissionService                      // Tylko uprawnienia
public class LoginViewModel : BaseViewModel         // Tylko logika logowania
```

### Open/Closed Principle (OCP)
```csharp
// BaseRepository jest otwarte na rozszerzenia, zamknięte na modyfikacje
public abstract class BaseRepository<T> : IRepository<T>
{
    // Stabilna implementacja CRUD
    // Można dodawać nowe typy repozytoriów bez modyfikacji kodu bazowego
}

public class SolutionRepository : BaseRepository<Solution>
{
    // Rozszerzenie o specjalistyczne metody
    public async Task<IEnumerable<Solution>> GetSolutionsByCategoryAsync(string category)
    {
        // Dodatkowa funkcjonalność bez modyfikacji BaseRepository
    }
}
```

### Liskov Substitution Principle (LSP)
```csharp
// Wszystkie implementacje IRepository mogą być używane zamiennie
IRepository<User> userRepo = new UserRepository(connectionString);
IRepository<Complaint> complaintRepo = new ComplaintRepository(connectionString);

// Można używać dowolnego repozytorium w ten sam sposób
var users = await userRepo.GetAllAsync();
var complaints = await complaintRepo.GetAllAsync();
```

### Interface Segregation Principle (ISP)
```csharp
// Małe, specjalistyczne interfejsy
public interface IAuthService
{
    Task<User> Register(string username, string email, string password, UserRole role);
    Task<User> Login(string username, string password);
}

// Zamiast jednego dużego interfejsu dla wszystkich operacji użytkownika
```

### Dependency Inversion Principle (DIP)
```csharp
public class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _authService; // Zależność od abstrakcji, nie konkretnej klasy

    public LoginViewModel(IAuthService authService)
    {
        _authService = authService; // Wstrzykiwanie zależności
    }
}
```

## Struktura Architektury

### Warstwa Modeli (Models)
```
Models/
├── User.cs          - Model użytkownika
├── Complaint.cs     - Model reklamacji
├── Solution.cs      - Model rozwiązania
└── IRepository.cs   - Interfejs repozytorium
```

### Warstwa Repozytoriów (Repositories)
```
Repositories/
├── BaseRepository.cs      - Bazowa klasa dla wszystkich repozytoriów
├── UserRepository.cs      - Repozytorium użytkowników
├── ComplaintRepository.cs - Repozytorium reklamacji
└── SolutionRepository.cs  - Repozytorium rozwiązań
```

### Warstwa Serwisów (Services)
```
Services/
├── IAuthService.cs         - Interfejs autoryzacji
├── AuthService.cs          - Implementacja autoryzacji
├── PermissionService.cs    - Serwis uprawnień
├── ComplaintService.cs     - Serwis logiki biznesowej reklamacji
└── NotificationService.cs  - Serwis powiadomień
```

### Warstwa Prezentacji (ViewModels + Views)
```
ViewModels/
├── BaseViewModel.cs           - Bazowy ViewModel
├── LoginViewModel.cs          - ViewModel logowania
├── MainViewModel.cs           - Główny ViewModel
├── ComplaintListViewModel.cs  - ViewModel listy reklamacji
├── ComplaintEditViewModel.cs  - ViewModel edycji reklamacji
└── SolutionListViewModel.cs   - ViewModel listy rozwiązań

Views/
├── LoginWindow.xaml           - Okno logowania
├── MainWindow.xaml            - Główne okno
├── ComplaintListView.xaml     - Widok listy reklamacji
├── ComplaintEditWindow.xaml   - Okno edycji reklamacji
└── SolutionListView.xaml      - Widok listy rozwiązań
```

## Kompozycja vs Dziedziczenie

### Przykład Kompozycji
```csharp
public class ComplaintEditViewModel : BaseViewModel
{
    // Kompozycja - zawiera różne serwisy
    private readonly ComplaintRepository _complaintRepository;
    private readonly UserRepository _userRepository;
    private readonly PermissionService _permissionService;
    private readonly UserRole _currentUserRole;

    public ComplaintEditViewModel(
        ComplaintRepository complaintRepository,
        UserRepository userRepository,
        PermissionService permissionService,
        UserRole currentUserRole)
    {
        // Wstrzykiwanie zależności przez konstruktor
        _complaintRepository = complaintRepository;
        _userRepository = userRepository;
        _permissionService = permissionService;
        _currentUserRole = currentUserRole;
    }
}
```

## Konwertery jako Przykład OOP

```csharp
// Enkapsulacja logiki konwersji
public class ComplaintStatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ComplaintStatus status)
        {
            return status switch
            {
                ComplaintStatus.New => new SolidColorBrush(Colors.OrangeRed),
                ComplaintStatus.InProgress => new SolidColorBrush(Colors.DarkOrange),
                ComplaintStatus.Resolved => new SolidColorBrush(Colors.DarkGreen),
                ComplaintStatus.Closed => new SolidColorBrush(Colors.Gray),
                _ => new SolidColorBrush(Colors.Black)
            };
        }
        return new SolidColorBrush(Colors.Black);
    }
}

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            if (parameter?.ToString() == "Inverse")
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }
}
```

## Korzyści Zastosowanego OOP

### 1. **Modularność**
- Każda klasa ma jasno określoną odpowiedzialność
- Łatwe testowanie jednostkowe
- Możliwość niezależnego rozwoju komponentów

### 2. **Reużywalność**
- BaseRepository może być użyte dla dowolnego typu encji
- BaseViewModel zapewnia wspólną funkcjonalność dla wszystkich ViewModeli
- Konwertery mogą być używane w różnych miejscach UI

### 3. **Skalowalność**
- Łatwe dodawanie nowych typów encji (np. Customer, Product)
- Proste rozszerzanie funkcjonalności przez dziedziczenie
- Możliwość dodawania nowych ViewModeli bez wpływu na istniejące

### 4. **Maintainability**
- Zmiany w jednej klasie nie wpływają na inne
- Jasna struktura ułatwia zrozumienie kodu
- Centralizacja logiki w odpowiednich serwisach

### 5. **Testowanie**
- Interfejsy umożliwiają mockowanie zależności
- Każda klasa może być testowana niezależnie
- Dependency Injection ułatwia unit testing

## Przykłady Zastosowania Wzorców

### Factory Pattern (implicit)
```csharp
// W MainViewModel - tworzenie odpowiednich ViewModeli
private async Task ShowComplaints()
{
    var complaintRepository = new ComplaintRepository(_connectionString);
    var userRepository = new UserRepository(_connectionString);
    CurrentViewModel = new ComplaintListViewModel(complaintRepository, userRepository, _permissionService, CurrentUser.Role);
}

private void ShowSolutions()
{
    var solutionRepository = new SolutionRepository(_connectionString);
    CurrentViewModel = new SolutionListViewModel(solutionRepository, _permissionService, CurrentUser.Role);
}
```

### Observer Pattern (przez INotifyPropertyChanged)
```csharp
public class BaseViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
```

## Podsumowanie

System reklamacji stanowi doskonały przykład profesjonalnego zastosowania paradygmatu programowania obiektowego w .NET/WPF. Kod demonstruje:

### Główne Zalety:
1. **Czysty kod** - każda klasa ma jasną odpowiedzialność
2. **Modularność** - łatwe dodawanie nowych funkcji
3. **Reużywalność** - komponenty mogą być używane wielokrotnie
4. **Testowalność** - łatwe unit testing dzięki interfejsom
5. **Skalowalność** - aplikacja może rosnąć bez refaktoryzacji
6. **Maintainability** - łatwe utrzymanie i modyfikacja

### Zastosowane Wzorce:
- **Repository Pattern** - separacja dostępu do danych
- **MVVM Pattern** - separacja logiki prezentacji
- **Command Pattern** - enkapsulacja akcji użytkownika
- **Service Layer** - centralizacja logiki biznesowej
- **Dependency Injection** - luźne powiązanie komponentów

### Zasady SOLID:
- **SRP** - jedna odpowiedzialność na klasę
- **OCP** - otwarte na rozszerzenia, zamknięte na modyfikacje
- **LSP** - zamienność implementacji
- **ISP** - małe, specjalistyczne interfejsy
- **DIP** - zależność od abstrakcji, nie konkretnych klas

Taki sposób implementacji sprawia, że kod jest profesjonalny, łatwy do zrozumienia, testowania i rozwijania - co są kluczowymi cechami dobrego oprogramowania biznesowego.