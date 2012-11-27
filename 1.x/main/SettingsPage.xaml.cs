using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace Awful
{
    public partial class SettingsPage : PhoneApplicationPage
    {
        public SettingsPage()
        {
            InitializeComponent();
			this.SettingsPanel.SysTrayToggled += new System.EventHandler(SettingsPanel_SysTrayToggled);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            SystemTray.IsVisible = !App.Settings.HideSystemTray;
            base.OnNavigatedTo(e);
        }

        private void SettingsPanel_SysTrayToggled(object sender, System.EventArgs e)
        {
        	SystemTray.IsVisible = !App.Settings.HideSystemTray;
        }
    }
}
