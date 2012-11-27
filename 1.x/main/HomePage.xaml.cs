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
using Telerik.Windows.Controls;
using Awful.Models;

namespace Awful
{
    public partial class HomePage : PhoneApplicationPage
    {
		public static bool ShowAboutPageOnStartup = false;
		
        private static ViewModels.HomePageViewModel _context;
        public static ViewModels.HomePageViewModel ViewModel { get { return HomePage._context; } }

        private readonly IApplicationBar _defaultBar;
        private readonly Commands.ViewForumCommand _viewForumCommand = new Commands.ViewForumCommand();
        private readonly Commands.ViewThreadCommand _viewThreadCommand = new Commands.ViewThreadCommand();

        private Menus.BookmarksContextMenu _bookmarksMenu;
        private Menus.FavoritesContextMenu _favsMenu;

        private const int REFRESH_BUTTON_INDEX = 0;
        private const int SETTINGS_BUTTON_INDEX = 1;

        private const int THREAD_NAVI_MENU_INDEX = 0;
        private const int FORUM_NAVI_MENU_INDEX = 1;
        private const int LOGOUT_MENU_INDEX = 2;

        private const int FORUMS_INDEX = 0;
        private const int FAVORITES_INDEX = 1;
        private const int BOOKMARKS_INDEX = 2;

        private const string URL_TAG_OK = "URLOK";
        private const string URL_TAG_CANCEL = "URLCANCEL";
		private const string FORUM_NAV_OK = "FORUMNAVOK";
		private const string FORUM_NAV_CANCEL = "FORUMNAVCANCEL";

        private RadContextMenu BookmarkMenu
        {
            get
            {
                if (this._bookmarksMenu == null)
                    this._bookmarksMenu = new Menus.BookmarksContextMenu();

                return this._bookmarksMenu;
            }
        }

        private RadContextMenu FavsMenu
        {
            get
            {
                if (this._favsMenu == null)
                    this._favsMenu = new Menus.FavoritesContextMenu();

                return this._favsMenu;
            }
        }

        public HomePage()
        {
            InitializeComponent();
            PrepareAnimations();
            InteractionEffectManager.AllowedTypes.Add(typeof(RadDataBoundListBoxItem));

            this.ForumsList.IsSynchronizedWithCurrentItem = false;
            this.BookmarksList.IsSynchronizedWithCurrentItem = false;
            this.FavoritesListBox.IsSynchronizedWithCurrentItem = false;
            this.FavoritesListBox.ItemAnimationMode = ItemAnimationMode.PlayOnNewSource;

            HomePage._context = new ViewModels.HomePageViewModel(this.ForumsList);
            this._defaultBar = this.ApplicationBar;

            BindEvents();
        }

        private void BindEvents()
        {
            App.Busy += new EventHandler(App_Busy);
            App.Ready += new EventHandler(App_Ready);
			this.Loaded += new System.Windows.RoutedEventHandler(HomePage_Loaded);
            this.MainMenu.SelectionChanged += new SelectionChangedEventHandler(MainMenu_SelectionChanged);
            this.ThreadNavigator.NavigationRequested += new EventHandler<ThreadNavigationArgs>(ThreadNavigator_NavigationRequested);
            this.ThreadNavigator.NavigationCancelled += new EventHandler<EventArgs>(ThreadNavigator_NavigationCancelled);
        }

        void App_Ready(object sender, EventArgs e)
        {
            this.busyIndicator.IsRunning = false;
        }

        void App_Busy(object sender, EventArgs e)
        {
            this.busyIndicator.IsRunning = true;
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            this.busyIndicator.IsRunning = false;
            base.OnNavigatingFrom(e);
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            HomePage.ViewModel.Loading -= new EventHandler(this.ContextLoading);
            HomePage.ViewModel.Loaded -= new EventHandler(this.ContextLoaded);

            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            SystemTray.IsVisible = !App.Settings.HideSystemTray;

            this.FavoritesListBox.SelectedItem = null;
            this.ForumsList.SelectedItem = null;
            this.BookmarksList.SelectedItem = null;

            HomePage.ViewModel.Loading += new EventHandler(this.ContextLoading);
            HomePage.ViewModel.Loaded += new EventHandler(this.ContextLoaded);

            if (this.DataContext == null)
                this.DataContext = HomePage.ViewModel;

            if (e.NavigationMode == System.Windows.Navigation.NavigationMode.New)
                HomePage.ViewModel.CurrentSectionIndex = App.Settings.MainMenuStartingIndex;

            base.OnNavigatedTo(e);
        }

        void ThreadNavigator_NavigationCancelled(object sender, EventArgs e)
        {
            this.ThreadNavigatorWindow.IsOpen = false;
        }

        void ThreadNavigator_NavigationRequested(object sender, ThreadNavigationArgs e)
        {
            this.ThreadNavigatorWindow.IsOpenThenInvoke(false, () =>
                {
                    this.NavigateToThread(e.Thread, e.Page);
                });
        }

        private void MainMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (this.MainMenu.SelectedIndex)
            {
                case FORUMS_INDEX:
                    RadContextMenu.SetFocusedElementType(this.ForumsList, typeof(RadDataBoundListBoxItem));
                    //App.MenuProvider.Menu = this.FavsMenu;
                    break;

                case FAVORITES_INDEX:
                    RadContextMenu.SetFocusedElementType(this.FavoritesListBox, typeof(RadDataBoundListBoxItem));
                    //App.MenuProvider.Menu = this.FavsMenu;
                    break;

                case BOOKMARKS_INDEX:
                    RadContextMenu.SetFocusedElementType(this.BookmarksList, typeof(RadDataBoundListBoxItem));
                    //App.MenuProvider.Menu = this.BookmarkMenu;
                    break;
            }

            UpdateRefresh();
        }

        private void UpdateRefresh()
        {
            bool? refresh = false;
            string type = string.Empty;
            switch (this.MainMenu.SelectedIndex)
            {
                case FORUMS_INDEX:
                    refresh = HomePage.ViewModel.IsForumsLoading;
                    this.ForumsList.StopPullToRefreshLoading(true);
                    type = "forums";
                    break;

                case FAVORITES_INDEX:
                    refresh = null;
                    break;

                case BOOKMARKS_INDEX:
                    refresh = HomePage.ViewModel.IsBookmarksLoading;
                    this.BookmarksList.StopPullToRefreshLoading(true);
                    type = "bookmarks";
                    break;
            }

            if (this._defaultBar != null)
            {
                IApplicationBarIconButton refreshItems = this._defaultBar.Buttons[REFRESH_BUTTON_INDEX] as IApplicationBarIconButton;

                if (refresh.HasValue)
                {
                    refreshItems.IsEnabled = true;

                    if (refresh.Value == true)
                    {
                        refreshItems.Text = "cancel";
                        refreshItems.IconUri = new Uri(Globals.Resources.CloseUri, UriKind.RelativeOrAbsolute);
                    }


                    else
                    {
                        refreshItems.Text = "refresh";
                        refreshItems.IconUri = new Uri(Globals.Resources.RefreshUri, UriKind.RelativeOrAbsolute);
                    }
                }

                else
                {
                    refreshItems.Text = "refresh";
                    refreshItems.IconUri = new Uri(Globals.Resources.RefreshUri, UriKind.RelativeOrAbsolute);
                    refreshItems.IsEnabled = false;
                }
            }
        }

        private void ContextLoading(object sender, EventArgs e)
        {
            UpdateRefresh();
        }

        private void ContextLoaded(object sender, EventArgs e)
        {
            UpdateRefresh();
        }

        private void AppBarClick(object sender, System.EventArgs e)
        {
            var item = sender as IApplicationBarMenuItem;
            if (item == null) return;

            int index = this._defaultBar.MenuItems.IndexOf(item);
            switch (index)
            {
                case FORUM_NAVI_MENU_INDEX:
                    this.ForumIDNavigator.IsOpen = true;
                    break;

                case THREAD_NAVI_MENU_INDEX:
                    this.ThreadUrlNavigator.IsOpen = true;
                    break;

                case LOGOUT_MENU_INDEX:
                    var selection = MessageBox.Show("Are you sure you want to logout?", ":o", MessageBoxButton.OKCancel);
                    if (selection == MessageBoxResult.OK)
                    {
                        App.Settings.CurrentProfileID = -1;
                        MessageBox.Show("You will be asked to log in again the next time you restart Awful.", ":)", MessageBoxButton.OK);
                    }
                    break;
            }
        }

        private void PrepareAnimations()
        {
            InteractionEffectManager.AllowedTypes.Add(typeof(RadDataBoundListBoxItem));

            Func<RadMoveAndFadeAnimation> createAddAnimation = () =>
            {
                var duration = new TimeSpan(0, 0, 0, 0, 500);
                var moveAndFade = new RadMoveAndFadeAnimation();
                moveAndFade.MoveAnimation.StartPoint = new Point(0, -90);
                moveAndFade.MoveAnimation.EndPoint = new Point(0, 0);
                moveAndFade.FadeAnimation.StartOpacity = 0;
                moveAndFade.FadeAnimation.EndOpacity = 1;
                moveAndFade.Easing = new CubicEase();
                moveAndFade.Duration = new Duration(duration);
                return moveAndFade;
            };

            FavoritesListBox.ItemAddedAnimation = createAddAnimation();
            BookmarksList.ItemAddedAnimation = createAddAnimation();
            ForumsList.ItemAddedAnimation = createAddAnimation();

            Func<RadMoveAndFadeAnimation> createRemoveAnimation = () =>
            {
                var duration = new TimeSpan(0, 0, 0, 0, 500);
                var moveAndFade = new RadMoveAndFadeAnimation();
                moveAndFade.MoveAnimation.StartPoint = new Point(0, 0);
                moveAndFade.MoveAnimation.EndPoint = new Point(0, -90);
                moveAndFade.FadeAnimation.StartOpacity = 1;
                moveAndFade.FadeAnimation.EndOpacity = 0;
                moveAndFade.Easing = new CubicEase() { EasingMode = EasingMode.EaseOut };
                moveAndFade.Duration = new Duration(duration);
                return moveAndFade;
            };

            BookmarksList.ItemRemovedAnimation = createRemoveAnimation();
            ForumsList.ItemRemovedAnimation = createRemoveAnimation();
        }

        private void ThreadSelected(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (BookmarksList.SelectedItem != null)
            {
                Models.SAThread thread = BookmarksList.SelectedItem as Models.SAThread;
                NavigateToThread(thread);
                this.BookmarksList.SelectedItem = null;
            }
        }

        private void ForumSelected(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;
            
            else
            {
                var forum = e.AddedItems[0] as ForumData;
                if (forum == null)
                    return;

                int id = forum.ID;

                PhoneApplicationService.Current.State["Forum"] = forum;
                NavigationService.Navigate(new Uri("/ThreadList.xaml?ID=" + id, UriKind.RelativeOrAbsolute));
            }
        }

        private void NavigateToThread(ThreadData thread, int page = 0)
        {
            PhoneApplicationService.Current.State["Thread"] = thread;
            var saThread = thread as SAThread;
            if (saThread.ThreadSeen == false)
                page = 1;

            var uri = "/ThreadView.xaml";
            if (page > 0)
            {
                uri = uri + "?Page=" + page;
            }
            NavigationService.Navigate(new Uri(uri, UriKind.RelativeOrAbsolute));
        }

        private void NavigatorButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var button = sender as Button;
            switch (button.Tag.ToString())
            {
                case URL_TAG_OK:
                    ThreadUrlNavigator.IsOpenThenInvoke(false, () => 
					{ 
						this._viewThreadCommand.Url = this.UrlTextBox.Text.Trim();
                        this._viewThreadCommand.ThreadID = this.UrlTextBox.Text.Trim();
						this.UrlTextBox.Text = string.Empty;
						this._viewThreadCommand.Execute(Commands.ViewThreadCommand.USE_URL_OR_THREADID);
					});
					break;

                case URL_TAG_CANCEL:
                    this.UrlTextBox.Text = string.Empty;
                    ThreadUrlNavigator.IsOpen = false;
                    break;
					
				case FORUM_NAV_OK:
                    if (this._viewForumCommand.CanExecute(this.ForumTextBox.Text))
                    {
                        ForumIDNavigator.IsOpenThenInvoke(false, () =>
                            {
                                this._viewForumCommand.Execute(this.ForumTextBox.Text);
                                this.ForumTextBox.Text = string.Empty;
                            });
                    }
					break;
					
				case FORUM_NAV_CANCEL:
                    this.ForumTextBox.Text = string.Empty;
                    this.ForumIDNavigator.IsOpen = false;
					break;
            }
        }

        private void Bookmarks_JumpToPage(object sender, 
			Telerik.Windows.Controls.ContextMenuItemSelectedEventArgs e)
        {
            var menu = sender as Menus.BookmarksContextMenu;
            var thread = menu.SelectedThread;
            if (thread == null) { return; }
            else
            {
                this.ThreadNavigator.Thread = thread;
                this.ThreadNavigatorWindow.IsOpen = true;
            }
        }

        private void Forums_FavoritesChanged(object sender, EventArgs e)
        {
            HomePage.ViewModel.RefreshFavorites();
        }

        private void AppBarButtonClick(object sender, System.EventArgs e)
        {
            var item = sender as IApplicationBarIconButton;
            if (item == null) return;

            int index = this._defaultBar.Buttons.IndexOf(item);
            switch (index)
            {
                case REFRESH_BUTTON_INDEX:
                    HomePage.ViewModel.RefreshAsync();
                    break;

                case SETTINGS_BUTTON_INDEX:
                    NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.RelativeOrAbsolute));
                    break;
            }
        }

        private void AboutPanel_AppThreadRequested(object sender, System.EventArgs e)
        {
			AboutWindow.IsOpenThenInvoke(false, () =>
            {
                var navigate = new Commands.ViewThreadCommand();
                navigate.Url = "http://forums.somethingawful.com/showthread.php?threadid=3460814";
                navigate.Execute(Commands.ViewThreadCommand.USE_URL);
            });
        }

        private void AboutPanel_EmailAuthorRequested(object sender, System.EventArgs e)
        {
        	AboutPanel.IsNavigating = false;
            AboutWindow.IsOpenThenInvoke(false, () =>
            {
                var email = new Commands.EmailCommand();
                email.Subject = "Awful QCS";
                email.To = Globals.Constants.AUTHOR_EMAIL;
                email.Execute(null);
            });
        }

        private void HomePage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
        	if (HomePage.ShowAboutPageOnStartup)
			{
				this.AboutWindow.IsOpen = true;
				HomePage.ShowAboutPageOnStartup = false;
			}
        }

        private void EmptyBookmarks_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            HomePage.ViewModel.RefreshAsync();
        }

        private void EmptyForumsList_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            HomePage.ViewModel.RefreshAsync();
        }

        private void ForumsList_RefreshRequested(object sender, EventArgs e)
        {
            HomePage.ViewModel.RefreshAsync();
        }

        private void BookmarksList_RefreshRequested(object sender, EventArgs e)
        {
            HomePage.ViewModel.RefreshAsync();
        }
    }
}
