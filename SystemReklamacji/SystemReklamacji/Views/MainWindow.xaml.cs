using ReklamacjeSystem.Models;
using ReklamacjeSystem.ViewModels;
using System.Windows;

namespace ReklamacjeSystem.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Konstruktor przyjmujący zalogowanego użytkownika
        public MainWindow(User loggedInUser)
        {
            InitializeComponent();

            // Konfiguracja stringa połączenia do bazy danych MySQL
            // Pamiętaj, aby dostosować te dane do swojej konfiguracji MySQL
            string connectionString = "server=127.0.0.1;port=3306;database=complaintsystem;user=reklamacje_user;password=zaq1@WSX;SslMode=None;";
            // ZMIEŃ 'twoje_haslo' NA PRAWDZIWE HASŁO DO BAZY DANYCH!

            // Ustawienie DataContext dla tego okna na nową instancję MainViewModel
            // Przekazujemy zalogowanego użytkownika i connectionString
            this.DataContext = new MainViewModel(loggedInUser, connectionString);
        }
    }
}