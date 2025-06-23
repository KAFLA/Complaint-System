using ReklamacjeSystem.ViewModels; 
using System.Collections.ObjectModel;
using ReklamacjeSystem.Models;
using ReklamacjeSystem.Repositories;
using ReklamacjeSystem.Services;

namespace ReklamacjeSystem.ViewModels
{
    public class UserManagementViewModel : BaseViewModel
    {
        private readonly UserRepository _userRepository;
        private readonly PermissionService _permissionService;
        private readonly UserRole _currentUserRole;

        public ObservableCollection<User> Users { get; set; } = new ObservableCollection<User>();

        public UserManagementViewModel(UserRepository userRepository, PermissionService permissionService, UserRole currentUserRole)
        {
            _userRepository = userRepository;
            _permissionService = permissionService;
            _currentUserRole = currentUserRole;
        }
    }
}