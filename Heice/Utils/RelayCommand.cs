#nullable enable
using System;
using System.Windows.Input;

namespace Heice.Utils
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _executeAction;
        private readonly Func<object?, bool>? _canExecuteFunction;
        
        public RelayCommand(Action<object?> execute)
        {
            _executeAction = execute;
        }
        
        public RelayCommand(Action<object?> execute, Func<object?, bool> canExecute) : this(execute)
        {
            _canExecuteFunction = canExecute;
        }
        
        public bool CanExecute(object? parameter)
        {
            return _canExecuteFunction == null || _canExecuteFunction(parameter);
        }

        public void Execute(object? parameter)
        {
            _executeAction(parameter);
        }

        public event EventHandler? CanExecuteChanged;
    }
}