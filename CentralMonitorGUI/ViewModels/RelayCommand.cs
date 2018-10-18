using System;
using System.Windows.Input;

namespace CentralMonitorGUI.ViewModels
{
    public class RelayCommand : ICommand
    {
        private readonly Action action;
        private readonly Func<bool> canExecute;

        public RelayCommand(Action action)
        {
            this.action = action;
            canExecute = () => true;
        }
        public RelayCommand(Action action, Func<bool> canExecute)
        {
            this.action = action;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return canExecute();
        }

        public void Execute(object parameter)
        {
            action();
        }

        public event EventHandler CanExecuteChanged;
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> action;
        private readonly Func<bool> canExecute;

        public RelayCommand(Action<T> action)
        {
            this.action = action;
            canExecute = () => true;
        }
        public RelayCommand(Action<T> action, Func<bool> canExecute)
        {
            this.action = action;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return canExecute();
        }

        public void Execute(object parameter)
        {
            action((T)parameter);
        }

        public event EventHandler CanExecuteChanged;
    }
}