using System.Security;
using System.Windows;
using System.Windows.Controls;

namespace ReklamacjeSystem.Views
{
    // Klasa pomocnicza dla PasswordBox do obsługi Data Bindingu
    public static class PasswordBoxAssistant
    {
        // Dołączona właściwość dla bezpiecznego wiązania hasła
        public static readonly DependencyProperty BoundPasswordProperty =
            DependencyProperty.RegisterAttached("BoundPassword", typeof(SecureString), typeof(PasswordBoxAssistant),
                new PropertyMetadata(default(SecureString), OnBoundPasswordChanged));

        // Metoda Get dla dołączonej właściwości
        public static SecureString GetBoundPassword(DependencyObject d)
        {
            return (SecureString)d.GetValue(BoundPasswordProperty);
        }

        // Metoda Set dla dołączonej właściwości
        public static void SetBoundPassword(DependencyObject d, SecureString value)
        {
            d.SetValue(BoundPasswordProperty, value);
        }

        // Handler, który jest wywoływany, gdy wartość BoundPassword się zmienia
        private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PasswordBox passwordBox = d as PasswordBox;
            if (passwordBox == null)
            {
                return;
            }

            // Uniemożliwia rekurencyjne wywołanie
            passwordBox.PasswordChanged -= PasswordBox_PasswordChanged;

            if (e.NewValue is SecureString newPassword)
            {
                // Aktualizuj PasswordBox tylko jeśli hasło w ViewModelu jest inne
                // (porównanie SecureString jest skomplikowane i często unika się go dla bezpośredniego porównania wartości)
                // W tym przypadku po prostu ustawiamy nową wartość
                passwordBox.Password = new System.Net.NetworkCredential(string.Empty, newPassword).Password;
            }
            else
            {
                passwordBox.Password = string.Empty;
            }

            passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
        }

        // Dołączona właściwość do śledzenia, czy już dołączyliśmy handler
        private static readonly DependencyProperty IsUpdatingProperty =
            DependencyProperty.RegisterAttached("IsUpdating", typeof(bool), typeof(PasswordBoxAssistant));

        private static bool GetIsUpdating(DependencyObject d)
        {
            return (bool)d.GetValue(IsUpdatingProperty);
        }

        private static void SetIsUpdating(DependencyObject d, bool value)
        {
            d.SetValue(IsUpdatingProperty, value);
        }

        // Handler dla zdarzenia PasswordChanged w PasswordBox
        private static void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox passwordBox = sender as PasswordBox;
            if (passwordBox == null || GetIsUpdating(passwordBox))
            {
                return;
            }

            SetIsUpdating(passwordBox, true);
            // Tworzymy SecureString z aktualnego hasła PasswordBox i przypisujemy do BoundPassword
            SetBoundPassword(passwordBox, new System.Net.NetworkCredential(string.Empty, passwordBox.SecurePassword).SecurePassword);
            SetIsUpdating(passwordBox, false);
        }

        // Dołączona właściwość do "enable" zachowania wiązania
        public static readonly DependencyProperty EnablePasswordBindingProperty =
            DependencyProperty.RegisterAttached("EnablePasswordBinding", typeof(bool), typeof(PasswordBoxAssistant),
                new PropertyMetadata(false, OnEnablePasswordBindingChanged));

        public static bool GetEnablePasswordBinding(DependencyObject d)
        {
            return (bool)d.GetValue(EnablePasswordBindingProperty);
        }

        public static void SetEnablePasswordBinding(DependencyObject d, bool value)
        {
            d.SetValue(EnablePasswordBindingProperty, value);
        }

        private static void OnEnablePasswordBindingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PasswordBox passwordBox = d as PasswordBox;
            if (passwordBox == null)
            {
                return;
            }

            if ((bool)e.NewValue)
            {
                passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
            }
            else
            {
                passwordBox.PasswordChanged -= PasswordBox_PasswordChanged;
            }
        }
    }
}
