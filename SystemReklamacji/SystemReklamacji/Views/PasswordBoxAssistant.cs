using System.Net;
using System.Security;
using System.Windows;
using System.Windows.Controls;

namespace ReklamacjeSystem.Views
{
    public static class PasswordBoxAssistant
    {
        public static readonly DependencyProperty BoundPasswordProperty =
            DependencyProperty.RegisterAttached("BoundPassword", typeof(SecureString), typeof(PasswordBoxAssistant),
                new PropertyMetadata(default(SecureString), OnBoundPasswordChanged));

        public static SecureString GetBoundPassword(DependencyObject d)
        {
            return (SecureString)d.GetValue(BoundPasswordProperty);
        }

        public static void SetBoundPassword(DependencyObject d, SecureString value)
        {
            d.SetValue(BoundPasswordProperty, value);
        }

        private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PasswordBox passwordBox = d as PasswordBox;
            if (passwordBox == null)
                return;

            if (GetUpdatingPassword(passwordBox))
                return;

            SetUpdatingPassword(passwordBox, true);

            if (e.NewValue is SecureString newPassword)
            {
                string currentPassword = passwordBox.Password;
                string newPasswordString = new NetworkCredential(string.Empty, newPassword).Password;

                if (currentPassword != newPasswordString)
                {
                    passwordBox.Password = newPasswordString;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(passwordBox.Password))
                {
                    passwordBox.Password = string.Empty;
                }
            }

            SetUpdatingPassword(passwordBox, false);
        }

        private static readonly DependencyProperty IsUpdatingPasswordProperty =
            DependencyProperty.RegisterAttached("IsUpdatingPassword", typeof(bool), typeof(PasswordBoxAssistant), new PropertyMetadata(false));

        private static bool GetUpdatingPassword(DependencyObject d)
        {
            return (bool)d.GetValue(IsUpdatingPasswordProperty);
        }

        private static void SetUpdatingPassword(DependencyObject d, bool value)
        {
            d.SetValue(IsUpdatingPasswordProperty, value);
        }

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
                return;

            if ((bool)e.NewValue)
            {
                passwordBox.PasswordChanged -= PasswordBox_PasswordChanged;
                passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
            }
            else
            {
                passwordBox.PasswordChanged -= PasswordBox_PasswordChanged;
            }
        }

        private static void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox passwordBox = sender as PasswordBox;
            if (passwordBox == null || GetUpdatingPassword(passwordBox))
                return;

            SetBoundPassword(passwordBox, passwordBox.SecurePassword);
        }
    }
}
