using System.IO;
using System.Text;
using System.Windows;

namespace ConfigCreator
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void SaveConfig_Click(object sender, RoutedEventArgs e)
        {
            string ip = IpTextBox.Text;
            string port = PortTextBox.Text;
            string database = DatabaseTextBox.Text;
            string user = UserTextBox.Text;
            string password = PasswordBox.Password;
            string sslMode = SslCheckBox.IsChecked == true ? "Required" : "None";

            string configContent = $@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<configuration>
    <startup>
        <supportedRuntime version=""v4.0"" sku="".NETFramework,Version=v4.7.2"" />
    </startup>

    <!-- Sekcja connectionStrings do przechowywania danych połączenia z bazą danych -->
    <connectionStrings>
        <add name=""ReklamacjeDbConnection""
             connectionString=""server={ip};port={port};database={database};user={user};password={password};SslMode={sslMode};""
             providerName=""MySql.Data.MySqlClient"" />
    </connectionStrings>
</configuration>";

            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "App.config");

            File.WriteAllText(filePath, configContent, Encoding.UTF8);
            MessageBox.Show("Plik App.config został zapisany.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
