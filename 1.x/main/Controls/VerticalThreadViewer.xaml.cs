using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Awful.ViewModels;
using Awful.Helpers;
using Microsoft.Phone.Tasks;
using Awful.Models;
using System.Windows.Threading;
using Telerik.Windows.Controls;
using Awful.Services;
using Awful.Menus;
using KollaSoft;

namespace Awful
{
	public partial class VerticalThreadViewer : UserControl
	{
        private const int MAX_REPLY_CHARS = 1977;

        private ThreadViewerViewModel _context;
        private readonly ThreadService _threadSvc = Services.SomethingAwfulThreadService.Current;
        private readonly DispatcherTimer _timer = new DispatcherTimer();
        private readonly Commands.RateThreadCommand _rate = new Commands.RateThreadCommand();
        private readonly PostContextMenu _postMenu = new PostContextMenu();
        private readonly ImageContextMenu _imgMenu = new ImageContextMenu();
        private readonly KSPagedString _replyText = new KSPagedString(MAX_REPLY_CHARS);
        private int _replyTextPage = 0;

        public string CurrentReplyTextPage
        {
            get { return this._replyText[this._replyTextPage].Text; }
            set { this._replyText[this._replyTextPage].Text = value; }
        }

        public PostContextMenu PostMenu { get { return _postMenu; } } 

        public int CurrentUserID { get; set; }

        private Commands.ViewThreadCommand m_viewThread;
        private SAPost _selectedPost;
        private bool _reloaded;
        private bool _quoteEnabled;
       
        private const int sendButton = 0;
        private const int cancelButton = 1;
        private const int hideButton = 2;

     

        public event EventHandler<ThreadNavigationArgs> NavigationRequested;

        private Commands.ViewThreadCommand _ViewThread
        {
            get
            {
                if (m_viewThread == null)
                    m_viewThread = new Commands.ViewThreadCommand();

                return m_viewThread;
            }
        }

        public bool ShowRatingsWindow
        {
            get { return RatingWindow.IsOpen; }
            set { RatingWindow.IsOpen = value; }
        }

        public bool ShowReplyWindow
        {
            get { return ReplyWindow.IsOpen; }
            set { ReplyWindow.IsOpen = value; }
        }

		public VerticalThreadViewer()
		{
			// Required to initialize variables
			InitializeComponent();
            BindEvents();

            this.ThreadPageLabel.Visibility = System.Windows.Visibility.Collapsed;

            this.ReplyBox.Context.Text = string.Empty;
            this.ReplyBox.Context.Title = string.Empty;

            this.UpdateReplyBoxInfo();
            
            ThreadContentView.Opacity = 0;

            if (App.Layout.CurrentTheme.Background.ToString().ToLower().Equals("#ffffffff"))
            {
                var gradientTop = this.Resources["LightGradientTop"] as GradientBrush;
                var gradientBottom = this.Resources["LightGradientBottom"] as GradientBrush;
                this.TransparentBorderTop.Background = gradientTop;
                this.TransparentBorderBottom.Background = gradientBottom;
            }

            else
            {
                var gradientTop = this.Resources["DarkGradientTop"] as GradientBrush;
                var gradientBottom = this.Resources["DarkGradientBottom"] as GradientBrush;
                this.TransparentBorderTop.Background = gradientTop;
                this.TransparentBorderBottom.Background = gradientBottom;
            }
		}

        private void BindEvents()
        {
            this._postMenu.ContentLoading += new EventHandler(PostMenu_ContentLoading);
            this._postMenu.ContentLoaded += new EventHandler<WebContentLoadedEventArgs>(PostMenu_ContentLoaded);
            this._postMenu.EditTapped += new EventHandler(EditPostMenuItem_Tapped);
            this._postMenu.FilterByAuthorTapped += new EventHandler(PostMenu_ByAuthorTapped);

            this.ReplyBox.replyText.LostFocus += new RoutedEventHandler(ReplyBox_LostFocus);
            this.ReplyBox.replyText.GotFocus += new RoutedEventHandler(ReplyBox_GotFocus);
            this.Unloaded += new RoutedEventHandler(VerticalThreadViewer_Unloaded);
            this.ReplyWindow.WindowOpening += new EventHandler<System.ComponentModel.CancelEventArgs>(ReplyWindow_WindowOpening);
            this.ReplyWindow.WindowOpened += new EventHandler<EventArgs>(ReplyWindow_WindowOpened);
            this.PostJumpList.GroupPickerCloseAnimation.Ended += new EventHandler<EventArgs>(PostJumpList_CloseAnimationEnded);
        }

        void ReplyBox_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                var send = this.ReplyWindow.ApplicationBarInfo.Buttons[sendButton];
                send.IsEnabled = false;
            }
            catch (Exception) { }
        }

        void ReplyWindow_WindowOpened(object sender, EventArgs e)
        {
            //this.ReplyBox.replyText.Focus();
        }

        private void ReplyWindow_WindowOpening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this._context != null)
            {
                string title = this._context.ReplyMode;
                this.ReplyBox.Context.Title = title;
            }
        }

        void PostMenu_ByAuthorTapped(object sender, EventArgs e)
        {
            if (this._context != null)
            {
                var menu = sender as Menus.PostContextMenu;
                if (!menu.AuthorFilterEnabled)
                {
                    var post = this._context.SelectedPost as SAPost;
                    this._context.CurrentUserID = post.UserID;
                }
                else { this._context.CurrentUserID = 0; }

                this.ShrinkAndDropContent.BeginThenInvoke(() =>
                    {
                        this._context.LoadThreadPageAsync(this._context.Thread);
                    });
            }
        }

        void UpdateReplyBoxInfo()
        {
           
        }

        void ReplyBox_TextChanged(object sender, TextChangedEventArgs e)
        {
           
        }

        private void ReplyBox_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                var send = this.ReplyWindow.ApplicationBarInfo.Buttons[sendButton];
                send.IsEnabled = true;
            }
            catch (Exception) { }
        }

        private void PostMenu_ContentLoaded(object sender, WebContentLoadedEventArgs e)
        {
            this.ReplyBox.Context.Text += e.Content;
            this.ShowReplyWindow = true;
            this.ReplyBox.replyText.IsReadOnly = false;
        }

        private void PostMenu_ContentLoading(object sender, EventArgs e)
        {
            this.ReplyBox.replyText.IsReadOnly = true;
        }

        private void VerticalThreadViewer_Unloaded(object sender, RoutedEventArgs e)
        {
            UnloadViewModel();
        }

        public void SetViewModel(ThreadViewerViewModel viewModel)
        {
            this._context = viewModel;
            this.DataContext = viewModel;

            if (viewModel != null)
            {
                this.ThreadPageLabel.Visibility = System.Windows.Visibility.Visible;
                this._context.PageLoading += new EventHandler(Context_PageLoading);
                this._context.PageLoaded += new EventHandler<PageLoadedEventArgs>(Context_PageLoaded);
                this._context.Sending += new EventHandler(Context_Sending);
                this._context.SendFailed += new EventHandler(Context_SendFailed);
                this._context.SendSucessful += new EventHandler(Context_SendSuccessful);
            }
        }

        public void UnloadViewModel()
        {
            if (this._context != null)
            {
                this._context.PageLoading -= new EventHandler(Context_PageLoading);
                this._context.PageLoaded -= new EventHandler<PageLoadedEventArgs>(Context_PageLoaded);
                this._context.Sending -= new EventHandler(Context_Sending);
                this._context.SendFailed -= new EventHandler(Context_SendFailed);
                this._context.SendSucessful -= new EventHandler(Context_SendSuccessful);
            }

            this._context = null;
        }

        private void Context_SendSuccessful(object sender, EventArgs e)
        {
            ReplyBox.replyText.Text = String.Empty;
            ReplyBox.replyText.IsReadOnly = false;
            ShrinkAndDropContent.Begin();
        }

        private void Context_SendFailed(object sender, EventArgs e)
        {
            try
            {
                ReplyBox.replyText.IsReadOnly = false;
                ReplyWindow.IsOpen = true;
            }
            catch (Exception ex)
            {
                ReplyWindow.IsOpen = false;
            }
        }

        private void Context_Sending(object sender, EventArgs e)
        {
            ReplyBox.replyText.IsReadOnly = true;
            ReplyWindow.IsOpen = false;
        }

        private void ThreadContentView_HandleNavigation(UrlEventArgs e)
        {
            // keep it in house.
            if (e.Url.Contains("forums.somethingawful.com/showthread.php"))
            {
                _ViewThread.Url = e.Url;
                ThreadData newThread = _ViewThread.GenerateThreadData(Commands.ViewThreadCommand.USE_URL);

                if(newThread == null)
                {
                    var result = MessageBox.Show("Load thread in IE?", ":o", MessageBoxButton.OKCancel);
                    if (result == MessageBoxResult.Cancel)
                        return;
                }

                else
                {
                    NavigationRequested.Fire<ThreadNavigationArgs>(this,
                        new ThreadNavigationArgs(newThread, newThread.LastViewedPageIndex));
                    return;
                }
            }

            // WebBrowserTask can only handle urls containing "http://", "https://", or none specified.
            // any other urls will throw an exception.
            if (e.IsHttpOrHttps)
            {
                WebBrowserTask task = new WebBrowserTask();
                task.Uri = new Uri(e.Url);
                try { task.Show(); }
                catch (Exception) { }
            }
            else
            {
                ThreadContentView.Navigate(new Uri(e.Url));
            }
        }

        private void Context_PageLoaded(object sender, PageLoadedEventArgs e)
        {
            if (e.Thread != null)
            {
                _reloaded = false;
                PostJumpList.Opacity = 0;
                ThreadContentView.Base = "www";
                string uri = this._context.HtmlUri;
                ThreadContentView.Navigate(new Uri(uri, UriKind.RelativeOrAbsolute));
            }

            else { App.IsBusy = false; }
        }

        private void Context_PageLoading(object sender, EventArgs e)
        {
            if (this.DataContext == null)
            {
                this.DataContext = this._context;
                this.ThreadPageLabel.Visibility = System.Windows.Visibility.Visible;
            }

            PostJumpList.Visibility = System.Windows.Visibility.Visible;
            PostJumpList.Opacity = 1;
        }

        private void ThreadContentView_ScriptNotify(object sender, Microsoft.Phone.Controls.NotifyEventArgs e)
        {
            if (e.Value == "pageloaded" && _reloaded == false)
            {
                SetViewStyles();
                return;
            }
            
            else if (e.Value == "styleset" && _reloaded == false)
            {
                ContentInFromBottom.BeginThenInvoke(() =>
                {
                    _reloaded = true;
                    if (_context != null)
                    {
                        var post = _context.SelectedPost as SAPost;
                        ScrollToPost(post);
                        ThreadContentView.Visibility = System.Windows.Visibility.Visible;
                        PostJumpList.Visibility = System.Windows.Visibility.Collapsed;
                    }
                    App.IsBusy = false;
                });

                return;
            }

            string[] separator = new string[] { Globals.Constants.DEMARC };
            var tokens = e.Value.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 1)
            {
                MessageBox.Show(e.Value);
                return;
            }

            switch (tokens[0])
            {
                case "a":
                    string url = tokens[1];
                    ThreadContentView_HandleNavigation(new UrlEventArgs(url));
                    break;

                case "image":
                    this._imgMenu.SelectedUrl = tokens[1];
                    App.MenuProvider.Menu = this._imgMenu;
                    App.MenuProvider.Menu.IsOpen = true;
                    break;

                case "post":
                    int index = Int32.Parse(tokens[1]);
                    _context.PostIndex = index - 1;
                    this._postMenu.SelectedPost = this._context.SelectedPost;
                    App.MenuProvider.Menu = this._postMenu;
                    this._postMenu.IsOpen = true;
                    break;

                case "error":
                    MessageBox.Show(tokens[1]);
                    break;
            }
        }

		private void ThreadContentView_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
		{
            if (_reloaded == true)
                return;

            App.IsBusy = true;
		}

        private void PageHeaderPanel_DoubleTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (_context.IsPageLoading) return;

            ThreadTitleFlip.BeginThenInvoke(() =>
            {
                PostJumpList.IsGroupPickerOpen = true;
            });
        }

		private void PostJumpListItemTap(object sender, Telerik.Windows.Controls.GroupPickerItemTapEventArgs e)
		{
            _selectedPost = e.DataItem as SAPost;
		}

        private void PostJumpList_CloseAnimationEnded(object sender, EventArgs e)
        {
            if (_selectedPost != null)
            {
                this._reloaded = true;
                ScrollToPost(_selectedPost);
                this._selectedPost = null;
                ThreadContentView.Visibility = System.Windows.Visibility.Visible;
                PostJumpList.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void ReplyWindow_ButtonClick(object sender,
          Telerik.Windows.Controls.ApplicationBarButtonClickEventArgs e)
        {
            var button = e.Button;
            var bar = sender as ApplicationBarInfo;
            switch (bar.Buttons.IndexOf(button))
            {
                case sendButton:
                    _context.Send(ReplyBox.replyText.Text.Trim());
                    break;

                case cancelButton:
                    ReplyWindow.IsOpen = false;
                    ReplyBox.replyText.Text = String.Empty;
                    _context.IsInEditMode = false;
                    _quoteEnabled = false;
                    break;

                case hideButton:
                    _quoteEnabled = true;
                    ReplyWindow.IsOpen = false;
                    break;
            }

            var replybutton = bar.Buttons[sendButton];
            replybutton.Text = "reply";
        }

        private void EditPostMenuItem_Tapped(object sender, EventArgs e)
        {
            this._context.IsInEditMode = true;
            var bar = this.ReplyWindow.ApplicationBarInfo;
            if (bar != null)
            {
                var button = bar.Buttons[sendButton];
                button.Text = "send";
            }
        }

        private void PostMenu_Opening(object sender, ContextMenuOpeningEventArgs e)
        {
            if (_quoteEnabled)
            {
                var menu = sender as RadContextMenu;
                var edit = menu.Items[0] as RadContextMenuItem;
                edit.IsEnabled = false;
            }
        }

        private void SetViewStyles()
        {
            string fontSize = App.Settings.PostTextSize.ToString();
            string fg = string.Format("#{0}", App.Layout.CurrentTheme.PostForeground.ToString().Substring(3));
            string accent = string.Format("#{0}", App.Layout.CurrentTheme.PostHasSeen.ToString().Substring(3));
            
            // javascript: set_styles(foreground, accent, fontsize)
            ThreadContentView.InvokeScript("set_styles", fg, accent, fontSize);
        }

        private void ScrollToPost(SAPost post)
        {
            if (post == null) return;

            string smooth;

            if (App.Settings.SmoothScrolling == true)
            {
                smooth = "true";
                ThreadContentView.InvokeScript("scrollTo", "postlink" + post.PostIndex, smooth);
            }
            else
            {
                smooth = "false";
                ThreadContentView.InvokeScript("scrollTo", "post_" + post.PostIndex, smooth);
            }
        }

        private void RatingListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            RatingWindow.IsOpen = false;
            var item = RatingsListBox.SelectedItem as RatingsItem;
            _rate.Thread = _context.Thread;
            _rate.Execute(item.Value);
        }

        private void RatingsAppBar_ButtonClick(object sender,
          Telerik.Windows.Controls.ApplicationBarButtonClickEventArgs e)
        {
            RatingWindow.IsOpen = false;
        }
	}
}