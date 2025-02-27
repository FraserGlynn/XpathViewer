using System;
using System.Windows.Input;

namespace XpathViewer
{
    internal class RelayCommand : ICommand
    {

        private readonly Action _action;

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => true;

        public RelayCommand(Action action) => _action = action;

        public void Execute(object parameter)
        {
            Action action = _action;
            action();
        }

    }
}
