using ReklamacjeSystem.ViewModels;
using System.Windows;
using ReklamacjeSystem.Models;
using ReklamacjeSystem.Repositories;
using ReklamacjeSystem.Services;
using System.Windows.Controls;

namespace ReklamacjeSystem.Views
{
    /// <summary>
    /// Interaction logic for ComplaintEditWindow.xaml
    /// </summary>
    public partial class ComplaintEditWindow : Window
    {
        // Konstruktor dla nowej reklamacji
        public ComplaintEditWindow(ComplaintRepository complaintRepository, UserRepository userRepository, PermissionService permissionService, UserRole currentUserRole)
        {
            InitializeComponent();
            var viewModel = new ComplaintEditViewModel(complaintRepository, userRepository, permissionService, currentUserRole);
            this.DataContext = viewModel;
            viewModel.CloseAction = () => this.Close();
            this.Title = "Dodaj Nową Reklamację";
            if (this.FindName("HeaderText") is TextBlock headerText)
            {
                headerText.Text = "Dodaj Nową Reklamację";
            }
        }

        // Konstruktor dla edycji istniejącej reklamacji
        public ComplaintEditWindow(Complaint complaint, ComplaintRepository complaintRepository, UserRepository userRepository, PermissionService permissionService, UserRole currentUserRole)
        {
            InitializeComponent();
            var viewModel = new ComplaintEditViewModel(complaint, complaintRepository, userRepository, permissionService, currentUserRole);
            this.DataContext = viewModel;
            viewModel.CloseAction = () => this.Close();
            this.Title = $"Edytuj Reklamację: {complaint.Title}";
            if (this.FindName("HeaderText") is TextBlock headerText)
            {
                headerText.Text = $"Edytuj Reklamację (ID: {complaint.Id})";
            }
        }
    }
}
