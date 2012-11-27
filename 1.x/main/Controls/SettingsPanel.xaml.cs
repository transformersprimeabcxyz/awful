using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO.IsolatedStorage;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows.Threading;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using Awful.Models;
using KollaSoft;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.Reminders;

namespace Awful
{
	public partial class SettingsPanel : UserControl
	{
        private readonly SettingsPanelViewModel viewModel = new SettingsPanelViewModel();
        private DispatcherTimer busyTimer = new DispatcherTimer();
       
        public event EventHandler Busy;
        public event EventHandler Ready;
		public event EventHandler SysTrayToggled;

        public SettingsPanel()
        {
            // Required to initialize variables
            InitializeComponent();
            BindEvents();
            busyTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
        }

        void BindEvents()
        {
            busyTimer.Tick += new EventHandler(BusyTimer_Tick);
            this.Loaded += new RoutedEventHandler(SettingsPanel_Loaded);
            this.AwfulAboutPanel.RateButtonClick += new EventHandler(AwfulAboutPanel_RateButtonClick);
            this.AwfulAboutPanel.AppThreadRequested += new EventHandler(AwfulAboutPanel_AppThreadRequested);
            this.AwfulAboutPanel.EmailAuthorRequested += new EventHandler(AwfulAboutPanel_EmailAuthorRequested);
            TextColorSlider.ManipulationStarted += new EventHandler<ManipulationStartedEventArgs>(TextColorSlider_ManipulationStarted);
            TextColorSlider.ManipulationCompleted += new EventHandler<ManipulationCompletedEventArgs>(TextColorSlider_ManipulationCompleted);
        }

        void AwfulAboutPanel_RateButtonClick(object sender, EventArgs e)
        {
            this.AboutWindow.IsOpenThenInvoke(false, () =>
                {
                   
                });
        }

        void SettingsPanel_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = viewModel; 
        }

        void AwfulAboutPanel_AppThreadRequested(object sender, EventArgs e)
        {
            AboutWindow.IsOpenThenInvoke(false, () =>
                {
                    var navigate = new Commands.ViewThreadCommand();
                    navigate.Url = "http://forums.somethingawful.com/showthread.php?threadid=3460814";
                    navigate.Execute(Commands.ViewThreadCommand.USE_URL);
                });
        }

        void AwfulAboutPanel_EmailAuthorRequested(object sender, EventArgs e)
        {
            AboutWindow.IsOpenThenInvoke(false, () =>
                {
                    var email = new Commands.EmailCommand();
                    email.Subject = "Awful QCS";
                    email.To = Globals.Constants.AUTHOR_EMAIL;
                    email.Execute(null);
                });
        }

        void BusyTimer_Tick(object sender, EventArgs e)
        {
            Ready.Fire(this);
            if (busyTimer.IsEnabled)
                busyTimer.Stop();
        }

        void TextColorSlider_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            if (!busyTimer.IsEnabled)
                busyTimer.Start();
        }

        void TextColorSlider_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            Busy.Fire(this);
            if (busyTimer.IsEnabled)
                busyTimer.Stop();
        }

        public void Load() {}

        private void StaticButtonPanel_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as Button).Tag.ToString().ToLower())
            {
                case "clear":
                    viewModel.ClearAccount();
                    break;

                case "default":
                    var result = MessageBox.Show("Load defaults?", ":o", MessageBoxButton.OKCancel);
                    if (result == MessageBoxResult.OK)
                    {
                        App.Settings.SetToDefault = true;
                        MessageBox.Show("Restart Awful to view changes.", ":)", MessageBoxButton.OK);
                    }
                    break;

                case "about":
                    AboutWindow.IsOpen = true;
                    break;
            }
        }

        private void SendDebugButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        	var frame = App.Current.RootVisual as PhoneApplicationFrame;
			frame.Navigate(new Uri("/ViewDebug.xaml", UriKind.RelativeOrAbsolute));
        }

        private void SettingsPicker_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
           
        }

        private void LockScreen_Click(object sender, RoutedEventArgs e)
        {
            var check = sender as CheckBox;
            if (check.IsChecked == true)
            {
                var result = MessageBox.Show("Enabling this will allow for a better browsing experience, " +
                                "at the cost of decreased battery life. Proceed?",
                                ":o", MessageBoxButton.OKCancel);

                if (result == MessageBoxResult.Cancel)
                {
                    App.Settings.RunUnderLockScreen = false;
                    return;
                }
            }

            MessageBox.Show("Restart Awful to enable changes.",
                            ":)", MessageBoxButton.OK);
        }

        private void HideSysTray_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        	SysTrayToggled.Fire(this);
        }
	}

    public class SettingsPanelViewModel : PropertyChangedBase
    {
        public enum TextSize { tiny = 12, small = 14, medium = 16, large = 20 };
        public enum HomePageType { Forums = 0, Favorites = 1, Bookmarks = 2 }

        private readonly List<TextSize> textSizes = new List<TextSize>()
        {
            TextSize.tiny,
            TextSize.small,
            TextSize.medium,
            TextSize.large
        };

        private List<AwfulSettings.ViewStyle> viewStyles = new List<AwfulSettings.ViewStyle>()
        {
            AwfulSettings.ViewStyle.Horizontal,
            AwfulSettings.ViewStyle.Vertical
        };

        private List<HomePageType> homePagePresets = new List<HomePageType>()
        {
            HomePageType.Forums,
            HomePageType.Favorites,
            HomePageType.Bookmarks
        };

        private List<AwfulSettings.ForumGroup> forumGroups = new List<AwfulSettings.ForumGroup>()
        {
            AwfulSettings.ForumGroup.Alphanumeric,
            AwfulSettings.ForumGroup.Classic
        };

        private readonly Dictionary<string, int> timeoutPresets = new Dictionary<string, int>(3);
        private readonly Dictionary<int, string> reversedTimeoutPresets = new Dictionary<int, string>(3);
        private TextSize selectedTextSize;

        public AwfulSettings Settings
        {
            get { return App.Settings; }
        }

        public IEnumerable<AwfulSettings.ViewStyle> ViewStyles { get { return viewStyles; } }
        public IEnumerable<TextSize> TextSizes { get { return textSizes; } }
        public IEnumerable<AwfulSettings.ForumGroup> ForumGroups { get { return forumGroups; } }

        public AwfulSettings.ForumGroup SelectedForumGroup
        {
            get { return Settings.ForumGrouping; }
            set
            {
                Settings.ForumGrouping = value;
                NotifyPropertyChangedAsync("SelectedForumGroup");
                HomePage.ViewModel.InitializeForumGroups(value);
            }
        }

        public AwfulSettings.ViewStyle SelectedViewStyle
        {
            get { return Settings.ThreadViewStyle; }
            set
            {
                Settings.ThreadViewStyle = value;
                NotifyPropertyChangedAsync("SelectedViewStyle");
                NotifyPropertyChangedAsync("SmoothScrollToggleEnabled");
            }
        }

        public bool CensorEnabled
        {
            get { return this.Settings.AreParentalControlsEnabled; }
            set
            {
                this.Settings.AreParentalControlsEnabled = Verify18YearsOrOlder(value);
                NotifyPropertyChangedAsync("CensorEnabled");
            }
        }

        private bool Verify18YearsOrOlder(bool value)
        {
            if (!value)
            {
                var confirm = MessageBox.Show(
                    "Content on the SomethingAwful Forums may contain content deemed " +
                    "unsuitable for minors. By clicking OK, you confirm that you " +
                    "are at least 18 years of age. If not, please click CANCEL to return " +
                    "back to the Settings menu.",
                    ":o",
                    MessageBoxButton.OKCancel);

                if (confirm == MessageBoxResult.Cancel)
                    value = true;
            }

            return value;
        }

        public bool SmoothScrollToggleEnabled
        {
            get { return this.Settings.ThreadViewStyle == AwfulSettings.ViewStyle.Vertical; }
        }

        public TextSize SelectedTextSize
        {
            get 
            {
                return selectedTextSize;
            }
            set
            {
                selectedTextSize = value;
                Settings.PostTextSize = (int) value;
                NotifyPropertyChangedAsync("SelectedTextSize");
                NotifyPropertyChangedAsync("SampleTextSize");
            }
        }

        public string LoggingStatus
        {
            get
            {
                if (this.Settings.LoggingEnabled)
                    return "on";
                else
                    return "off";
            }
        }

        public bool IsPageSwipeEnabled
        {
            get { return Settings.PageSwipeThreshold != AwfulSettings.PAGE_SWIPE_THRESHOLD_DISABLED_VALUE; }
            set
            {
                if (value) { this.Settings.PageSwipeThreshold = AwfulSettings.PAGE_SWIPE_THRESHOLD_DEFAULT_VALUE; }
                else { this.Settings.PageSwipeThreshold = AwfulSettings.PAGE_SWIPE_THRESHOLD_DISABLED_VALUE; }
                this.NotifyPropertyChanged("IsPageSwipeEnabled");
                this.NotifyPropertyChanged("PageSwipeCurrentValue");
            }
        }

        private const double PAGE_SWIPE_SETTINGS_LARGE_STEP = 100;
        private const double PAGE_SWIPE_SETTINGS_SMALL_STEP = 50;
        public double PageSwipeSmallStep { get { return PAGE_SWIPE_SETTINGS_SMALL_STEP; } }
        public double PageSwipeLargeStep { get { return PAGE_SWIPE_SETTINGS_LARGE_STEP; } }
        public double PageSwipeMinValue { get { return AwfulSettings.PAGE_SWIPE_THRESHOLD_DEFAULT_VALUE; } }
        public double PageSwipeMaxValue { get { return AwfulSettings.PAGE_SWIPE_THRESHOLD_MAX_VALUE; } }
        public double PageSwipeCurrentValue
        {
            get { return Math.Min(this.PageSwipeMaxValue, this.Settings.PageSwipeThreshold); }
            set
            {
                this.Settings.PageSwipeThreshold = value;
                this.NotifyPropertyChanged("PageSwipeCurrentValue");
            }
        }

        public double SampleTextSize
        {
            get { return (double)Settings.PostTextSize; }
        }

        public IEnumerable<string> TimeoutPresets { get { return timeoutPresets.Keys; } }
        
        public IEnumerable<HomePageType> StartupViewPresets { get { return homePagePresets; } }
       
        public void ClearAccount()
        {
            if (MessageBox.Show("You will have to reenter your account information the next time you restart Awful. Proceed?",
               ":o", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                Settings.Username = String.Empty;
                Settings.Password = String.Empty;
                Settings.SaveAccount = false;
                MessageBox.Show("Account cleared.", ":)", MessageBoxButton.OK);
            }
        }

        public SettingsPanelViewModel()
        {
            timeoutPresets.Add("10 seconds", 10000);
            timeoutPresets.Add("30 seconds", 30000);
            timeoutPresets.Add("1 minute", 60000);

            reversedTimeoutPresets.Add(10000, "10 seconds");
            reversedTimeoutPresets.Add(30000, "30 seconds");
            reversedTimeoutPresets.Add(60000, "1 minute");

            selectedTextSize = (TextSize)Settings.PostTextSize;

            Settings.PropertyChanged += (obj, args) =>
            {
                if (args.PropertyName.Equals("LoggingEnabled"))
                    NotifyPropertyChangedAsync("LoggingStatus");
            };
        }
    }
}