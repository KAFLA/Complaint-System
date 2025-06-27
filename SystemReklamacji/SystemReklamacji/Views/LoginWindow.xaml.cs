using ReklamacjeSystem.ViewModels;
using ReklamacjeSystem.Services;
using System.Windows;
using ReklamacjeSystem.Repositories;
using System.IO;
using System.Xml;

namespace ReklamacjeSystem.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();

            string connectionString = string.Empty;

            try
            {
                // Ścieżka do App.config obok pliku wykonywalnego
                string configPath = Path.Combine(Directory.GetCurrentDirectory(), "App.config");

                if (!File.Exists(configPath))
                {
                    MessageBox.Show("Nie znaleziono pliku App.config. Upewnij się, że plik istnieje w katalogu aplikacji.", "Błąd Konfiguracji", MessageBoxButton.OK, MessageBoxImage.Error);
                    Application.Current.Shutdown();
                    return;
                }

                XmlDocument doc = new XmlDocument();
                doc.Load(configPath);

                // Odczyt connection stringa z App.config
                XmlNode node = doc.SelectSingleNode("/configuration/connectionStrings/add[@name='ReklamacjeDbConnection']");
                if (node != null && node.Attributes != null)
                {
                    connectionString = node.Attributes["connectionString"]?.Value;

                    if (string.IsNullOrWhiteSpace(connectionString))
                    {
                        MessageBox.Show("Błąd: Wartość connectionString w App.config jest pusta.", "Błąd Konfiguracji", MessageBoxButton.OK, MessageBoxImage.Error);
                        Application.Current.Shutdown();
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("Nie znaleziono wpisu 'ReklamacjeDbConnection' w pliku App.config.", "Błąd Konfiguracji", MessageBoxButton.OK, MessageBoxImage.Error);
                    Application.Current.Shutdown();
                    return;
                }
            }
            catch (XmlException ex)
            {
                MessageBox.Show($"Błąd XML w pliku App.config: {ex.Message}", "Błąd Konfiguracji", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
                return;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Wystąpił nieoczekiwany błąd podczas ładowania App.config: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
                return;
            }

            // Użycie connection stringa
            UserRepository userRepository = new UserRepository(connectionString);
            AuthService authService = new AuthService(userRepository);
            LoginViewModel viewModel = new LoginViewModel(authService);
            this.DataContext = viewModel;
            viewModel.CloseAction = () => this.Close();
        }
    }
}
