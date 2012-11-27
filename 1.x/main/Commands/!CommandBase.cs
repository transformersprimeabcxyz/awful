using System;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows;
using KollaSoft;

namespace Awful.Commands
{
    public abstract class CommandBase : PropertyChangedBase, ICommand
    {
        public abstract bool CanExecute(object parameter);

        public event EventHandler CanExecuteChanged;
        public abstract void Execute(object parameter);

        protected void NotifyCanExecuteChanged()
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    if (CanExecuteChanged != null)
                        CanExecuteChanged(this, EventArgs.Empty);
                });
        }
    }

    public abstract class CommandBase<T> : CommandBase
    {
        public abstract override void Execute(object parameter);

        protected virtual bool TypeCheck(object parameter)
        {
            return parameter is T;
        }

        public override bool CanExecute(object parameter)
        {
            return TypeCheck(parameter);
        }
    }
}
