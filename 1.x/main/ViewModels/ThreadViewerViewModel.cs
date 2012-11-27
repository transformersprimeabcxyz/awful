using System;
using System.Linq;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;
using Awful.Models;
using System.Threading;
using Awful.Helpers;
using KollaSoft;

namespace Awful.ViewModels
{
    public class ThreadViewerViewModel : PropertyChangedBase
    {
        #region Fields

        private readonly Services.ThreadService _threadSvc = Services.SomethingAwfulThreadService.Current;
        private ThreadPage _currentThreadPage = null;
        private ThreadData _currentThreadData = null;
        private string _currentHtml;
        private bool _reload;
        private ICancellable _Cancellable;
        
        private string m_ReplyMode = "reply";
        private bool m_IsInEditMode;
        private string m_ThreadTitle;
        private int m_CurrentPage;
        private int m_TotalPages;
        private IList<PostData> m_Posts;
        private int m_PostIndex = -1;
        private bool m_IsPageLoading;
        private string m_ReplyText;
        private bool m_IsInHorizontalViewMode;
        private bool m_ShowReply = false;

        // commands
        private readonly Commands.ImageCommand m_ImageCommand = App.ImageCommand;
        private readonly Commands.PostCommand m_PostCommand = App.PostCommand;
        private readonly ICommand m_ThreadCommand;
        private readonly Commands.EditCommand m_EditCommand = new Commands.EditCommand();
        private readonly Commands.ReplyCommand m_ReplyCommand = new Commands.ReplyCommand();
        
        #endregion

        #region Properties

        public int CurrentUserID { get; set; }

        public bool ShowReply
        {
            get { return m_ShowReply; }
            set
            {
                if (m_ShowReply == value) return;
                m_ShowReply = value;
                NotifyPropertyChangedAsync("ShowReply");
            }
        }

        protected string _SaveIndex { get; set; }

        public string HtmlUri { get; protected set; }

        public bool IsInHorizontalViewMode
        {
            get { return m_IsInHorizontalViewMode; }
            set { m_IsInHorizontalViewMode = value; }
        }

        public ThreadData Thread
        {
            get { return _currentThreadData; }
        }

        private string _ReplyMode
        {
            get { return m_ReplyMode; }
            set
            {
                if (m_ReplyMode == value) return;
                m_ReplyMode = value;
                NotifyPropertyChangedAsync("ReplyMode");
            }
        }

        public string ReplyMode
        {
            get { return m_ReplyMode; }
        }

        public bool IsInEditMode
        {
            get { return m_IsInEditMode; }
            set
            {
                var post = this.SelectedPost as SAPost;
                if (value && post.IsEditable)
                {
                    this.m_EditCommand.Post = post;
                    _ReplyMode = string.Format("edit post #{0}", post.PostIndex);
                    m_IsInEditMode = true;
                }
                else
                {
                    _ReplyMode = "reply";
                    m_IsInEditMode = false;
                }

                NotifyPropertyChangedAsync("IsInEditMode");
            }
        }

        public string ReplyText
        {
            get { return m_ReplyText; }
            set
            {
                m_ReplyText = value;
                NotifyPropertyChangedAsync("ReplyText");
            }
        }

        public bool IsPageLoading
        {
            get { return m_IsPageLoading; }
            set
            {
                if (m_IsPageLoading == value) return;
                
                m_IsPageLoading = value;

                if (value) { PageLoading.Fire(this); }

                NotifyPropertyChangedAsync("IsPageLoading");
            }
        }

        public string ThreadTitle
        {
            get 
            { 
                return m_ThreadTitle == null ? "?" : m_ThreadTitle; 
            }
            set
            {
                if (m_ThreadTitle == value) return;
                m_ThreadTitle = value;
                NotifyPropertyChangedAsync("ThreadTitle");
            }
        }

        protected int _TotalPages
        {
            get { return m_TotalPages; }
            set
            {
                if (m_TotalPages == value) return;
                m_TotalPages = value;
                NotifyPropertyChangedAsync("TotalPages");
            }
        }

        protected int _CurrentPage
        {
            get { return m_CurrentPage; }
            set
            {
                if (m_CurrentPage == value) return;
                m_CurrentPage = value;
                NotifyPropertyChangedAsync("CurrentPage");
            }
        }

        public string CurrentPage { get { return m_CurrentPage == 0 ? "?" : m_CurrentPage.ToString(); } }
        public string TotalPages {  get { return m_TotalPages == 0 ? "?" : m_TotalPages.ToString(); } }

        public IList<PostData> Posts
        {
            get { return m_Posts; }
            set
            {
                if (m_Posts == value) return;
                m_Posts = value;
                NotifyPropertyChangedAsync("Posts");
            }
        }

        public PostData SelectedPost
        {
            get 
            {
                try { return Posts[PostIndex]; }
                catch (Exception) { return null; }
            }

            set
            {
                if (value == null) this.PostIndex = -1;
                else
                {
                    if (this.Posts != null)
                    {
                        int index = this.Posts.IndexOf(value);
                        this.PostIndex = index;
                    }
                }

                NotifyPropertyChangedAsync("SelectedPost");
            }
        }

        public int PostIndex
        {
            get { return m_PostIndex; }
            set
            {
                SetPost(m_PostIndex, value);
                if (m_PostIndex == value) return;
                if (m_Posts != null && (value < 0 || value >= m_Posts.Count)) return;
                m_PostIndex = value;
                NotifyPropertyChangedAsync("PostIndex");
            }
        }

        public ICommand ImageCommand { get { return m_ImageCommand; } }
        public ICommand PostCommand { get { return m_PostCommand; } }
        public ICommand ThreadCommand { get { return m_ThreadCommand; } }
       
        #endregion

        #region Events

        public event EventHandler PageLoading;
        public event EventHandler<PageLoadedEventArgs> PageLoaded;
        public event EventHandler TextLoading;
        public event EventHandler<WebContentLoadedEventArgs> TextLoaded;
        public event EventHandler Sending;
        public event EventHandler SendFailed;
        public event EventHandler SendSucessful;
       
        #endregion

        #region Public Methods

        public void Send(string text)
        {
            MessageBoxResult result;
            if (IsInEditMode)
            {
                result = MessageBox.Show("Send edit?", ":o", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    Sending.Fire(this);
                    m_EditCommand.Execute(text);
                }
            }
            else
            {
                result = MessageBox.Show("Send reply?", ":o", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    Sending.Fire(this);
                    this.m_ReplyCommand.Thread = this._currentThreadData;
                    this.m_ReplyCommand.Execute(text);
                }
            }
        }

        public void SetImageUrl(string url)
        {
            m_ImageCommand.ImageUrl = url;
        }

        public virtual void LoadThreadPageAsync(ThreadData thread, int pageNumber = 0, int postNumber = -1)
        {
            if (pageNumber < 0 || pageNumber > thread.MaxPages)
            {
                string error = string.Format("ThreadViewerViewModel: pageNumber {0} not in range 1 to {1}.",
                    pageNumber, thread.MaxPages);

                Awful.Core.Event.Logger.AddEntry(error);
                Awful.Core.Event.Logger.AddEntry("Setting page index to 0 instead, to fetch last page...");
                pageNumber = 0;
            }

            this.IsPageLoading = true;

            if (thread.ThreadTitle == null) { this.ThreadTitle = ((SAThread)thread).ThreadURL; }
            else { this.ThreadTitle = thread.ThreadTitle; }

            this.Posts = null;
            this._CurrentPage = pageNumber;
            this._TotalPages = thread.MaxPages;
            this._currentThreadData = thread;

            // else fetch page from cache

            SAThread saThread = thread as SAThread;
            SAThreadPage tPage = null;

            if (this.CurrentUserID == 0) { tPage = this.LoadFromDatabase(saThread, pageNumber); }

            if (tPage == null)
            {
                tPage = new SAThreadPage(saThread, pageNumber, this.CurrentUserID);
                this._Cancellable = SAThreadPageFactory.BuildAsync((result, page) =>
                    {
                        postNumber = thread.LastViewedPostIndex;
                        thread.LastViewedPageIndex = 0;
                        thread.LastViewedPostIndex = -1;
                        ThreadPool.QueueUserWorkItem(state => { this.HandleResult(result, page, postNumber); }, null);
                    }, tPage);
            }

            else 
            {
                ThreadPool.QueueUserWorkItem(state => { this.HandleResult(Awful.Core.Models.ActionResult.Success, tPage, postNumber); }, null);
            }
        }

        public virtual void ReloadCurrentPageAsync(ThreadData thread, int postNumber = -1)
        {
            this.IsPageLoading = true;
            this.ThreadTitle = thread.ThreadTitle;
            this.Posts = null;
            this._TotalPages = thread.MaxPages;
            this._reload = true;

            SAThread saThread = thread as SAThread;
            SAThreadPage tPage = new SAThreadPage(saThread, this._CurrentPage, this.CurrentUserID);

            this._Cancellable = SAThreadPageFactory.BuildAsync((result, page) =>
            {
                postNumber = thread.LastViewedPostIndex;
                thread.LastViewedPageIndex = 0;
                thread.LastViewedPostIndex = -1;
                ThreadPool.QueueUserWorkItem(state => { this.HandleResult(result, page, postNumber); }, null);

            }, tPage);
        }

        public void CancelAsync() 
        {
            if (this._Cancellable != null)
            {
                this._Cancellable.Cancel();
                this._Cancellable = null;
            }
        }

        #endregion

        #region Private Methods

        private void SetPost(int oldIndex, int newIndex)
        {
            if(m_Posts == null) return;
            if (newIndex >= m_Posts.Count) return;

            if (newIndex < 0) return;

            var post = newIndex < 0 ? null : m_Posts[newIndex] as SAPost;
            m_PostCommand.CurrentPost = post;
        }

        private SAThreadPage LoadFromDatabase(SAThread thread, int pageNumber)
        {
            SAThreadPage page = null;

            /// only do work if the pageNumber is nonzero and positive. Should probably throw an exception here...
            if (pageNumber >= 1)
            {
                page = ThreadCache.GetPageFromCache(thread, pageNumber);
            }

            return page;
        }

        protected void HandleResult(Awful.Core.Models.ActionResult result, ThreadPage page, int postNumber)
        {
            ThreadPage old = this._currentThreadPage;

            switch (result)
            {
                case Awful.Core.Models.ActionResult.Success:
                    this._currentThreadPage = page;
                    if (this.CurrentUserID == 0) { ThreadCache.AddPageToCache(page as SAThreadPage); }
                    break;

                case Awful.Core.Models.ActionResult.Failure:
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                        { 
                            MessageBox.Show(Globals.Constants.WEB_PAGE_FAILURE, ":(", MessageBoxButton.OK); 
                        });
                    break;
            }

            object[] args = new object[2] { _currentThreadPage, postNumber };
            this.BuildContentFromThreadPage(args);
        }

        private void BuildContentFromThreadPage(object args)
        {
            string content = string.Empty;
            var parameters = args as object[];

            SAThreadPage newPage = parameters[0] as SAThreadPage;

            if (newPage != null)
            {
                _CurrentPage = newPage.PageNumber;
                _TotalPages = newPage.MaxPages;
                ThreadTitle = newPage.ThreadTitle;

                // load up posts from page
                Posts = newPage.Posts;

                int index = (int)parameters[1];
                if (index == -1)
                {
                    // find first unread post
                    var unread = Posts.Where(post => { return (post as SAPost).HasSeen == false; }).FirstOrDefault();
                    // set current index to unread or first post of page
                    PostIndex = unread == null ? 0 : Posts.IndexOf(unread);
                }
                else
                {
                    PostIndex = index;
                }

                // generate content
                if (_currentHtml == null)
                {
                   this._currentHtml = newPage.Html;
                   this._currentHtml.SaveTextToFile(this._SaveIndex, "\\");
                }
            }

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                IsPageLoading = false;
                ThreadData data = newPage == null ? null : newPage.Thread;
                PageLoaded.Fire<PageLoadedEventArgs>(this, new PageLoadedEventArgs(data, _CurrentPage, _currentHtml));
                _currentHtml = null;
            });
        }

        private void OnTextContentFromWeb(object sender, WebContentLoadedEventArgs e)
        {
            TextLoaded.Fire(this, e);
        }

        private void ReplyCommand_RequestCompleted(object sender,ActionResultEventArgs e)
        {
            if (e.Result == Awful.Core.Models.ActionResult.Success)
            {
                SendSucessful.Fire(this);
                LoadThreadPageAsync(_currentThreadData);
            }
            else
                SendFailed.Fire(this);
        }

        private void EditCommand_RequestCompleted(object sender,ActionResultEventArgs e)
        {
            if (e.Result == Awful.Core.Models.ActionResult.Success)
            {
                SendSucessful.Fire(this);
                IsInEditMode = false;
                ReloadCurrentPageAsync(_currentThreadData, m_EditCommand.Post.PostIndex - 1);
                m_EditCommand.Post = null;
            }
            else
                SendFailed.Fire(this);
        }

        private void PostCommand_ContentLoading(object sender, EventArgs e)
        {
            TextLoading.Fire(this);
        }

        #endregion

        // ctor
        public ThreadViewerViewModel()
        {
            this._SaveIndex = "www\\index.html";
            this.HtmlUri = "index.html";

            m_ReplyText = String.Empty;
            m_PostCommand.ContentLoaded += new EventHandler<WebContentLoadedEventArgs>(OnTextContentFromWeb);
            m_PostCommand.ContentLoading += new EventHandler(PostCommand_ContentLoading);
            m_ReplyCommand.RequestCompleted += new EventHandler<ActionResultEventArgs>(ReplyCommand_RequestCompleted);
            m_EditCommand.RequestCompleted += new EventHandler<ActionResultEventArgs>(EditCommand_RequestCompleted);
        }
    }
}
