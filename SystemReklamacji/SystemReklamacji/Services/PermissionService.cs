using ReklamacjeSystem.Models; // Dodajemy referencję do modeli
using System.Collections.Generic;
using System.Linq; // Do użycia .Contains()

namespace ReklamacjeSystem.Services
{
    // Usługa zarządzania uprawnieniami w aplikacji
    public class PermissionService
    {
        // Metoda sprawdzająca, czy dana rola ma uprawnienia do wykonania konkretnej akcji
        public bool HasPermission(UserRole userRole, PermissionAction action)
        {
            // Definiujemy uprawnienia dla każdej roli
            // Można to przenieść do konfiguracji lub bazy danych w większych aplikacjach
            switch (userRole)
            {
                case UserRole.Admin:
                    // Admin ma wszystkie uprawnienia
                    return true;
                case UserRole.Manager:
                    // Manager ma uprawnienia do zarządzania użytkownikami (tylko odczyt, edycja),
                    // pełne zarządzanie reklamacjami, zarządzanie rozwiązaniami.
                    return action == PermissionAction.ViewUsers ||
                           action == PermissionAction.EditUsers || // Manager może edytować użytkowników (np. role)
                           action == PermissionAction.CreateComplaint ||
                           action == PermissionAction.ViewComplaints ||
                           action == PermissionAction.EditComplaints ||
                           action == PermissionAction.ChangeComplaintStatus ||
                           action == PermissionAction.AssignComplaints ||
                           action == PermissionAction.CreateSolution ||
                           action == PermissionAction.ViewSolutions ||
                           action == PermissionAction.EditSolutions ||
                           action == PermissionAction.DeleteSolution;
                case UserRole.Employee:
                    // Pracownik może tworzyć, przeglądać i edytować własne reklamacje,
                    // przeglądać wszystkie reklamacje, zmieniać ich status (jeśli przypisane)
                    // oraz przeglądać rozwiązania.
                    return action == PermissionAction.CreateComplaint ||
                           action == PermissionAction.ViewComplaints ||
                           action == PermissionAction.EditComplaints || // Może edytować własne lub przypisane
                           action == PermissionAction.ChangeComplaintStatus ||
                           action == PermissionAction.ViewSolutions;
                default:
                    return false;
            }
        }

        // Metoda pomocnicza do określania, czy użytkownik może usunąć reklamację
        public bool CanDeleteComplaint(UserRole userRole)
        {
            return userRole == UserRole.Admin;
        }

        // Metoda pomocnicza do określania, czy użytkownik może edytować użytkowników
        public bool CanEditUsers(UserRole userRole)
        {
            return userRole == UserRole.Admin || userRole == UserRole.Manager;
        }

        // Dodatkowe metody sprawdzające uprawnienia do konkretnych akcji
        // Można dodać więcej takich metod w miarę potrzeb
        public bool CanViewAllComplaints(UserRole userRole)
        {
            return userRole == UserRole.Admin || userRole == UserRole.Manager || userRole == UserRole.Employee;
        }

        public bool CanAssignComplaint(UserRole userRole)
        {
            return userRole == UserRole.Admin || userRole == UserRole.Manager;
        }
    }

    // Enum definiujący różne akcje, dla których sprawdzamy uprawnienia
    public enum PermissionAction
    {
        // Użytkownicy
        ViewUsers,
        CreateUser,
        EditUsers,
        DeleteUser,

        // Reklamacje
        CreateComplaint,
        ViewComplaints,
        EditComplaints,
        DeleteComplaint,
        ChangeComplaintStatus,
        AssignComplaints,

        // Rozwiązania
        CreateSolution,
        ViewSolutions,
        EditSolutions,
        DeleteSolution
    }
}
