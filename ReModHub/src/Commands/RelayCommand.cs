using System.Windows.Input;

namespace ReModHub.Commands
{
    public class RelayCommand : ICommand
    {
        private readonly Func<Task> execute;
        private readonly Func<bool>? canExecute;
        private bool isExecuting;

        public RelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter)
        {
            return !isExecuting && (canExecute?.Invoke() ?? true);
        }

        public async void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
            {
                return;
            }

            try
            {
                isExecuting = true;
                CommandManager.InvalidateRequerySuggested();
                await execute();
            }
            finally
            {
                isExecuting = false;
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Func<T?, Task> execute;
        private readonly Func<T?, bool>? canExecute;
        private bool isExecuting;

        public RelayCommand(Func<T?, Task> execute, Func<T?, bool>? canExecute = null)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter)
        {
            return !isExecuting && (canExecute?.Invoke((T?)parameter) ?? true);
        }

        public async void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
            {
                return;
            }

            try
            {
                isExecuting = true;
                CommandManager.InvalidateRequerySuggested();
                await execute((T?)parameter);
            }
            finally
            {
                isExecuting = false;
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }
} 
