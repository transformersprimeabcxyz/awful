using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.Reminders;
using KollaSoft;

namespace Awful
{
	public partial class AboutPanel : UserControl
	{
		public event EventHandler AppThreadRequested;
		public event EventHandler EmailAuthorRequested;
        public event EventHandler RateButtonClick;

		public AboutPanel()
		{
			// Required to initialize variables
			InitializeComponent();
		}

        public bool IsNavigating { get; set; }

		private void ViewAppThreadLinkTapped(object sender, System.Windows.Input.GestureEventArgs e)
		{
            IsNavigating = true;
			AppThreadRequested.Fire(this);
		}

		private void EmailLinkTapped(object sender, System.Windows.Input.GestureEventArgs e)
		{
            IsNavigating = true;
			EmailAuthorRequested.Fire(this);
		}

		private void RateMeButton_Click(object sender, System.Windows.RoutedEventArgs e)
		{
            RateButtonClick.Fire(this);

            DataTemplate template = this.Resources["RateAppTemplate"] as DataTemplate;
            RadRateApplicationReminder reminder = new RadRateApplicationReminder();
            reminder.MessageBoxInfo = new MessageBoxInfoModel()
            {
                Buttons = MessageBoxButtons.YesNo,
                Title = string.Empty,
                Content = new ContentControl() { ContentTemplate = template }
            };

            reminder.AllowUsersToSkipFurtherReminders = false;
            reminder.RecurrencePerUsageCount = 1;
            reminder.Notify();
		}
	}
}