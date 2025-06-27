using ReklamacjeSystem.ViewModels;
using System.Windows;
using ReklamacjeSystem.Models;
using ReklamacjeSystem.Repositories;
using ReklamacjeSystem.Services;
using System.Windows.Controls; // Wymagane dla TextBlock

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
            // Ustawienie tytułu okna i nagłówka dla nowej reklamacji
            this.Title = "Dodaj Nową Reklamację";
            if (this.FindName("HeaderText") is TextBlock headerText)
            {
                headerText.Text = "Dodaj Nową Reklamację";
            }
        }

        // Konstruktor dla edycji/podglądu istniejącej reklamacji
        public ComplaintEditWindow(Complaint complaint, ComplaintRepository complaintRepository, UserRepository userRepository, PermissionService permissionService, UserRole currentUserRole)
        {
            InitializeComponent();
            var viewModel = new ComplaintEditViewModel(complaint, complaintRepository, userRepository, permissionService, currentUserRole);
            this.DataContext = viewModel;
            viewModel.CloseAction = () => this.Close();
            // Ustawienie tytułu okna i nagłówka dla istniejącej reklamacji
            // Tytuł okna będzie się zmieniał dynamicznie w ViewModelu
            // Nagłówek w oknie również będzie dynamiczny (szczegóły / edycja)
        }
    }
}
