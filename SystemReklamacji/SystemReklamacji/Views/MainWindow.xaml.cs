using ReklamacjeSystem.Models;
using ReklamacjeSystem.ViewModels;
using System.Windows;
using System.IO;
using System.Xml;

namespace ReklamacjeSystem.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow(User loggedInUser)
        {
            InitializeComponent();

            string connectionString = string.Empty;

            try
            {
                string configPath = Path.Combine(Directory.GetCurrentDirectory(), "App.config");

                if (!File.Exists(configPath))
                {
                    MessageBox.Show("Nie znaleziono pliku App.config w katalogu aplikacji.", "Błąd Konfiguracji", MessageBoxButton.OK, MessageBoxImage.Error);
                    Application.Current.Shutdown();
                    return;
                }

                XmlDocument doc = new XmlDocument();
                doc.Load(configPath);

                XmlNode node = doc.SelectSingleNode("/configuration/connectionStrings/add[@name='ReklamacjeDbConnection']");
                if (node != null && node.Attributes != null)
                {
                    connectionString = node.Attributes["connectionString"]?.Value;

                    if (string.IsNullOrWhiteSpace(connectionString))
                    {
                        MessageBox.Show("Błąd: connectionString w App.config jest pusty.", "Błąd Konfiguracji", MessageBoxButton.OK, MessageBoxImage.Error);
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

            this.DataContext = new MainViewModel(loggedInUser, connectionString);

            // Przekazanie delegatu do zamknięcia okna do ViewModelu
            var vm = (MainViewModel)this.DataContext;
            vm.CloseAction = new System.Action(() => this.Close());
        }
    }
}
