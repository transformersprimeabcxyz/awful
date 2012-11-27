using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Awful
{
	public partial class SpoilerTextItem : UserControl
	{
		public SpoilerTextItem()
		{
			// Required to initialize variables
			InitializeComponent();
		}

        private void SpoilerButtonClick(object sender, RoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "ShowSpoiler", true);
        }
	}
}