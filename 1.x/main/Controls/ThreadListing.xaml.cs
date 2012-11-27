using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace Awful
{
	public partial class ThreadListing : UserControl
	{
		public ThreadListing()
		{
			// Required to initialize variables
			InitializeComponent();
		}

		private void LastPageCommandMenuClick(object sender, System.Windows.RoutedEventArgs e)
		{
			PhoneApplicationFrame frame = App.Current.RootVisual as PhoneApplicationFrame;
			if(frame != null)
			{
                var thread = (sender as FrameworkElement).Tag as Models.ThreadData;
                PhoneApplicationService.Current.State["Thread"] = thread;
                frame.Navigate(new Uri("/PostList.xaml?NewPost=true", UriKind.RelativeOrAbsolute));
			}
		}

		private void BookmarkThreadCommandMenuClick(object sender, System.Windows.RoutedEventArgs e)
		{
			// TODO: Add event handler implementation here.
		}
	}
}