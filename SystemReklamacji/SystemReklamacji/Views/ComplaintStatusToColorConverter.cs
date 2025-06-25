using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using ReklamacjeSystem.Models; // Wymagane dla ComplaintStatus

namespace ReklamacjeSystem.Views
{
    // Konwerter do mapowania ComplaintStatus na kolor
    public class ComplaintStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ComplaintStatus status)
            {
                switch (status)
                {
                    case ComplaintStatus.New:
                        return new SolidColorBrush(Colors.OrangeRed); // Nowa - czerwono-pomarańczowy
                    case ComplaintStatus.InProgress:
                        return new SolidColorBrush(Colors.DarkOrange); // W toku - ciemnopomarańczowy
                    case ComplaintStatus.Resolved:
                        return new SolidColorBrush(Colors.DarkGreen); // Rozwiązana - ciemnozielony
                    case ComplaintStatus.Closed:
                        return new SolidColorBrush(Colors.Gray); // Zamknięta - szary
                    default:
                        return new SolidColorBrush(Colors.Black);
                }
            }
            return new SolidColorBrush(Colors.Black);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException(); // Konwersja w drugą stronę nie jest potrzebna w tym przypadku
        }
    }
}
