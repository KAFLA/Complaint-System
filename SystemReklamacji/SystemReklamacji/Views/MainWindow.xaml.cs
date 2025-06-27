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
            // WAŻNE: ZMIEŃ 'twoje_haslo' NA PRAWDZIWE HASŁO DO UŻYTKOWNIKA BAZY DANYCH!
            string connectionString = "server=localhost;port=3306;database=reklamacje_db;user=kaflisz;password=zaq1@WSX;SslMode=None;";
            // Jeśli używasz użytkownika 'reklamacje_user' stworzonego skryptem SQL, hasło to 'zaq1@WSX':
            // string connectionString = "server=localhost;port=3306;database=reklamacje_db;user=reklamacje_user;password=zaq1@WSX;SslMode=None;"; 

            // Ustawienie DataContext dla tego okna na nową instancję MainViewModel
            // Przekazujemy zalogowanego użytkownika i connectionString
            this.DataContext = new MainViewModel(loggedInUser, connectionString);
        }
    }
}
