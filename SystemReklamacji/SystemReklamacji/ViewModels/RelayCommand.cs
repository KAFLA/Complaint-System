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

        // Metoda jawnie wywołująca zdarzenie CanExecuteChanged
        public void RaiseCanExecuteChanged()
        {
            // To jest bezpieczny sposób na wywołanie zdarzenia, które aktualizuje UI
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
