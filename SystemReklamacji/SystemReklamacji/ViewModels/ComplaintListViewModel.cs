using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using ReklamacjeSystem.Models;
using ReklamacjeSystem.Repositories;
using ReklamacjeSystem.Services;
using System.Windows;
using ReklamacjeSystem.Views;
using System.Linq;
using System;

namespace ReklamacjeSystem.ViewModels
{
    // ViewModel for the list of complaints
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
                // Change command name to reflect the new "View/Edit" functionality
                ((RelayCommand)ViewEditComplaintCommand)?.RaiseCanExecuteChanged();
                ((RelayCommand)DeleteComplaintCommand)?.RaiseCanExecuteChanged();
                ((RelayCommand)ChangeStatusCommand)?.RaiseCanExecuteChanged();
                ((RelayCommand)AssignComplaintCommand)?.RaiseCanExecuteChanged();
            }
        }

        public ICommand LoadComplaintsCommand { get; }
        public ICommand AddComplaintCommand { get; }
        public ICommand ViewEditComplaintCommand { get; } // New command name
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
            // Initialize new command: opens edit window in view mode for existing complaints
            ViewEditComplaintCommand = new RelayCommand(obj => ViewEditComplaint(), obj => SelectedComplaint != null);
            DeleteComplaintCommand = new RelayCommand(async obj => await DeleteComplaint(), obj => SelectedComplaint != null && _permissionService.CanDeleteComplaint(_currentUserRole));
            ChangeStatusCommand = new RelayCommand(async obj => await ChangeStatus(obj as ComplaintStatus?), obj => SelectedComplaint != null && _permissionService.HasPermission(_currentUserRole, PermissionAction.ChangeComplaintStatus));
            AssignComplaintCommand = new RelayCommand(async obj => await AssignComplaint(), obj => SelectedComplaint != null && _permissionService.CanAssignComplaint(_currentUserRole));

            Task.Run(async () => await LoadComplaintsAsync()).ConfigureAwait(false);
        }

        public async Task LoadComplaintsAsync()
        {
            var allComplaints = await _complaintRepository.GetAllAsync();
            var users = (await _userRepository.GetAllAsync()).ToDictionary(u => u.Id);

            var complaintsToDisplay = new ObservableCollection<Complaint>();
            foreach (var complaint in allComplaints)
            {
                if (complaint.UserId.HasValue && users.ContainsKey(complaint.UserId.Value))
                {
                    complaint.User = users[complaint.UserId.Value];
                }
                complaintsToDisplay.Add(complaint);
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                Complaints.Clear();
                foreach (var c in complaintsToDisplay)
                {
                    Complaints.Add(c);
                }
            });
        }

        // Handles adding a new complaint - opens window in edit mode (new complaint)
        private void AddComplaint()
        {
            if (!_permissionService.HasPermission(_currentUserRole, PermissionAction.CreateComplaint))
            {
                MessageBox.Show("No permission to add complaints.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Open the complaint add window. Constructor for new complaint will set IsEditMode = true by default.
            ComplaintEditWindow addWindow = new ComplaintEditWindow(_complaintRepository, _userRepository, _permissionService, _currentUserRole);
            addWindow.ShowDialog();

            Task.Run(async () => await LoadComplaintsAsync()).ConfigureAwait(false);
        }

        // Handles viewing/editing a selected complaint - opens window in view mode
        private void ViewEditComplaint()
        {
            if (SelectedComplaint != null)
            {
                // Open the edit window, passing the selected complaint.
                // Constructor for existing complaint will set IsEditMode = false by default.
                ComplaintEditWindow viewEditWindow = new ComplaintEditWindow(SelectedComplaint, _complaintRepository, _userRepository, _permissionService, _currentUserRole);
                viewEditWindow.ShowDialog();

                Task.Run(async () => await LoadComplaintsAsync()).ConfigureAwait(false);
            }
            else
            {
                MessageBox.Show("Please select a complaint to view/edit.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Remaining methods (DeleteComplaint, ChangeStatus, AssignComplaint) remain unchanged
        // ... (paste remaining methods from your current ComplaintListViewModel) ...

        private async Task DeleteComplaint()
        {
            if (SelectedComplaint != null)
            {
                if (MessageBox.Show($"Are you sure you want to delete complaint '{SelectedComplaint.Title}'?", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
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
                            MessageBox.Show("Complaint deleted successfully.", "Success");
                        }
                        else
                        {
                            MessageBox.Show("No permission to delete complaints.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting complaint: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a complaint to delete.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                        MessageBox.Show("No permission to change complaint status.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    SelectedComplaint.Status = newStatus.Value;
                    await _complaintRepository.UpdateAsync(SelectedComplaint);
                    MessageBox.Show($"Complaint status changed to: {newStatus.Value}.", "Success");
                    await LoadComplaintsAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error changing status: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a complaint and new status.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async Task AssignComplaint()
        {
            if (SelectedComplaint != null)
            {
                MessageBox.Show($"Assign complaint functionality (ID: {SelectedComplaint.Id}) will be implemented.", "Information");
            }
            else
            {
                MessageBox.Show("Please select a complaint to assign.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
