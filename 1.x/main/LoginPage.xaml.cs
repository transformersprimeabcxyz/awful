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
using Awful.Models;
using Telerik.Windows.Controls;
using Awful.Helpers;
using Telerik.Windows.Data;
using Awful.Commands;
using Awful.ViewModels;
using System.Diagnostics;

namespace Awful
{
    public partial class LoginPage : PhoneApplicationPage
    {
        private LoginViewModel login;
        
      
        public LoginPage()
        {
            InitializeComponent();
            this.login = App.LoginViewModel;
            BindEvents();
            this.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void BindEvents()
        {
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
            this.Unloaded += new RoutedEventHandler(MainPage_Unloaded);
            this.login.GrabbingCookies += new EventHandler(login_GrabbingCookies);
        }

        private void login_GrabbingCookies(object sender, EventArgs e)
        {
            this.manualLoginTap.IsHitTestVisible = false;
            this.browserWindow.IsOpen = false;
        }

        private void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
          
        }

        private void CheckLoginStatus()
        {
            if (Awful.Core.Web.AwfulWebRequest.CanAuthenticate)
            {
                GoToShowForumsState();
            }

            else
            {
                if (App.Settings.CurrentProfileID != -1)
                {
                    GoToAutoLoginState();
                    Login();
                }

                else
                    GoToShowLoginState();
            }   
        }

        private void GoToManualLoginState()
        {
            VisualStateManager.GoToState(this, "ManualLogin", true);
        }

        private void GoToShowForumsState()
        {
			NavigationService.Navigate(new Uri("/HomePage.xaml", UriKind.RelativeOrAbsolute));
        }

        private void GoToAutoLoginState()
        {
            VisualStateManager.GoToState(this, "AutoLogin", true);
        }

        private void GoToShowLoginState()
        {
            //this.manualLoginTap.IsHitTestVisible = true;
            //this.browserWindow.IsOpen = false;

            VisualStateManager.GoToState(this, "ShowLogin", true);
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            CheckLoginStatus();
        }

        private void ShowLoginButtonClick(object sender, System.EventArgs e)
        {
            VisualStateManager.GoToState(this, "ShowLogin", true);
        }

        private void Login()
        {
            this.login.LoginAsync(result =>
               {
                   if (result == Awful.Core.Models.ActionResult.Success)
                   {
                       GoToShowForumsState();
                   }
                   else
                       GoToShowLoginState();
               });
        }

        private void LoginButtonClick(object sender, RoutedEventArgs e)
        {
            GoToManualLoginState();
            Login();
        }
      
        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            // ensure the login browser is no longer attached //
            this.browserBorder.Child = null;

            base.OnNavigatedFrom(e);
            this.NavigationService.RemoveBackEntry();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            this.browserBorder.Child = this.login.Browser; 

            if (e.IsNavigationInitiator)
            {
                this.CheckLoginStatus();
            }

            else
            {
                this.Visibility = System.Windows.Visibility.Visible;
                base.OnNavigatedTo(e);
            }
        }

        private void OnOrientationChanged(object sender, Microsoft.Phone.Controls.OrientationChangedEventArgs e)
        {
            Thickness margin = this.LoginPanel.Margin;

            if (this.Orientation.IsPortrait()) { margin.Top = 88; }
            else { margin.Top = 0; }
        }

        private void ManualLoginTap_Tapped(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.browserWindow.IsOpenThenInvoke(true, () => { Login(); });
        }
    }
}
