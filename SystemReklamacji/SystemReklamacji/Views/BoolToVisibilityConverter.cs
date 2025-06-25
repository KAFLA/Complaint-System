using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ReklamacjeSystem.Views
{
    // Konwerter do mapowania wartości boolowskiej na widoczność (Visibility)
    // false -> Collapsed (nie zajmuje miejsca)
    // true -> Visible
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                // Jeśli parametr jest "Inverse", odwróć logikę
                if (parameter?.ToString() == "Inverse")
                {
                    return boolValue ? Visibility.Collapsed : Visibility.Visible;
                }
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed; // Domyślnie niewidoczny
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Ta konwersja nie jest wymagana dla tego scenariusza
            throw new NotImplementedException();
        }
    }
}
