using System.Windows;
using ReklamacjeSystem.Views;

namespace ReklamacjeSystem
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Uruchamiamy LoginWindow jako pierwsze okno aplikacji
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
        }
    }
}
