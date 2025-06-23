using System;
using System.Windows.Input; // Wymagane dla ICommand

namespace ReklamacjeSystem.ViewModels
{
    // Implementacja ICommand, która pozwala na wiązanie UI z logiką w ViewModelu
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute; // Akcja do wykonania
        private readonly Predicate<object> _canExecute; // Predykat do sprawdzenia, czy akcja może być wykonana

        // Zdarzenie, które informuje UI o zmianie stanu możliwości wykonania komendy
        // Domyślnie używa CommandManager.RequerySuggested do automatycznego wywoływania
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        // Konstruktor przyjmujący akcję do wykonania i opcjonalnie predykat
        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        // Sprawdza, czy komenda może być wykonana
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        // Wykonuje komendę
        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        // KLUCZOWA ZMIANA: Metoda jawnie wywołująca zdarzenie CanExecuteChanged
        // Pozwala to ViewModelom na ręczne wymuszenie ponownej oceny stanu CanExecute komendy
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
            // Alternatywnie, jeśli nie chcemy polegać na CommandManagerze:
            // CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
