using ReklamacjeSystem.ViewModels; // Upewnij się, że masz to
using System.Collections.ObjectModel;
using ReklamacjeSystem.Models;
using ReklamacjeSystem.Repositories;
using ReklamacjeSystem.Services;

namespace ReklamacjeSystem.ViewModels
{
    public class SolutionListViewModel : BaseViewModel
    {
        private readonly SolutionRepository _solutionRepository;
        private readonly PermissionService _permissionService;
        private readonly UserRole _currentUserRole;

        public ObservableCollection<Solution> Solutions { get; set; } = new ObservableCollection<Solution>();

        public SolutionListViewModel(SolutionRepository solutionRepository, PermissionService permissionService, UserRole currentUserRole)
        {
            _solutionRepository = solutionRepository;
            _permissionService = permissionService;
            _currentUserRole = currentUserRole;
        }
    }
}