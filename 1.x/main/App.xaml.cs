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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using ImageTools.IO.Gif;
using Awful.Helpers;
using Telerik.Windows.Controls;
using Awful.Models;
using KollaSoft;
using Awful.ViewModels;

namespace Awful
{
    public partial class App : Application
    {
        public static readonly Action DoNothing = new Action(DoNothingMethod);
        public static readonly Action<object> DoNothing2 = new Action<object>(DoNothingMethod);
        private static void DoNothingMethod() { }
        private static void DoNothingMethod(object args) { }

        public static event EventHandler Busy;
        public static event EventHandler Ready;

        private static bool m_isBusy = false;
      
        public static Commands.PostCommand PostCommand
        {
            get { return App.Current.Resources["PostCommand"] as Commands.PostCommand; }
        }
        public static Commands.ImageCommand ImageCommand
        {
            get { return App.Current.Resources["ImageCommand"] as Commands.ImageCommand; }
        }

        public static ContextMenuProvider MenuProvider
        {
            get { return App.Current.Resources["MenuProvider"] as ContextMenuProvider; }
        }

        public static LoginViewModel LoginViewModel
        {
            get { return App.Current.Resources["loginViewModel"] as LoginViewModel; }
        }

        private static AwfulSettings _settings;
        public static AwfulSettings Settings
        {
            get { if (_settings == null) _settings = new AwfulSettings(); return _settings; }
        }

        public static string CurrentUser { get; set; }

        public static bool IsBusy
        {
            get { return m_isBusy; }
            set
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        m_isBusy = value;
                        if (value)
                            Busy.Fire(App.Current);
                        else
                            Ready.Fire(App.Current);
                    });
            }
        }
      
        public static SAForumManager ForumManager
        {
            get { return App.Current.Resources["SAForumManager"] as SAForumManager; }
        }
        public static LayoutManager Layout
        {
            get 
            {
                return App.Current.Resources["LookAndFeel"] as LayoutManager;
            }
        }
 
        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }
        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            // register gif decoder
            ImageTools.IO.Decoders.AddDecoder<GifDecoder>();

            // Global handler for uncaught exceptions. 
            // Note that exceptions thrown by ApplicationBarItem.Click will not get caught here.
            UnhandledException += Application_UnhandledException;

            // Show graphics profiling information while debugging.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Display the current frame rate counters.
                //Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode, 
                // which shows areas of a page that are being GPU accelerated with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;
            }

            // Standard Silverlight initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();
        }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
            Settings.LoadSettings();
            if (App.Settings.LoggingEnabled)
            {
                Awful.Core.Event.Logger.AddEntry("App has launched from Start.");
            }
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            Settings.LoadSettings();
            if (App.Settings.LoggingEnabled)
            {
                Awful.Core.Event.Logger.AddEntry("App has been brought to the foreground.");
            }
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            Settings.SaveSettings();
            if (App.Settings.LoggingEnabled)
            {
                Awful.Core.Event.Logger.AddEntry("App has been deactivated.");
                Awful.Core.Event.Logger.ExportToFile();
            }
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
            Settings.SaveSettings();
            if (App.Settings.LoggingEnabled)
            {
                Awful.Core.Event.Logger.AddEntry("App is closing.");
                Awful.Core.Event.Logger.ExportToFile();
            }
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            e.Handled = true;
            Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    if (App.Settings.LoggingEnabled)
                    {
                        var result = MessageBox.Show("A navigation error occured. Tap OK to report this error to the author.", 
                            ":'(", MessageBoxButton.OKCancel);
                        
                        if (result == MessageBoxResult.OK)
                        {
                            Commands.EmailCommand email = new Commands.EmailCommand();
                            email.To = Globals.Constants.AUTHOR_EMAIL;
                            email.Subject = "Navigation Error";
                            email.Body = string.Format("'{0}'", e.Uri);
                            email.Execute(null);
                        }
                    }

                    if (System.Diagnostics.Debugger.IsAttached)
                    {
                        // A navigation has failed; break into the debugger
                        System.Diagnostics.Debugger.Break();
                    }
                });
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    if (App.Settings.LoggingEnabled)
                    {
                        var result = MessageBox.Show("An unknown error occured. Tap OK to report this error to the author.", 
                            ":'(", MessageBoxButton.OKCancel);
                        
                        if (result == MessageBoxResult.OK)
                        {
                            Commands.EmailCommand email = new Commands.EmailCommand();
                            email.To = Globals.Constants.AUTHOR_EMAIL;
                            email.Subject = "Unknown Error";
                            email.Body = string.Format("[{0}] - {1}", e.ExceptionObject.Message, e.ExceptionObject.StackTrace);
                            email.Execute(null);
                        }

                        if (System.Diagnostics.Debugger.IsAttached)
                        {
                            // An unhandled exception has occurred; break into the debugger

                            MessageBox.Show(e.ExceptionObject.Message,
                                "error", MessageBoxButton.OK);

                            System.Diagnostics.Debugger.Break();
                        }
                    }
                });
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            // Uncomment this for standard:
            // RootFrame = new PhoneApplicationFrame();
            // Uncomment this for silverlight tools
            // RootFrame = new TransitionFrame();
            // Uncomment this for telerik frames
            RadPhoneApplicationFrame frame = new RadPhoneApplicationFrame();

            // rad frame transistions
            RadTransition transition = new RadTransition();
            transition.BackwardInAnimation = this.Resources["fadeInAnimation"] as RadAnimation;
            transition.BackwardOutAnimation = this.Resources["fadeOutAnimation"] as RadAnimation;
            transition.ForwardInAnimation = this.Resources["fadeInAnimation"] as RadAnimation;
            transition.ForwardOutAnimation = this.Resources["fadeOutAnimation"] as RadAnimation;
            transition.PlayMode = TransitionPlayMode.Consecutively;

            frame.Transition = transition;
            frame.OrientationChangeAnimation = this.Resources["fadeInAnimation"] as RadFadeAnimation;

            RootFrame = frame;
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion
    }
}