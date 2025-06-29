# System Obsługi Reklamacji

Aplikacja WPF do zarządzania reklamacjami obsługiwana przez pracowników z systemem uprawnień opartym na rolach.

## 📋 Spis treści

- [Opis projektu](#opis-projektu)
- [Funkcjonalności](#funkcjonalności)
- [Stack technologiczny](#stack-technologiczny)
- [Wymagania systemowe](#wymagania-systemowe)
- [Instalacja](#instalacja)
- [Konfiguracja](#konfiguracja)
- [Struktura projektu](#struktura-projektu)
- [Paradygmat obiektowy](#paradygmat-obiektowy)
- [Diagram UML](#diagram-uml)
- [Użytkowanie](#użytkowanie)
- [Licencja](#licencja)

## 🎯 Opis projektu

System Obsługi Reklamacji to aplikacja desktopowa stworzona w technologii WPF (.NET 8.0) przeznaczona do zarządzania procesem obsługi reklamacji w przedsiębiorstwie. Aplikacja umożliwia pracownikom różnych szczebli efektywne zarządzanie zgłoszeniami, ich statusami oraz dostępem do bazy rozwiązań.

## ✨ Funkcjonalności

### Główne funkcje:
- **Zarządzanie reklamacjami**: Tworzenie, wyświetlanie, edycja i usuwanie reklamacji
- **System statusów**: Śledzenie cyklu życia reklamacji (New → InProgress → Resolved → Closed)
- **Przypisywanie użytkownikom**: Delegowanie odpowiedzialności za konkretne reklamacje
- **System uprawnień**: Rozróżnienie ról (Admin, Manager, Employee) z różnymi poziomami dostępu
- **Repozytorium rozwiązań**: Baza wiedzy do automatyzacji i wspomagania pracy pracowników

### Dodatkowe funkcjonalności:
- Bezpieczne uwierzytelnianie z hashowaniem haseł (BCrypt)
- Intuicyjny interfejs użytkownika z Material Design
- Filtrowanie i wyszukiwanie reklamacji
- Powiadomienia o zmianach statusów

## 📸 Zrzuty Ekranu Aplikacji:

![Okno logowanie rejestracja](https://github.com/user-attachments/assets/ff5215bc-4021-4e8a-a047-967f05d848ff)

![Okno listy reklamacji](https://github.com/user-attachments/assets/a5860505-d69d-4de9-9da3-d4e317e944ea)

![Okno Szczegóły reklamacji](https://github.com/user-attachments/assets/71ec391e-e37b-49c9-81ba-a604efd252df)

![Okno Eycja reklamacji](https://github.com/user-attachments/assets/e005fca3-8801-4d36-b63a-f6a7aaac308f)

![Okno Dodaj reklamację](https://github.com/user-attachments/assets/774bb885-b32e-4b83-b63d-9510c2d49612)

![Okno listy rozwiązań](https://github.com/user-attachments/assets/0e58bdc0-37da-4ce7-97d7-30837064cc11)

## 🛠 Stack technologiczny

- **Framework**: .NET 8.0
- **UI**: WPF (Windows Presentation Foundation)
- **Baza danych**: MySQL
- **ORM**: Custom Repository Pattern
- **Haszowanie haseł**: BCrypt.Net
- **Design**: Material Design In XAML
- **Architektura**: MVVM (Model-View-ViewModel)

## 📋 Wymagania systemowe

- Windows 10/11 (x64)
- .NET 8.0 Runtime
- MySQL Server 8.0+
- Minimum 4GB RAM
- 100MB wolnego miejsca na dysku

## 🔧 Instalacja

1. **Pobierz kod źródłowy**:
   ```bash
   https://github.com/KAFLA/Complaint-System.git
   cd Complaint-System
   ```

2. **Przywróć pakiety NuGet**:
   ```bash
   dotnet restore
   ```

3. **Skonfiguruj bazę danych**:
   - Uruchom MySQL Server
   - Wykonaj skrypt `Example_database.sql` w MySQL


## ⚙️ Konfiguracja

### Konfiguracja bazy danych

Aplikacja wymaga pliku `App.config` w katalogu głównym z konfiguracją połączenia do bazy danych. 

**Plik konfiguracyjny jest tworzony przez osobny program `AppConfig.exe`. Program gotowy do kompliacji dołączony jest również do projektu. Po skompilowaniu aplikacji należy przrzucić ją do folderu z plikiem wykonywalnym aplikacji głównej `SystemReklamacji.exe` lub przenieść tam wygenerowany plik `App.config`**


### Domyślne konta użytkowników

Po pierwszym uruchomieniu możesz zarejestrować nowe konta lub skorzystać z domyślnych:
- **Admin**: Pełne uprawnienia do zarządzania systemem
- **Manager**: Zarządzanie użytkownikami i reklamacjami
- **Employee**: Tworzenie i edycja własnych reklamacji

## 📁 Struktura projektu

```
ReklamacjeSystem/
├── Models/                    # Modele danych
│   ├── User.cs               # Model użytkownika
│   ├── Complaint.cs          # Model reklamacji
│   ├── Solution.cs           # Model rozwiązania
│   └── IRepository.cs        # Interfejs repozytorium
├── ViewModels/               # ViewModele (MVVM)
│   ├── BaseViewModel.cs      # Bazowy ViewModel
│   ├── LoginViewModel.cs     # ViewModel logowania
│   ├── MainViewModel.cs      # Główny ViewModel
│   ├── ComplaintListViewModel.cs
│   └── ComplaintEditViewModel.cs
├── Views/                    # Widoki WPF
│   ├── LoginWindow.xaml      # Okno logowania
│   ├── MainWindow.xaml       # Główne okno
│   ├── ComplaintListView.xaml
│   └── ComplaintEditWindow.xaml
├── Repositories/             # Warstwa dostępu do danych
│   ├── BaseRepository.cs     # Bazowe repozytorium
│   ├── UserRepository.cs     # Repozytorium użytkowników
│   ├── ComplaintRepository.cs
│   └── SolutionRepository.cs
├── Services/                 # Usługi biznesowe
│   ├── AuthService.cs        # Uwierzytelnianie
│   ├── PermissionService.cs  # Zarządzanie uprawnieniami
│   └── NotificationService.cs
└── App.xaml                  # Konfiguracja aplikacji
```

## 🏗 Paradygmat obiektowy

Aplikacja została zaprojektowana z zachowaniem kluczowych zasad programowania obiektowego:

### 🔒 Enkapsulacja
- **Modele danych**: Właściwości z kontrolowanym dostępem (get/set)
- **Repozytoria**: Ukrywanie szczegółów implementacji dostępu do bazy danych
- **Usługi**: Enkapsulacja logiki biznesowej w dedykowanych klasach

```csharp
public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    private string _passwordHash; // Enkapsulowane hasło
}
```

### 🧬 Dziedziczenie
- **BaseRepository<T>**: Wspólna funkcjonalność dla wszystkich repozytoriów
- **BaseViewModel**: Implementacja INotifyPropertyChanged dla wszystkich ViewModeli
- **Hierarchia uprawnień**: UserRole enum z różnymi poziomami dostępu

```csharp
public abstract class BaseRepository<T> : IRepository<T> where T : class, new()
{
    protected abstract T MapToEntity(MySqlDataReader reader);
}

public class UserRepository : BaseRepository<User>
{
    protected override User MapToEntity(MySqlDataReader reader) { /* */ }
}
```

### 🔄 Polimorfizm
- **IRepository<T>**: Wspólny interfejs dla różnych typów repozytoriów
- **IAuthService**: Interfejs usługi uwierzytelniania
- **RelayCommand**: Uniwersalna implementacja ICommand

```csharp
public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> GetByIdAsync(int id);
}
```

### 🎭 Abstrakcja
- **Separation of Concerns**: Wyraźny podział na warstwy (Models, Views, ViewModels, Services)
- **MVVM Pattern**: Separacja logiki prezentacji od logiki biznesowej
- **Repository Pattern**: Abstrakcja dostępu do danych

### 🔧 Zasady SOLID
- **Single Responsibility**: Każda klasa ma jedną odpowiedzialność
- **Open/Closed**: Łatwe rozszerzanie funkcjonalności przez dziedziczenie
- **Liskov Substitution**: Klasy pochodne są kompatybilne z klasami bazowymi
- **Interface Segregation**: Małe, wyspecjalizowane interfejsy
- **Dependency Inversion**: Zależności od abstrakcji, nie od konkretnych implementacji

## 📊 Diagram UML

<!-- Tutaj zostanie umieszczony diagram UML systemu -->
```mermaid
classDiagram
    %% Modele danych
    class User {
        +int Id
        +string Username
        +string Email
        +string PasswordHash
        +UserRole Role
        +DateTime CreatedAt
    }

    class Complaint {
        +int Id
        +string Title
        +string Description
        +ComplaintStatus Status
        +ComplaintPriority Priority
        +DateTime CreatedAt
        +int UserId
        +User User
    }

    class Solution {
        +int Id
        +string Title
        +string Description
        +string Category
        +DateTime CreatedAt
    }

    %% Enumy
    class UserRole {
        <<enumeration>>
        Admin
        Manager
        Employee
    }

    class ComplaintStatus {
        <<enumeration>>
        New
        InProgress
        Resolved
        Closed
    }

    class ComplaintPriority {
        <<enumeration>>
        Low
        Medium
        High
        Critical
    }

    %% Interfejs Repository
    class IRepository {
        <<interface>>
        +GetAllAsync()
        +GetByIdAsync(int id)
        +AddAsync(entity)
        +UpdateAsync(entity)
        +DeleteAsync(int id)
    }

    %% Klasa bazowa Repository
    class BaseRepository {
        <<abstract>>
        #string connectionString
        #string tableName
        +GetAllAsync()
        +GetByIdAsync(int id)
        +AddAsync(entity)
        +UpdateAsync(entity)
        +DeleteAsync(int id)
        +MapToEntity(reader)*
    }

    %% Konkretne repozytoria
    class UserRepository {
        +MapToEntity(reader) User
    }

    class ComplaintRepository {
        +MapToEntity(reader) Complaint
    }

    class SolutionRepository {
        +MapToEntity(reader) Solution
        +GetSolutionsByCategoryAsync(category)
        +SearchSolutionsAsync(searchTerm)
    }

    %% Serwisy
    class IAuthService {
        <<interface>>
        +Register(username, email, password, role)
        +Login(username, password)
    }

    class AuthService {
        -UserRepository userRepository
        +Register(username, email, password, role)
        +Login(username, password)
    }

    class ComplaintService {
        -ComplaintRepository complaintRepository
        -UserRepository userRepository
        +GetAllComplaints()
        +AddComplaint(complaint)
        +UpdateComplaint(complaint)
        +ChangeComplaintStatus(id, status, role)
        +AssignComplaintToUser(complaintId, userId)
    }

    class PermissionService {
        +HasPermission(role, action) bool
        +CanDeleteComplaint(role) bool
        +CanEditUsers(role) bool
    }

    %% ViewModels
    class BaseViewModel {
        <<abstract>>
        +PropertyChanged
        #OnPropertyChanged(propertyName)
    }

    class LoginViewModel {
        -IAuthService authService
        +string Username
        +SecureString Password
        +ICommand LoginCommand
        +ICommand RegisterCommand
    }

    class MainViewModel {
        +User CurrentUser
        +BaseViewModel CurrentViewModel
        +ICommand ShowComplaintsCommand
        +ICommand ShowUsersCommand
        +ICommand ShowSolutionsCommand
    }

    class ComplaintListViewModel {
        -ComplaintRepository complaintRepository
        -UserRepository userRepository
        -PermissionService permissionService
        +ObservableCollection Complaints
        +Complaint SelectedComplaint
        +ICommand AddComplaintCommand
        +ICommand ViewEditComplaintCommand
        +ICommand DeleteComplaintCommand
    }

    %% Relacje dziedziczenia
    IRepository <|.. BaseRepository : implements
    BaseRepository <|-- UserRepository : extends
    BaseRepository <|-- ComplaintRepository : extends
    BaseRepository <|-- SolutionRepository : extends
    IAuthService <|.. AuthService : implements

    BaseViewModel <|-- LoginViewModel : extends
    BaseViewModel <|-- MainViewModel : extends
    BaseViewModel <|-- ComplaintListViewModel : extends

    %% Relacje zależności
    AuthService --> UserRepository : uses
    ComplaintService --> ComplaintRepository : uses
    ComplaintService --> UserRepository : uses
    LoginViewModel --> IAuthService : uses
    ComplaintListViewModel --> ComplaintRepository : uses
    ComplaintListViewModel --> UserRepository : uses
    ComplaintListViewModel --> PermissionService : uses
    MainViewModel --> PermissionService : uses

    %% Asocjacje między modelami
    User --> Complaint : assigned_to
    Complaint --> User : belongs_to
```

## 🚀 Użytkowanie

### Pierwsze uruchomienie
1. Uruchom `AppConfig.exe` aby skonfigurować połączenie z bazą danych
2. Uruchom aplikację główną `ReklamacjeSystem.exe`
3. Zarejestruj pierwsze konto administratora
4. Zaloguj się i rozpocznij korzystanie z systemu

### Podstawowe operacje
- **Dodawanie reklamacji**: Przycisk "Add Complaint" w głównym oknie
- **Edycja reklamacji**: Kliknij "View/Edit" przy wybranej reklamacji
- **Zmiana statusu**: Przycisk "Change Status" lub edycja reklamacji
- **Zarządzanie użytkownikami**: Dostępne dla ról Manager i Admin

### Role użytkowników
- **Employee**: Może tworzyć i edytować własne reklamacje, przeglądać rozwiązania
- **Manager**: Dodatkowo może zarządzać użytkownikami i wszystkimi reklamacjami
- **Admin**: Pełne uprawnienia do wszystkich funkcji systemu

## 📄 Licencja

Ten projekt jest licencjonowany na zasadach MIT License. Zobacz plik `LICENSE` dla szczegółów.

---

**Wersja**: 0.19.1 
**Ostatnia aktualizacja**: 2025-06-28  
**Autorzy**: [Mateusz Kazuła], [Dawid Kubica]  
