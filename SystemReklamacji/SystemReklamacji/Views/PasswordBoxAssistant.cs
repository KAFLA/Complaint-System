using System.Security;
using System.Windows;
using System.Windows.Controls;

namespace ReklamacjeSystem.Views
{
    // Klasa pomocnicza dla PasswordBox do obsługi wiązania danych (Data Binding) z ViewModel'em
    public static class PasswordBoxAssistant
    {
        // Dołączona właściwość (Attached Property) do wiązania hasła (SecureString)
        public static readonly DependencyProperty BoundPasswordProperty =
            DependencyProperty.RegisterAttached("BoundPassword", typeof(SecureString), typeof(PasswordBoxAssistant),
                new PropertyMetadata(default(SecureString), OnBoundPasswordChanged));

        // Metoda do pobierania wartości BoundPassword
        public static SecureString GetBoundPassword(DependencyObject d)
        {
            return (SecureString)d.GetValue(BoundPasswordProperty);
        }

        // Metoda do ustawiania wartości BoundPassword
        public static void SetBoundPassword(DependencyObject d, SecureString value)
        {
            d.SetValue(BoundPasswordProperty, value);
        }

        // Metoda wywoływana, gdy zmienia się wartość BoundPassword w ViewModelu
        private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PasswordBox passwordBox = d as PasswordBox;
            if (passwordBox == null)
            {
                return;
            }

            // Zapobieganie rekurencyjnym wywołaniom: jeśli już aktualizujemy hasło programowo, pomiń
            if (GetUpdatingPassword(passwordBox))
            {
                return;
            }

            // Ustaw flagę, że aktualizujemy hasło programowo
            SetUpdatingPassword(passwordBox, true);

            if (e.NewValue is SecureString newPassword)
            {
                // Konwertujemy SecureString na string i ustawiamy w PasswordBox.Password
                // Nowy NetworkCredential tworzy string z SecureString. Następnie ustawiamy tę wartość.
                passwordBox.Password = new System.Net.NetworkCredential(string.Empty, newPassword).Password;
            }
            else
            {
                // Jeśli nowa wartość to null, czyścimy PasswordBox
                passwordBox.Password = string.Empty;
            }

            // Zwolnij flagę aktualizacji
            SetUpdatingPassword(passwordBox, false);
        }

        // Dołączona właściwość pomocnicza do śledzenia, czy aktualizujemy hasło programowo
        private static readonly DependencyProperty IsUpdatingPasswordProperty =
            DependencyProperty.RegisterAttached("IsUpdatingPassword", typeof(bool), typeof(PasswordBoxAssistant), new PropertyMetadata(false));

        // Metoda do pobierania wartości IsUpdatingPassword
        private static bool GetUpdatingPassword(DependencyObject d)
        {
            return (bool)d.GetValue(IsUpdatingPasswordProperty);
        }

        // Metoda do ustawiania wartości IsUpdatingPassword
        private static void SetUpdatingPassword(DependencyObject d, bool value)
        {
            d.SetValue(IsUpdatingPasswordProperty, value);
        }

        // Dołączona właściwość do aktywacji mechanizmu wiązania hasła
        public static readonly DependencyProperty EnablePasswordBindingProperty =
            DependencyProperty.RegisterAttached("EnablePasswordBinding", typeof(bool), typeof(PasswordBoxAssistant),
                new PropertyMetadata(false, OnEnablePasswordBindingChanged)); // Domyślnie false

        // Metoda do pobierania wartości EnablePasswordBinding
        public static bool GetEnablePasswordBinding(DependencyObject d)
        {
            return (bool)d.GetValue(EnablePasswordBindingProperty);
        }

        // Metoda do ustawiania wartości EnablePasswordBinding
        public static void SetEnablePasswordBinding(DependencyObject d, bool value)
        {
            d.SetValue(EnablePasswordBindingProperty, value);
        }

        // Handler dla zdarzenia zmiany właściwości EnablePasswordBinding
        private static void OnEnablePasswordBindingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PasswordBox passwordBox = d as PasswordBox;
            if (passwordBox == null)
            {
                return;
            }

            if ((bool)e.NewValue) // Jeśli EnablePasswordBinding jest ustawione na true
            {
                // Dodajemy handler do zdarzenia PasswordChanged tylko raz
                // Ważne: Usuwamy handler przed dodaniem, aby zapobiec wielokrotnemu subskrybowaniu
                passwordBox.PasswordChanged -= PasswordBox_PasswordChanged;
                passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
            }
            else
            {
                // Usuwamy handler, gdy wiązanie jest wyłączone
                passwordBox.PasswordChanged -= PasswordBox_PasswordChanged;
            }
        }

        // Handler dla zdarzenia PasswordChanged w PasswordBox (użytkownik wpisuje hasło)
        private static void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox passwordBox = sender as PasswordBox;
            // Sprawdź, czy nie jest to zmiana programowa (aby uniknąć pętli)
            if (passwordBox == null || GetUpdatingPassword(passwordBox))
            {
                return;
            }

            // Ustawiamy BoundPassword w ViewModelu na podstawie SecurePassword z PasswordBox
            // To jest kluczowy punkt, w którym hasło z UI trafia do ViewModelu
            // passwordBox.SecurePassword to już SecureString, więc nie potrzebujemy NetworkCredential
            SetBoundPassword(passwordBox, passwordBox.SecurePassword);
        }
    }
}
