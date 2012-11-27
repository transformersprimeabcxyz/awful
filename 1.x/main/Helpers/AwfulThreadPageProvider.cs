using System;
using System.Linq;
using KollaSoft;
using KollaSoft.Primitives;
using Awful.Models;
using System.Collections.Generic;
using System.Threading;
using System.Windows;

namespace Awful.Helpers
{
    public class AwfulThreadPageItem : PropertyChangedBase, Indexable<SAThreadPage>
    {
        private bool _isLoading = false;
        private SAPost _selectedPost = null;
        private int _selectedPostIndex;
        private SAThreadPage _page = null;
        private readonly SAThread _parent = null;
        private readonly int _pageNumber;
        private string _html;

        #region Properties

        public SAThreadPage Item
        {
            get { return this.Page; }
            set { this.Page = value; }
        }

        public bool IsLoading
        {
            get { return this._isLoading; }
            private set
            {
                this._isLoading = value;
                NotifyPropertyChangedAsync("IsLoading");
            }
        }

        public SAPost SelectedPost
        {
            get { return this._selectedPost; }
            set
            {
                try
                {
                    this._selectedPost = value;
                    this._selectedPostIndex = this.Page.Posts.IndexOf(value);
                    NotifyPropertyChangedAsync("SelectedPost");
                    NotifyPropertyChangedAsync("SelectedPostIndex");
                }
                catch (Exception ex)
                {
                    this._selectedPost = null;
                    this._selectedPostIndex = -1;
                    NotifyPropertyChangedAsync("SelectedPost");
                    NotifyPropertyChangedAsync("SelectedPostIndex");
                }
            }
        }

        public int SelectedPostIndex
        {
            get { return this._selectedPostIndex; }
            set
            {
                try
                {
                    this._selectedPostIndex = value;
                    this._selectedPost = this.Page.Posts[value] as SAPost;
                    NotifyPropertyChangedAsync("SelectedPost");
                }
                catch (Exception)
                {
                    this._selectedPostIndex = -1;
                    this._selectedPost = null;
                    NotifyPropertyChangedAsync("SelectedPost");
                }
            }
        }

        public int Index { get; set; }

        public SAThreadPage Page
        {
            get { return this._page; }
            set
            {
                this._page = value;
                NotifyPropertyChangedAsync("Page");
            }
        }

        #endregion

        public event EventHandler PageLoading;
        public event EventHandler PageLoaded;
        public event EventHandler PageFailed;

        public AwfulThreadPageItem() { }

        public AwfulThreadPageItem(SAThread parent, int pageNumber)
        {
            this._parent = parent;
            this._pageNumber = pageNumber;
            this._selectedPostIndex = -1;
            this._selectedPost = null;
        }

        public void Activate()
        {
            if (this.Page != null) return;
            else
            {
                this.IsLoading = true;
                SAThreadPage page = ThreadCache.GetPageFromCache(this._parent, this._pageNumber);
                if (page != null)
                {
                    this.PageLoading.Fire(this);
                    this.Page = page;
                    this.IsLoading = false;
                    this.PageLoaded.Fire(this);
                }
                else
                {
                    page = new SAThreadPage(this._parent, this._pageNumber);
                    SAThreadPageFactory.BuildAsync((result, p) =>
                    {
                        switch (result)
                        {
                            case Awful.Core.Models.ActionResult.Success:
                                this.HandleBuildSuccess(p);
                                break;

                            case Awful.Core.Models.ActionResult.Failure:
                                this.HandleBuildFailure();
                                break;

                            default:
                                this.IsLoading = false;
                                this.PageFailed.Fire(this);
                                break;
                        }

                    }, page);
                }
            }
        }

        private void HandleBuildSuccess(SAThreadPage p)
        {
            this._parent.LastViewedPageIndex = 0;
            this._parent.LastViewedPostIndex = -1;
            this.Page = p;

            ThreadPool.QueueUserWorkItem(state =>
                {
                    // find first unread post
                    if (this.SelectedPostIndex == -1)
                    {
                        var findLastReadPostQuery = from post in p.Posts
                                                    where (post as SAPost).HasSeen == false
                                                    select post;

                        PostData firstUnreadPost = findLastReadPostQuery.FirstOrDefault();
                        if (firstUnreadPost == null) { this.SelectedPost = p.Posts[0] as SAPost; }
                        else { this.SelectedPost = p.Posts[firstUnreadPost.PostIndex - 1] as SAPost; }
                    }

                    this._html = this.Page.Html;
                    this._html.SaveTextToFile("index.html", "\\");
                    this.IsLoading = false;
                    this.PageLoaded.Fire(this);

                }, null);
        }

        private void HandleBuildFailure()
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                this.IsLoading = false;
                MessageBox.Show(Globals.Constants.WEB_PAGE_FAILURE, ":(", MessageBoxButton.OK);
                PageFailed.Fire(this);
            });
        }
    }

    public class AwfulThreadPageProvider : KSVirtualizedList<AwfulThreadPageItem>
    {
        private readonly SAThread _thread;

        public event EventHandler SizeChanged;

        public AwfulThreadPageProvider(SAThread thread)
        {
            this._thread = thread;
        }

        protected override AwfulThreadPageItem GetItem(int index)
        {
            if (index == -1) return null;

            AwfulThreadPageItem item = new AwfulThreadPageItem(this._thread, index + 1);
            return item;
        }

        protected override void SetItem(AwfulThreadPageItem item, int index)
        {
            item.Index = index;
        }

        public override bool Contains(AwfulThreadPageItem item)
        {
            throw new NotImplementedException();
        }

        public override int Count
        {
            get
            {
                return this._thread.MaxPages;
            }
            protected set
            {
                this._thread.MaxPages = value;
            }
        }

        public override int IndexOf(AwfulThreadPageItem item)
        {
            return item.Index;
        }
    }
}
