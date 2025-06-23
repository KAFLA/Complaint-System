using System.Windows;
using ReklamacjeSystem.Views; // Upewnij się, że masz to użycie

namespace ReklamacjeSystem
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Uruchamiamy LoginWindow jako pierwsze okno
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
        }
    }
}