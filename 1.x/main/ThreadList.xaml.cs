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
using Awful.Helpers;
using Awful.Services;
using Telerik.Windows.Controls;
using KollaSoft;

namespace Awful
{
    public partial class ThreadList : PhoneApplicationPage
    {
        private ViewModels.ThreadsViewModel context;
        private ProgressIndicator progressIndicator;
        private readonly Commands.ToggleFavoritesCommand _toggleFavorites = new Commands.ToggleFavoritesCommand();
        private readonly Data.SAForumDB _dbContext = new Data.SAForumDB();
        private bool reload;

        private const int ADD_REMOVE_FAVORITES_MENU_INDEX = 0;
        private const int NEW_THREAD_MENU_INDEX = 1;
        private const int PREVBUTTON_INDEX = 0;
        private const int NEXTBUTTON_INDEX = 1;
        private const int RELOADBUTTON_INDEX = 2;

        private ForumData _currentForum;
        private readonly Uri _cancelUri = new Uri(Globals.Resources.CloseUri, UriKind.RelativeOrAbsolute);
        private readonly Uri _reloadUri = new Uri(Globals.Resources.RefreshUri, UriKind.RelativeOrAbsolute);

        private IApplicationBar _defaultAppBar;

        private ProgressIndicator _progress;
        private RadDataBoundListBox _reloadingListBox;

        private Storyboard showNav;
        private bool navVisible;

        public ThreadList()
        {
            InitializeComponent();
            PrepareAnimations();
            BindEvents();

            this.ThreadsList.IsSynchronizedWithCurrentItem = false;
            this.NewThreadsList.IsSynchronizedWithCurrentItem = false;
            this.ReadThreadsList.IsSynchronizedWithCurrentItem = false;
            this.MiscFilterList.IsSynchronizedWithCurrentItem = false;

            InteractionEffectManager.AllowedTypes.Add(typeof(RadDataBoundListBoxItem));

            this._defaultAppBar = this.ApplicationBar;

            if (App.IsBusy) { OnPageLoading(this, EventArgs.Empty); }

            GatherStoryboardEvents();
        
            this.context = new ViewModels.ThreadsViewModel();
            this.reload = true;
        }

        private void BindEvents()
        {
            App.Busy += new EventHandler(App_Busy);
            App.Ready += new EventHandler(App_Ready);
            this.Loaded += new RoutedEventHandler(ThreadList_Loaded);
            this.Unloaded += new RoutedEventHandler(ThreadList_Unloaded);
            ThreadNavigator.NavigationRequested += new EventHandler<ThreadNavigationArgs>(ThreadNavigator_NavigationRequested);
        }

        void ThreadList_Unloaded(object sender, RoutedEventArgs e)
        {
            this.context.PageLoading -= new EventHandler(OnPageLoading);
            this.context.PageLoaded -= new EventHandler(OnPageLoaded);
        }

        private void PrepareAnimations()
        {
            var duration = new TimeSpan(0, 0, 0, 0, 500);

            var setAddAnimation = new Action<RadVirtualizingDataControl>(listBox =>
                {       
                    var moveAndFade = new RadMoveAndFadeAnimation();
                    moveAndFade.MoveAnimation.StartPoint = new Point(0, -90);
                    moveAndFade.MoveAnimation.EndPoint = new Point(0, 0);
                    moveAndFade.FadeAnimation.StartOpacity = 0;
                    moveAndFade.FadeAnimation.EndOpacity = 1;
                    moveAndFade.Easing = new CubicEase();
                    moveAndFade.Duration = new Duration(duration);

                    listBox.ItemAddedAnimation = moveAndFade;
                });

            var setRemoveAnimation = new Action<RadVirtualizingDataControl>(listBox =>
                {
                    var moveAndFade = new RadMoveAndFadeAnimation();
                    moveAndFade.MoveAnimation.StartPoint = new Point(0, 0);
                    moveAndFade.MoveAnimation.EndPoint = new Point(0, -90);
                    moveAndFade.FadeAnimation.StartOpacity = 1;
                    moveAndFade.FadeAnimation.EndOpacity = 0;
                    moveAndFade.Easing = new CubicEase() { EasingMode = EasingMode.EaseOut };
                    moveAndFade.Duration = new Duration(duration);

                    listBox.ItemRemovedAnimation = moveAndFade;
                });

            setAddAnimation(this.ThreadsList);
            setRemoveAnimation(this.ThreadsList);

            setAddAnimation(this.NewThreadsList);
            setRemoveAnimation(this.NewThreadsList);

            setAddAnimation(this.ReadThreadsList);
            setRemoveAnimation(this.ReadThreadsList);

            setAddAnimation(this.MiscFilterList);
            setRemoveAnimation(this.MiscFilterList);   
        }

        private void App_Ready(object sender, EventArgs e)
        {
            //OnPageLoaded(sender, e);
            
           if (progressIndicator != null)
            {
                progressIndicator.IsIndeterminate = false;
            }
        }

        private void App_Busy(object sender, EventArgs e)
        {
            if (null == progressIndicator)
            {
                progressIndicator = new ProgressIndicator();
                progressIndicator.IsVisible = true;
                SystemTray.ProgressIndicator = progressIndicator;
            }

            progressIndicator.IsIndeterminate = true;
        }

        private void GatherStoryboardEvents()
        {
            showNav = LayoutRoot.FindStoryboard("CommonStates", "ShowNav");
        }

        private void ThreadNavigator_NavigationRequested(object sender, ThreadNavigationArgs e)
        {
            NavigateToThread(e.Thread, e.Page);
        }

        private void ThreadList_Loaded(object sender, RoutedEventArgs e)
        {
            this.context.PageLoading += new EventHandler(OnPageLoading);
            this.context.PageLoaded += new EventHandler(OnPageLoaded);
            this.DataContext = context;

            if (this.reload) { context.CurrentPage = context.CurrentPage; }
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (navVisible)
            {
                VisualStateManager.GoToState(this, "Normal", true);
                ThreadsList.IsHitTestVisible = true;
                e.Cancel = true;
                navVisible = false;
            }
            else
            {
                context.CancelAsync();
                base.OnBackKeyPress(e);
            }
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if (navVisible)
            {
                VisualStateManager.GoToState(this, "Normal", true);
                ThreadsList.IsHitTestVisible = true;
            }
             
            App.Settings.LastViewedForum = context.CurrentForum;

            base.OnNavigatingFrom(e);
            context.CancelAsync();
        }

        private void UpdateFavoritesMenu(ForumData forum)
        {
            var favoriteMenu = ApplicationBar.MenuItems[ADD_REMOVE_FAVORITES_MENU_INDEX] as IApplicationBarMenuItem;
            bool canExecute = this._toggleFavorites.CanExecute(forum);

            if (canExecute)
            {
                favoriteMenu.Text = this._toggleFavorites.Header;
            }
        }

        protected override void OnNavigatedTo(
            System.Windows.Navigation.NavigationEventArgs e)
        {
            SystemTray.IsVisible = !App.Settings.HideSystemTray;

            base.OnNavigatedTo(e);

            ForumData forum = null;
            try 
            {
                int id = Convert.ToInt32(NavigationContext.QueryString["ID"]);
                using (var db = new Data.SAForumDB())
                {
                    forum = db.Forums.Where(f => f.ID == id).Single();
                }
            }
            catch (Exception) 
            {
                forum = App.Settings.LastViewedForum; 
            }

            UpdateFavoritesMenu(forum);

            context.SetForum(forum);

            if (e.NavigationMode == System.Windows.Navigation.NavigationMode.Back)
                reload = false;
        }

        void OnPageLoaded(object sender, EventArgs e)
        {
            this._defaultAppBar.SetButton(RELOADBUTTON_INDEX, "reload", this._reloadUri);
            if (this._reloadingListBox != null)
            {
                this._reloadingListBox.StopPullToRefreshLoading(true);
                this._reloadingListBox = null;
            }
        }

        void OnPageLoading(object sender, EventArgs e)
        {
            this._defaultAppBar.SetButton(RELOADBUTTON_INDEX, "cancel", this._cancelUri);
        }

        private void ThreadSelected(object sender, 
            System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                Models.SAThread thread = e.AddedItems[0] as Models.SAThread;
                NavigateToThread(thread);
            }
        }

        private void AppBarButtonClick(object sender, System.EventArgs e)
        {
            var button = sender as IApplicationBarIconButton;
            if (button == null) return;

            switch (this._defaultAppBar.Buttons.IndexOf(button))
            {
                case PREVBUTTON_INDEX:
                    GoToPage(this.context.CurrentPage - 1);
                    break;

                case NEXTBUTTON_INDEX:
                    GoToPage(this.context.CurrentPage + 1);
                    break;

                case RELOADBUTTON_INDEX:
                    if (this.context.IsLoading) { this.context.CancelAsync(); }
                    else { this.GoToPage(this.context.CurrentPage); }
                    break;
            }
        }

        private void GoToPage(int index)
        {
            if (this.context.IsLoading) return;
            
            else if (index == this.context.CurrentPage)
            {
                this.context.Refresh();
            }
            
            else
            {
                this.context.CurrentPage = index;
            }
        }

        private void NavigateToThread(ThreadData thread, int page = 0)
        {
            PhoneApplicationService.Current.State["Thread"] = thread;
            
            var saThread = thread as SAThread;
            if (saThread.ThreadSeen == false)
                page = 1;

            var uri = App.Settings.ThreadViewStyle == AwfulSettings.ViewStyle.Vertical ? "/ThreadView.xaml" : "/ViewThread.xaml";
            if (page > 0)
            {
                uri = uri + "?Page=" + page;
            }

            NavigationService.Navigate(new Uri(uri, UriKind.RelativeOrAbsolute));
        }

        private void ThreadContextMenu_Closed(object sender, System.Windows.RoutedEventArgs e)
        {
           
        }

        private void HandleFavorites()
        {
            var forum = context.CurrentForum;

            this._toggleFavorites.Execute(forum);

            var current = this._dbContext.Profiles.SingleOrDefault(profile => profile.ID == App.Settings.CurrentProfileID);
            var exists = this._dbContext.ForumFavorites
                .Where(fav => fav.ForumID == forum.ID && fav.ProfileID == current.ID)
                .SingleOrDefault() == null;

                if (exists) { MessageBox.Show("Forum added to favorites.", ":)", MessageBoxButton.OK); }
                else { MessageBox.Show("Forum removed from favorites.", ":)", MessageBoxButton.OK); }
            
            this.UpdateFavoritesMenu(forum);
            HomePage.ViewModel.RefreshFavorites();
        }

        private void PageCommandMenuTapped(object sender, ContextMenuItemSelectedEventArgs e)
        {
            var element = sender as FrameworkElement;
            SAThread thread = element.Tag as SAThread;

            ThreadNavigator.Thread = thread;
            navVisible = true;
            this.OnVisualStateFinish("ShowNav", showNav, () =>
            {
                ThreadsList.IsHitTestVisible = false;
            });
        }

        private void AppBarMenuClick(object sender, System.EventArgs e)
        {
            var menu = sender as IApplicationBarMenuItem;
            if (menu == null) return;

            int index = this._defaultAppBar.MenuItems.IndexOf(menu);
            switch (index)
            {
                case ADD_REMOVE_FAVORITES_MENU_INDEX:
                    this.HandleFavorites();
                    break;

                case NEW_THREAD_MENU_INDEX:
                    this.HandleNewThread();
                    break;
            }
        }

        private void HandleNewThread()
        {
            PhoneApplicationService.Current.State["Forum"] = this.context.CurrentForum;
            this.NavigationService.Navigate(new Uri("/NewThread.xaml", UriKind.RelativeOrAbsolute));
        }

        private void ThreadsList_RefreshRequested(object sender, EventArgs e)
        {
            var listBox = sender as RadDataBoundListBox;
            if (listBox != null)
            {
                this._reloadingListBox = listBox;
                this.GoToPage(this.context.CurrentPage);
            }
        }
    }
}
