using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Awful.Models;

namespace Awful
{
	public partial class PostHeaderControl : UserControl
	{
        private Models.PostData _post;
        public Models.PostData Data
        {
            get { return _post; }
        }

		public PostHeaderControl()
		{
			// Required to initialize variables
			InitializeComponent();
		}

        public PostHeaderControl(Models.PostData post) : this()
        {
			this._post = post;
            LayoutRoot.DataContext = _post;
            Loaded += new RoutedEventHandler(OnLoad);
        }

        void OnLoad(object sender, RoutedEventArgs e)
        {
           
        }
	}
}