using System;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows;
using Awful.Models;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Controls;
using KollaSoft;

namespace Awful.Commands
{
    public class ViewForumCommand : CommandBase
    {
        public string Url { get; set; }
        public event EventHandler Navigating;

        public override bool CanExecute(object parameter)
        {
            try
            {
                int id = int.Parse(parameter.ToString());
                return true;
            }

            catch (Exception) 
            {
                MessageBox.Show("Sorry, that's an invalid id.", ":(", MessageBoxButton.OK);
                return false; 
            }
        }

        public override void Execute(object parameter)
        {
            if (CanExecute(parameter))
            {
                int id = int.Parse(parameter.ToString());
                SAForum forum = new SAForum() { ID = id };

                Navigating.Fire(this);
                NavigateToForum(forum);
            }
        }

        private void NavigateToForum(SAForum forum)
        {
            if (forum == null)
            {
                MessageBox.Show("Could not locate valid forum.", ":(", MessageBoxButton.OK);
                return;
            }

            PhoneApplicationService.Current.State["Forum"] = forum;
            var frame = App.Current.RootVisual as PhoneApplicationFrame;
            if (frame != null)
            {
                string uri = "/ThreadList.xaml?ID=" + forum.ID;
                frame.Navigate(new Uri(uri, UriKind.RelativeOrAbsolute));
            }
        }
    }
}
