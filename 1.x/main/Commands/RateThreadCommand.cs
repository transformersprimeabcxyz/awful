using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Awful.Models;

namespace Awful.Commands
{
    public class RateThreadCommand : CommandBase
    {
        private ThreadData m_Thread;

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
            if (Thread == null) return false;
            int rating = -1;
            if(Int32.TryParse(parameter.ToString(), out rating))
                return true;

            return false;
        }

        public override void Execute(object parameter)
        {
            if (CanExecute(parameter))
            {
                int rating = Int32.Parse(parameter.ToString());
                App.IsBusy = true;
                Services.SomethingAwfulThreadService.Current.RateThreadAsync(Thread, rating,
                    result =>
                    {
                        if (result == Awful.Core.Models.ActionResult.Success)
                        {
                            MessageBox.Show(string.Format("You rated this thread a {0}! Good Job!", rating), ":)",
                                MessageBoxButton.OK);
                        }

                        else
                            MessageBox.Show("Rating failed.", ":(", MessageBoxButton.OK);

                        App.IsBusy = false;
                    });
            }
        }
    }
}
