using System;
using System.Windows.Input;
using KollaSoft;
using Awful.Models;
using System.Windows;
using Awful.Helpers;

namespace Awful.Commands
{
    public class ReplyCommand : CommandBase
    {
        private readonly Services.SomethingAwfulThreadService _threadSvc = Services.SomethingAwfulThreadService.Current;
        private ThreadData m_Thread;

        public event EventHandler<ActionResultEventArgs> RequestCompleted;

        public ThreadData Thread
        {
            get { return m_Thread; }
            set
            {
                if (m_Thread == value) return;
                m_Thread = value;
                NotifyCanExecuteChanged();
                NotifyPropertyChangedAsync("Thread");
            }
        }

        public override bool CanExecute(object parameter)
        {
            if (m_Thread == null) return false;
            if (parameter == null) return false;
            var text = parameter.ToString();
            if (String.IsNullOrEmpty(text)) return false;
            return true;
        }

        public override void Execute(object parameter)
        {
            if (CanExecute(parameter))
            {
                App.IsBusy = true;
                var text = parameter.ToString();

                // Debugging here
                // ***********************************************************
                // ***********************************************************
                // MessageBox.Show(text, "debug reply", MessageBoxButton.OK);
                // App.IsBusy = false;
                // RequestCompleted.Fire<ActionResultEventArgs>(this, newActionResultEventArgs(Awful.Core.Models.ActionResult.Success));
                // return;
                // ***********************************************************
                // ***********************************************************
                
                _threadSvc.ReplyAsync(m_Thread, text, result =>
                    {
                        if (result == Awful.Core.Models.ActionResult.Success)
                            MessageBox.Show("Reply successful!", ":)", MessageBoxButton.OK);
                        else
                            MessageBox.Show("Reply failed.", ":(", MessageBoxButton.OK);

                        App.IsBusy = false;

                        RequestCompleted.Fire<ActionResultEventArgs>(this, new ActionResultEventArgs(result));
                    });
            }
        }
    }
}
