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
using Awful.ViewModels;
using Microsoft.Phone.Shell;
using Awful.Helpers;
using Awful.Models;
using Telerik.Windows.Controls;
using Awful.Commands;
using KollaSoft;

namespace Awful
{
    public partial class ThreadView : PhoneApplicationPage
    {
        // app bar button and menu indexes
        private const int prevButton = 0;
        private const int nextButton = 1;
        private const int refreshButton = 2;
        private const int replyButton = 3;

        private const int goToPageMenu = 0;
        private const int rateThreadMenu = 1;
        private const int bookmarkThreadMenu = 2;
        private const int lockOrientationMenu = 3;

        private IApplicationBar _defaultAppBar;
        private VerticalThreadViewer ThreadPagePresenter;

        private bool _jumpToForums;
        private ThreadData _currentThread;
        private int _currentPageNumber;
        private int _lastPostIndex;
        private bool _orientationLocked;
        private PageOrientation _currentOrientation;
        private Cancellable _pageRequest;
        private ProgressIndicator _progress;
        private readonly AddBookmarkCommand _addBookmark = new AddBookmarkCommand();
        private readonly Uri _refreshUri = new Uri(Globals.Resources.RefreshUri, UriKind.RelativeOrAbsolute);
        private readonly Uri _cancelUri = new Uri(Globals.Resources.CloseUri, UriKind.RelativeOrAbsolute);
        private readonly ThreadViewerViewModel _context = new ThreadViewerViewModel();

        public static event EventHandler<ValueEventArgs<SAThread>> ThreadSelected;

        public ThreadView()
        {
            InitializeComponent();
            BindEvents();
            this.SupportedOrientations = SupportedPageOrientation.PortraitOrLandscape;
            this._currentOrientation = this.Orientation;
            this._defaultAppBar = this.ApplicationBar;
        }

        private void BindEvents()
        {
            App.Busy += new EventHandler(App_Busy);
            App.Ready += new EventHandler(App_Ready);

            this._context.PageLoading += new EventHandler(Context_PageLoading);
            this._context.PageLoaded += new EventHandler<PageLoadedEventArgs>(Context_PageLoaded);
            
            this.ThreadNavigator.NavigationCancelled += new EventHandler<EventArgs>(ThreadNavigator_NavigationCancelled);
            this.ThreadNavigator.NavigationRequested += new EventHandler<ThreadNavigationArgs>(ThreadNavigator_NavigationRequested);
            this.Loaded += new RoutedEventHandler(ThreadView_Loaded);
            this.Unloaded += new RoutedEventHandler(ThreadView_Unloaded);
            this.OrientationChanged += new EventHandler<OrientationChangedEventArgs>(ThreadView_OrientationChanged);
        }

        void ReplyWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.UpdateReplyBoxMargin(this.Orientation);
        }

        void ReplyBox_SmileyWindowRequested(object sender, EventArgs e)
        {
            if (this.ThreadPagePresenter.ReplyWindow.IsOpen)
            {
                EventHandler<WindowClosedEventArgs> handler = null;
                handler = (obj, args) =>
                    {
                        this.ThreadPagePresenter.ReplyBox.SmileyWindow.WindowClosed -= new EventHandler<WindowClosedEventArgs>(handler);
                        this.ThreadPagePresenter.ReplyWindow.IsOpen = true;
                    };

                this.ThreadPagePresenter.ReplyBox.SmileyWindow.WindowClosed += new EventHandler<WindowClosedEventArgs>(handler);
                this.ThreadPagePresenter.ReplyWindow.IsOpenThenInvoke(false, () => 
                {
                    if (SystemTray.IsVisible)
                    {
                        SystemTray.BackgroundColor = (Color)App.Current.Resources["PhoneChromeColor"];
                    }

                    this.ThreadPagePresenter.ReplyBox.SmileyWindow.IsOpen = true;
                });
            }


        }

        void ReplyWindow_WindowOpened(object sender, EventArgs e)
        {
            var oldOrientation = this.IsOrientationLocked;
            EventHandler<WindowClosedEventArgs> handler = null;
            handler = (obj, args) =>
            {
                var window = obj as RadWindow;
                if (window != null)
                {
                    window.WindowClosed -= new EventHandler<WindowClosedEventArgs>(handler);
                    if (SystemTray.IsVisible)
                    {
                        SystemTray.BackgroundColor = (Color)App.Current.Resources["PhoneBackgroundColor"];
                    }
                }

                this.IsOrientationLocked = oldOrientation;
            };

            this.ThreadPagePresenter.ReplyWindow.WindowClosed += new EventHandler<WindowClosedEventArgs>(handler);
            this.IsOrientationLocked = false;

            if (SystemTray.IsVisible)
            {
                SystemTray.BackgroundColor = (Color) App.Current.Resources["PhoneChromeColor"];
            }
        }

        private void ThreadPagePresenter_NavigationRequested(object sender, ThreadNavigationArgs e)
        {
            this._currentThread = e.Thread;
            LoadPage(e.Thread, e.Thread.LastViewedPageIndex, e.Thread.LastViewedPostIndex);
        }

        void ThreadView_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        private bool IsOrientationLocked
        {
            get { return this._orientationLocked; }
            set
            {
                this._orientationLocked = value;

                if (value)
                {
                    if (Orientation.IsPortrait())
                        SupportedOrientations = SupportedPageOrientation.Portrait;
                    else
                        SupportedOrientations = SupportedPageOrientation.Landscape;
                }

                else
                {
                    SupportedOrientations = SupportedPageOrientation.PortraitOrLandscape;
                }
            }
        }

        private void ThreadView_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            if (_orientationLocked)
                base.OnOrientationChanged(e);
        }

        private void HandleOrientation()
        {
            var menu = this._defaultAppBar.MenuItems[lockOrientationMenu] as IApplicationBarMenuItem;

            this.IsOrientationLocked = !this.IsOrientationLocked;

            if (this.IsOrientationLocked)
            {
                if (menu != null) { menu.Text = "unlock view"; }
            }

            else
            {
                if (menu != null) { menu.Text = "lock view"; }
            }
        }

        private void App_Ready(object sender, EventArgs e)
        {
            /*
            if (_progress != null)
            {
                _progress.IsIndeterminate = false;
                _progress.IsVisible = false;
            }
             */

            this.busyIndicator.IsRunning = false;
        }

        private void App_Busy(object sender, EventArgs e)
        {
            /*
            if (_progress == null)
            {
                _progress = new ProgressIndicator();
                SystemTray.ProgressIndicator = _progress;
            }

            _progress.IsIndeterminate = true;
            _progress.IsVisible = true;
            */

            this.busyIndicator.IsRunning = true;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            SystemTray.IsVisible = !App.Settings.HideSystemTray;

            
            this._currentPageNumber = 0;
            this._lastPostIndex = -1;

            ThreadData thread = null;
            try { thread = PhoneApplicationService.Current.State["Thread"] as ThreadData; }
            catch (Exception)
            {
                Awful.Core.Event.Logger.AddEntry("ThreadView: Thread no longer in state memory. Pulling from storage...");

                // do something if the state is empty //
                thread = App.Settings.LastViewedThread;

                if (thread == null)
                {

                    MessageBox.Show("An error occured while trying to view this thread. Hit ok to navigate back.",
                        ":(",
                        MessageBoxButton.OK);

                    NavigationService.GoBack();
                    return;
                }
            }

            this._currentThread = thread;
            this.ThreadNavigator.Thread = thread;

            if (!e.IsNavigationInitiator)
            {
                if (thread != null)
                {
                    this._currentPageNumber = thread.LastViewedPageIndex;
                    this._lastPostIndex = thread.LastViewedPostIndex;
                }
            }

            else if (NavigationContext.QueryString.ContainsKey("Page"))
            {
                int page = 0;

                if (Int32.TryParse(NavigationContext.QueryString["Page"], out page))
                    _currentPageNumber = page;
            }

            base.OnNavigatedTo(e);
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (this.ThreadNavigatorContainer.IsOpen) { base.OnBackKeyPress(e); }

                else if (this.NavigationService.CanGoBack == false)
                {
                    e.Cancel = true;
                    this._jumpToForums = true;
                    this.NavigationService.Navigate(new Uri("/HomePage.xaml", UriKind.RelativeOrAbsolute));
                }

                else
                {
                    base.OnBackKeyPress(e);
                }
            }

            catch (NullReferenceException ex)
            {
                Awful.Core.Event.Logger.AddEntry(string.Format("An error occured while navigating back from Thread View."));
            }
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (this._jumpToForums)
            {
                NavigationService.RemoveBackEntry();
                this._jumpToForums = false;
            }

            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            this.busyIndicator.IsRunning = false;

            if (e.IsNavigationInitiator == false)
            {
                _currentThread.LastViewedPageIndex = _currentPageNumber;
                _currentThread.LastViewedPostIndex = _context.PostIndex;
                App.Settings.LastViewedThread = _currentThread;
            }
            else
            {
                // we aren't exiting the app, so clear the last viewed thread
                App.Settings.LastViewedThread = null;
            }

            if (this._context != null)
            {
                this._context.CurrentUserID = 0;
                this.ThreadPagePresenter.PostMenu.AuthorFilterEnabled = false;
            }

            base.OnNavigatingFrom(e);
        }

        private void ThreadView_Loaded(object sender, RoutedEventArgs e)
        {
            var menu = ApplicationBar.MenuItems[lockOrientationMenu] as IApplicationBarMenuItem;
            HandleOrientation();
            //LoadPage(this._currentThread, this._currentPageNumber, this._lastPostIndex);
        }

        private void ThreadNavigator_NavigationRequested(object sender, ThreadNavigationArgs e)
        {
            this.ThreadNavigatorContainer.IsOpenThenInvoke(false, () => { LoadPage(e.Thread, e.Page); });
        }

        private void ThreadNavigator_NavigationCancelled(object sender, EventArgs e)
        {
            this.ThreadNavigatorContainer.IsOpen = false;
        }

        private void Context_PageLoaded(object sender, PageLoadedEventArgs e)
        {
            this.threadViewCanvas.IsHitTestVisible = true;
            this.threadViewCanvas.StopPullToRefreshLoading(true);

            if (this._defaultAppBar != null)
            {
                IApplicationBarIconButton reply = this._defaultAppBar.Buttons[replyButton] as IApplicationBarIconButton;
                reply.IsEnabled = true;

                IApplicationBarIconButton refresh = this._defaultAppBar.Buttons[refreshButton] as IApplicationBarIconButton;
                refresh.Text = "refresh";
                refresh.IconUri = this._refreshUri;

                IApplicationBarIconButton next = this._defaultAppBar.Buttons[nextButton] as IApplicationBarIconButton;
                if (e.PageNumber == this._currentThread.MaxPages) { next.IsEnabled = false; }
                else { next.IsEnabled = true; }

                IApplicationBarIconButton prev = this._defaultAppBar.Buttons[prevButton] as IApplicationBarIconButton;
                if (e.PageNumber == 1) { prev.IsEnabled = false; }
                else { prev.IsEnabled = true; }
            }

            if (e.Thread == null) { MessageBox.Show("Could not load the requested page. Check your connection settings and try again.", ":(", MessageBoxButton.OK); }
            else
            {
                (e.Thread as SAThread).HasViewedToday = true;
                this._currentThread = e.Thread;
                ThreadSelected.Fire(this, new ValueEventArgs<SAThread>(this._currentThread as SAThread));
            }

            this._currentPageNumber = e.PageNumber;
            this._lastPostIndex = this._context.PostIndex;
        }

        private void Context_PageLoading(object sender, EventArgs e)
        {
            this.threadViewCanvas.IsHitTestVisible = false;

            if (ThreadPagePresenter.ShowReplyWindow == true)
            {
                ThreadPagePresenter.ReplyWindow.IsOpenThenInvoke(false, () => { RefreshAppBar(); });
            }

            else
                RefreshAppBar();
        }

        private void RefreshAppBar()
        {
            var refresh = this._defaultAppBar.Buttons[refreshButton] as IApplicationBarIconButton;
            var reply = this._defaultAppBar.Buttons[replyButton] as IApplicationBarIconButton;

            refresh.IconUri = _cancelUri;
            refresh.Text = "cancel";
            reply.IsEnabled = false;
        }

        private void LoadPage(ThreadData thread, int pageNumber = -1, int postNumber = -1)
        {
            if (this._context.IsPageLoading) return;

            if (pageNumber < -1) throw new ArgumentException("ThreadView: PageNumber cannot be less than -1.");

            // if pageNumber is not assigned, then we are refreshing
            if (pageNumber == -1)
            {
                // reload only if we aren't reloading already
                if (this._context.IsPageLoading == false)
                {
                    ThreadPagePresenter.ShrinkAndDropContent.BeginThenInvoke(() =>
                    {
                        this._context.ReloadCurrentPageAsync(thread, postNumber);
                    });
                }
                // if we are reloading, then we'll cancel
                else { this._context.CancelAsync(); }
            }

            else
            {
                // load up next page
                ThreadPagePresenter.ShrinkAndDropContent.BeginThenInvoke(() =>
                {
                    this._context.LoadThreadPageAsync(thread, pageNumber, postNumber);
                });
            }
        }

        private void AppBarButtonClick(object sender, System.EventArgs e)
        {
            var button = sender as IApplicationBarIconButton;
            switch (ApplicationBar.Buttons.IndexOf(button))
            {
                case prevButton:
                    this.GoToPrevPage();
                    break;

                case nextButton:
                    this.GoToNextPage();
                    break;

                case refreshButton:
                    if (this._context.IsPageLoading) { this._context.CancelAsync(); }
                    else { LoadPage(this._currentThread); }
                    break;

                case replyButton:
                    ThreadPagePresenter.ShowReplyWindow = true;
                    break;
            }
        }

        private void GoToPrevPage()
        {
            int prevIndex = this._currentPageNumber - 1;
            if (prevIndex <= 0) return;
            this.LoadPage(this._currentThread, prevIndex);
        }

        private void GoToNextPage()
        {
            int nextIndex = this._currentPageNumber + 1;
            if (nextIndex > this._currentThread.MaxPages) return;
            this.LoadPage(this._currentThread, nextIndex);
        }

        private void AppBarMenuClick(object sender, System.EventArgs e)
        {
            var menu = sender as IApplicationBarMenuItem;
            switch (ApplicationBar.MenuItems.IndexOf(menu))
            {
                case goToPageMenu:
                    this.ThreadNavigatorContainer.IsOpen = true;
                    break;

                case rateThreadMenu:
                    this.ThreadPagePresenter.ShowRatingsWindow = true;
                    break;

                case bookmarkThreadMenu:
                    this._addBookmark.Execute(_currentThread);
                    break;

                case lockOrientationMenu:
                    HandleOrientation();
                    break;
            }
        }

        private const string MARGIN_UP = "LayoutMargin_PortraitUp";
        private const string MARGIN_DOWN = "LayoutMargin_PortraitDown";
        private const string MARGIN_LEFT = "LayoutMargin_LandscapeLeft";
        private const string MARGIN_RIGHT = "LayoutMargin_LandscapeRight";

        private void Page_OrientationChanged(object sender, Microsoft.Phone.Controls.OrientationChangedEventArgs e)
        {
            var orientation = e.Orientation;
            this.UpdateReplyBoxMargin(orientation);
        }

        private void UpdateReplyBoxMargin(PageOrientation orientation)
        {
            switch (orientation)
            {
                case PageOrientation.LandscapeLeft:
                    this.ThreadPagePresenter.ReplyBox.Margin = (Thickness)this.Resources[MARGIN_LEFT];
                    break;

                case PageOrientation.LandscapeRight:
                    this.ThreadPagePresenter.ReplyBox.Margin = (Thickness)this.Resources[MARGIN_RIGHT];
                    break;

                case PageOrientation.PortraitUp:
                    this.ThreadPagePresenter.ReplyBox.Margin = (Thickness)this.Resources[MARGIN_UP];
                    break;

                case PageOrientation.PortraitDown:
                    this.ThreadPagePresenter.ReplyBox.Margin = (Thickness)this.Resources[MARGIN_DOWN];
                    break;
            }
        }

        private void threadViewCanvas_RefreshRequested(object sender, EventArgs e)
        {
            this.LoadPage(this._currentThread);
        }

        private void ThreadPagePresenter_Loaded(object sender, RoutedEventArgs e)
        {
            var element = sender as VerticalThreadViewer;

            if (element != null)
            {
                this.ThreadPagePresenter = element;
                this.ThreadPagePresenter.Width = this.threadViewCanvas.ActualWidth;
                this.ThreadPagePresenter.Height = this.threadViewCanvas.ActualHeight;
                this.ThreadPagePresenter.NavigationRequested +=
                    new EventHandler<ThreadNavigationArgs>(ThreadPagePresenter_NavigationRequested);
                this.ThreadPagePresenter.ReplyWindow.Loaded += new RoutedEventHandler(ReplyWindow_Loaded);
                this.ThreadPagePresenter.ReplyWindow.WindowOpened += new EventHandler<EventArgs>(ReplyWindow_WindowOpened);
                this.ThreadPagePresenter.ReplyBox.SmileyWindowRequested += new EventHandler(ReplyBox_SmileyWindowRequested);
                this.ThreadPagePresenter.SetViewModel(this._context);
                this.LoadPage(this._currentThread, this._currentPageNumber, this._lastPostIndex);
            }
        }

        private void ThreadViewCanvas_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
        	if (this.ThreadPagePresenter != null)
			{
				this.ThreadPagePresenter.Width = this.threadViewCanvas.ActualWidth;
				this.ThreadPagePresenter.Height = this.threadViewCanvas.ActualHeight;
			}
        }

        private void GestureListener_Flick(object sender, FlickGestureEventArgs e)
        {
            bool valid = !this._context.IsPageLoading;
            if (valid)
            {
                var settings = new AwfulSettings();
                valid = e.Direction == System.Windows.Controls.Orientation.Horizontal;
                valid = valid && Math.Abs(e.HorizontalVelocity) > settings.PageSwipeThreshold;
                bool validLeft = valid && e.HorizontalVelocity < 0;
                bool validRight = valid;
                if (validLeft) { this.GoToNextPage(); }
                else if (validRight) { this.GoToPrevPage(); }
            }
        }
    }
}
