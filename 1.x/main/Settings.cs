using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.IO.IsolatedStorage;
using System.Collections.Generic;
using Awful.Models;
using Awful.Helpers;
using Microsoft.Phone.Shell;
using KollaSoft;
using System.Windows.Media;
using System.Diagnostics;
using Telerik.Windows.Controls;
using System.Net;

namespace Awful
{
    public class AwfulSettings : Settings
    {
        // ********************************************************
        // ******************** VERSION INFO **********************
        // **** UPDATE THIS BEFORE YOU SUBMIT TO THE APP STORE! ***
        
        public const string VERSION_INFO = "1.15.302";
        public const int VERSION_DB = 2;
        
        // ********************************************************
        // ********************************************************

        public enum ViewStyle { Horizontal=0, Vertical };
        public enum ForumGroup { Alphanumeric = 0, Classic = 1 };

        #region Fields

        private readonly Data.DatabaseManager DBUpdater = new Data.DatabaseManager();

        // presets
 
        private readonly Dictionary<string, int> timeoutPresets = new Dictionary<string, int>(3);
        
        // Isolated storage and setting keys
        IsolatedStorageSettings settings;
        const string PARENTAL_CONTROLS_ENABLED = "ParentalControlsEnabled";
        const string VERSION_INFO_KEY = "VersionInfo";
        const string USERNAME = "Username";
        const string PASSWORD = "Password";
        const string SAVE_ACCOUNT = "SaveAccount";
        const string IMG_HEIGHT = "ImageHeight";
        const string IMG_WIDTH = "ImageWidth";
        const string POST_TEXT_SIZE = "PostTextSize";
        const string THREAD_TIMEOUT = "ThreadTimeout";
        const string SWIPE_SENSITIVITY = "SwipeSensitivity";
        const string HOME_PAGE = "HomePage";
        const string LAST_VIEWED_THREAD = "LastViewedThread";
        const string LAST_VIEWED_FORUM = "LastViewedForum";
        const string POST_TEXT_COLOR_SLIDER_VALUE = "PostTextColorSliderValue";
        const string FORUM_FAVORITES = "ForumFavorites";
        const string LOGGING_ENABLED = "LoggingEnabled";
        const string COOKIE_JAR = "CookieJar";
        const string VIEWSTYLE = "ViewStyle";
        const string SMOOTH_SCROLL = "SmoothScroll";
        const string FORUM_GROUPING = "ForumGrouping";
        const string RUN_UNDER_LOCK = "RunUnderLockScreen";
        const string SET_DEFAULT = "SetDefault";
        const string CURRENT_PROFILE_ID = "CurrentProfileID";
        const string HIDE_SYSTRAY = "HideSystemTray";

        // defaults
        const string VERSION_INFO_DEFAULT = "1.0.0";
        public const bool PARENTAL_CONTROLS_ENABLED_DEFAULT = true;
        public const bool HIDE_SYSTRAY_DEFAULT = false;
        public const int CURRENT_PROFILE_ID_DEFAULT = -1;
        public const bool SET_DEFAULTS_DEFAULT = true;
        public const bool RUN_UNDER_LOCK_DEFAULT = false;
        public const ForumGroup FORUM_GROUPING_DEFAULT = ForumGroup.Alphanumeric;
        public const bool SMOOTH_SCROLL_DEFAULT = false;
        public const ViewStyle VIEWSTYLE_DEFAULT = ViewStyle.Vertical;
        public const bool LOGGING_ENABLED_DEFAULT = false;
        public const int HOME_PAGE_DEFAULT = 0;
        public const int THREAD_TIMEOUT_DEFAULT = 20000;
        public const string USERNAME_DEFAULT = "";
        public const string PASSWORD_DEFAULT = "";
        public const bool SAVE_ACCOUNT_DEFAULT = false;
        public const double IMG_HEIGHT_DEFAULT = 100;
        public const double IMG_WIDTH_DEFAULT = 350;
        public const int POST_TEXT_SIZE_DEFAULT = 14;
        public const int SWIPE_SENSITIVITY_DEFAULT = 50;
        public const double POST_TEXT_COLOR_SLIDER_VALUE_DEFAULT = 0.52;
        
        #endregion

        #region Properties

        private const string PAGE_SWIPE_THRESHOLD_KEY = "PageSwipeThreshold";
        public const double PAGE_SWIPE_THRESHOLD_DEFAULT_VALUE = 200;
        public const double PAGE_SWIPE_THRESHOLD_MAX_VALUE = 2000;
        public const double PAGE_SWIPE_THRESHOLD_DISABLED_VALUE = double.MaxValue;
        public double PageSwipeThreshold
        {
            get { return this.GetValueOrDefault<double>(PAGE_SWIPE_THRESHOLD_KEY, PAGE_SWIPE_THRESHOLD_DEFAULT_VALUE); }
            set 
            { 
                this.AddOrUpdateValue(PAGE_SWIPE_THRESHOLD_KEY, value);
                this.NotifyPropertyChangedAsync("PageSwipeThreshold");
            }
        }

        public bool AreParentalControlsEnabled
        {
            get { return this.GetValueOrDefault<bool>(PARENTAL_CONTROLS_ENABLED, PARENTAL_CONTROLS_ENABLED_DEFAULT); }
            set
            {
                this.AddOrUpdateValue(PARENTAL_CONTROLS_ENABLED, value);
                this.NotifyPropertyChanged("AreParentalControlsEnabled");
            }
        }

        public bool HideSystemTray
        {
            get { return GetValueOrDefault<bool>(HIDE_SYSTRAY, HIDE_SYSTRAY_DEFAULT); }
            set { AddOrUpdateValue(HIDE_SYSTRAY, value); }
        }

        public int CurrentProfileID
        {
            get { return GetValueOrDefault<int>(CURRENT_PROFILE_ID, CURRENT_PROFILE_ID_DEFAULT); }
            set { AddOrUpdateValue(CURRENT_PROFILE_ID, value); }
        }

        public string VersionInfo
        {
            get { return GetValueOrDefault<string>(VERSION_INFO_KEY, VERSION_INFO_DEFAULT); }
            private set 
            { 
                AddOrUpdateValue(VERSION_INFO_KEY, value);
                NotifyPropertyChangedAsync("VersionInfo");
            }
        }

        public bool SetToDefault
        {
            get { return GetValueOrDefault<bool>(SET_DEFAULT, SET_DEFAULTS_DEFAULT); }
            set
            {
                AddOrUpdateValue(SET_DEFAULT, value);
            }
        }

        public ForumGroup ForumGrouping
        {
            get
            {
                return (ForumGroup)GetValueOrDefault<int>(FORUM_GROUPING,
                    (int)FORUM_GROUPING_DEFAULT);
            }

            set
            {
                int group = (int)value;
                AddOrUpdateValue(FORUM_GROUPING, group);
                NotifyPropertyChangedAsync("ForumGrouping");
            }
        }

        public bool RunUnderLockScreen
        {
            get { return GetValueOrDefault<bool>(RUN_UNDER_LOCK, RUN_UNDER_LOCK_DEFAULT); }
            set
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        if (value == RunUnderLockScreen)
                        {
                            NotifyPropertyChangedAsync("RunUnderLockScreen");
                            return;
                        }

                        AddOrUpdateValue(RUN_UNDER_LOCK, value);
                        NotifyPropertyChangedAsync("RunUnderLockScreen");
                        
                    });
            }
        }

        public bool SmoothScrolling
        {
            get { return GetValueOrDefault<bool>(SMOOTH_SCROLL, SMOOTH_SCROLL_DEFAULT); }
            set
            {
                AddOrUpdateValue(SMOOTH_SCROLL, value);
                NotifyPropertyChangedAsync("SmoothScrolling");
            }
        }

        public ViewStyle ThreadViewStyle
        {
            get { return ViewStyle.Vertical; }
            set
            {
                AddOrUpdateValue(VIEWSTYLE, value);
                NotifyPropertyChangedAsync("ThreadViewStyle");
            }
        }

        public List<Cookie> CookieJar
        {
            get { return GetValueOrDefault<List<Cookie>>(COOKIE_JAR, new List<Cookie>()); }
            set
            {
                AddOrUpdateValue(COOKIE_JAR, value);
                NotifyPropertyChangedAsync("CookieJar");
            }
        }

        public bool LoggingEnabled
        {
            get { return GetValueOrDefault<bool>(LOGGING_ENABLED, LOGGING_ENABLED_DEFAULT); }
            set { AddOrUpdateValue(LOGGING_ENABLED, value); NotifyPropertyChangedAsync("LoggingEnabled"); }
        }

        public List<int> ForumFavorites
        {
            get
            {
                return GetValueOrDefault<List<int>>(FORUM_FAVORITES, new List<int>());
            }
            set
            {
                AddOrUpdateValue(FORUM_FAVORITES, value);
                NotifyPropertyChangedAsync(FORUM_FAVORITES);
            }
        }

        public double PostTextColorSliderValue
        {
            get
            {
                return GetValueOrDefault<double>(POST_TEXT_COLOR_SLIDER_VALUE, 
                    POST_TEXT_COLOR_SLIDER_VALUE_DEFAULT);
            }

            set
            {
                AddOrUpdateValue(POST_TEXT_COLOR_SLIDER_VALUE, value);
                SetPostForeground(value);
                NotifyPropertyChangedAsync(POST_TEXT_COLOR_SLIDER_VALUE);
            }
        }

        public ThreadData LastViewedThread
        {
            get
            {
                return GetValueOrDefault<SAThread>(LAST_VIEWED_THREAD, null);
            }
            set
            {
                AddOrUpdateValue(LAST_VIEWED_THREAD, value);
                NotifyPropertyChangedAsync(LAST_VIEWED_THREAD);
            }
        }

        public ForumData LastViewedForum
        {
            get
            {
                return GetValueOrDefault<SAForum>(LAST_VIEWED_FORUM, null);
            }
            set
            {
                AddOrUpdateValue(LAST_VIEWED_FORUM, value);
                NotifyPropertyChangedAsync(LAST_VIEWED_FORUM);
            }
        }

        public int MainMenuStartingIndex
        {
            get 
            { 
                int value = GetValueOrDefault<int>(HOME_PAGE, HOME_PAGE_DEFAULT);
                return value;
            }
            set 
            {
                AddOrUpdateValue(HOME_PAGE, value); NotifyPropertyChangedAsync("MainMenuStartingIndex"); 
            }
        }

        public int SwipeSensitivity
        {
            get { return GetValueOrDefault<int>(SWIPE_SENSITIVITY, SWIPE_SENSITIVITY_DEFAULT); }
            set { AddOrUpdateValue(SWIPE_SENSITIVITY, value); NotifyPropertyChangedAsync(SWIPE_SENSITIVITY); }
        }
        public int ThreadTimeout
        {
            get 
            {
                // want a really long timeout to step through functions without signal being set.
                if (LoggingEnabled || Debugger.IsAttached)
                    return Int32.MaxValue;

                return GetValueOrDefault<int>(THREAD_TIMEOUT, THREAD_TIMEOUT_DEFAULT); 
            }
            set { AddOrUpdateValue(THREAD_TIMEOUT, value); NotifyPropertyChangedAsync(THREAD_TIMEOUT); }
        }

        public string Username
        {
            get {return GetValueOrDefault<string>(USERNAME, USERNAME_DEFAULT); }
            set { AddOrUpdateValue(USERNAME, value); NotifyPropertyChangedAsync("Username"); }
        }
        public string Password
        {
            get { return GetValueOrDefault<string>(PASSWORD, PASSWORD_DEFAULT); }
            set { AddOrUpdateValue(PASSWORD, value); NotifyPropertyChangedAsync("Password"); }
        }
        public bool SaveAccount
        {
            get { return GetValueOrDefault<bool>(SAVE_ACCOUNT, SAVE_ACCOUNT_DEFAULT); }
            set { AddOrUpdateValue(SAVE_ACCOUNT, value); NotifyPropertyChangedAsync("SaveAccount"); }
        }

        public double PostImageHeight
        {
            get { return GetValueOrDefault(IMG_HEIGHT, IMG_HEIGHT_DEFAULT); }
            set { AddOrUpdateValue(IMG_HEIGHT, value); NotifyPropertyChangedAsync("PostImageHeight"); }
        }
        public double PostImageWidth
        {
            get { return GetValueOrDefault(IMG_WIDTH, IMG_WIDTH_DEFAULT); }
            set { AddOrUpdateValue(IMG_WIDTH, value); NotifyPropertyChangedAsync("PostImageWidth"); }
        }

        public int PostTextSize
        {
            get { return GetValueOrDefault<int>(POST_TEXT_SIZE, POST_TEXT_SIZE_DEFAULT); }
            set
            {
                AddOrUpdateValue(POST_TEXT_SIZE, value);
                NotifyPropertyChangedAsync("PostTextSize");
            }
        }

        #endregion

        public AwfulSettings()
        {
            try
            {
                settings = IsolatedStorageSettings.ApplicationSettings;
                if (VERSION_INFO != VersionInfo)
                {
                    HomePage.ShowAboutPageOnStartup = true;
                    this.RefreshHTML();
                    VersionInfo = VERSION_INFO;
                }
            }
            catch (Exception) { }
        }

        #region Members

        public void SetPostForeground(double value)
        {
            Color color = new Color();
            color.A = Convert.ToByte(255);
            color.R = Convert.ToByte(255 * value);
            color.G = Convert.ToByte(255 * value);
            color.B = Convert.ToByte(255 * value);

            Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    App.Layout.CurrentTheme.PostForeground = color;
                });
        }

        private void RefreshHTML()
        {
            KollaSoft.Extensions.CopyToIsoStore("style/main.css", "www", "awful.css");
            KollaSoft.Extensions.CopyToIsoStore("style/awful.js", "www", "awful.js");
            KollaSoft.Extensions.CopyToIsoStore("style/jquery.min.js", "www", "jquery.min.js");
        }

        private void LoadDefaults()
        {
            RefreshHTML();
            HomePage.ShowAboutPageOnStartup = true;
            this.CurrentProfileID = CURRENT_PROFILE_ID_DEFAULT;
            this.SmoothScrolling = SMOOTH_SCROLL_DEFAULT;
            this.SwipeSensitivity = SWIPE_SENSITIVITY_DEFAULT;
            this.ThreadViewStyle = VIEWSTYLE_DEFAULT;
            this.PostTextSize = POST_TEXT_SIZE_DEFAULT;
            this.PostTextColorSliderValue = POST_TEXT_COLOR_SLIDER_VALUE_DEFAULT;
            this.LoggingEnabled = LOGGING_ENABLED_DEFAULT;
            this.ForumGrouping = FORUM_GROUPING_DEFAULT;
            this.ThreadTimeout = THREAD_TIMEOUT_DEFAULT;
            this.RunUnderLockScreen = RUN_UNDER_LOCK_DEFAULT;
        }

        public override void LoadSettings()
        {
            ApplicationUsageHelper.Init(AwfulSettings.VERSION_INFO);

            // check database for updates
            DBUpdater.ManageDatabase();

            if (this.SetToDefault)
            {
                LoadDefaults();
                this.SetToDefault = false;
            }

            // set application detection mode to disabled: phone will not kill Awful if running in the foreground.
            PhoneApplicationService.Current.ApplicationIdleDetectionMode = this.RunUnderLockScreen ?
                IdleDetectionMode.Disabled :
                IdleDetectionMode.Enabled;

            
            if (!Awful.Core.Web.AwfulWebRequest.CanAuthenticate)
            {
                using (var db = new Data.SAForumDB())
                {
                    var query = db.Tokens.Where(t => t.ProfileID == CurrentProfileID);
                    var token = query.FirstOrDefault();
                    if (token != null)
                    {
                        var profile = token.Profile;
                        var cookies = profile.GetTokens();
                        Awful.Core.Web.AwfulWebRequest.SetCookieJar(cookies);
                        App.CurrentUser = profile.Username;
                    }
                }
            }
            

            this.SetPostForeground(this.PostTextColorSliderValue);
            Globals.Resources.HasSeen = App.Layout.CurrentTheme.PostHasSeen;
            Globals.Resources.PostForeground = App.Layout.CurrentTheme.PostForeground;
            Globals.Resources.Foreground = App.Layout.CurrentTheme.Foreground;
            Globals.Resources.Background = App.Layout.CurrentTheme.PostBackground;
        }

        public override void SaveSettings()
        {
            base.SaveSettings();
            using (var db = new Data.SAForumDB()) { db.SubmitChanges(); }
        }

        #endregion
    }
}
