using System;
using System.ComponentModel;
using Awful.Models;
using System.Collections;
using System.Windows;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Awful.Helpers;
using Awful.Services;
using System.Threading;
using KollaSoft;
using System.Windows.Threading;
using System.Windows.Data;
using Telerik.Windows.Data;

namespace Awful.ViewModels
{
    public class ThreadsViewModel : ViewModelBase
    {
        private ForumData _data;
        private bool _svcIsBusy;
        private int _currentPageNumber;
        private IList<ThreadData> m_Threads;
        private readonly Dictionary<int, IList<ThreadData>> _threadCache = new Dictionary<int, IList<ThreadData>>();
        private readonly DispatcherTimer _threadListDelayTimer = new DispatcherTimer();
        private ForumPage currentPage;
        private string m_pageNumberLabel;

        private readonly CollectionViewSource _newThreads = new CollectionViewSource();
        private readonly CollectionViewSource _seenThreads = new CollectionViewSource();
        private readonly ObservableCollection<DataDescriptor> _miscFilter = new ObservableCollection<DataDescriptor>();
        private readonly ObservableCollection<DataDescriptor> _allFilter = new ObservableCollection<DataDescriptor>();

        public event EventHandler PageLoading;
        public event EventHandler PageLoaded;

        public IEnumerable<DataDescriptor> GroupData { get { return this._miscFilter; } }
        public IEnumerable<DataDescriptor> StickyData { get { return this._allFilter; } }

        public ICollectionView NewThreads 
        { 
            get 
            {
                if (this._newThreads.Source != null)
                    this._newThreads.View.Refresh();
                
                return this._newThreads.View; 
            } 
        }

        public ICollectionView SeenThreads
        {
            get
            {
                if (this._seenThreads.Source != null)
                    this._seenThreads.View.Refresh();

                return this._seenThreads.View;
            }
        }

        public ForumData CurrentForum
        {
            get { return _data; }
        }

        public string ForumName
        {
            get
            {
                if (_data == null) return String.Empty;
                return _data.ForumName;
            }
        }

        public string PageNumberLabel
        {
            get
            {
                return m_pageNumberLabel;
            }
            set
            {
                m_pageNumberLabel = value;
                NotifyPropertyChangedAsync("PageNumberLabel");
            }
        }

        public IList<ThreadData> Threads
        {
            get { return m_Threads; }
            set
            {
                this.m_Threads = value;
                NotifyPropertyChangedAsync("Threads");

                this._newThreads.Source = value;
                this._seenThreads.Source = value;

                NotifyPropertyChangedAsync("NewThreads");
                NotifyPropertyChangedAsync("SeenThreads");
            }
        }

        public bool IsLoading
        {
            get { return _svcIsBusy; }
            set
            {
                if (_svcIsBusy != value)
                {
                    _svcIsBusy = value;
                    NotifyPropertyChangedAsync("IsLoading");
                }
            }
        }

        public int CurrentPage
        {
            get 
            {
                return this._currentPageNumber; 
            }
            set 
            {
                if (value > 0 && value <= _data.MaxPages)
                {          
                    this.Load(value, _currentPageNumber);
                    this._currentPageNumber = value;
                }
            }
        }

        public void CancelAsync() { SAForumPageFactory.CancelAsync(); }

        public void AddThreadToBookmarks(ThreadData thread, Action onFinish)
        {
            PageLoading.Fire(this);
            
            SomethingAwfulThreadService.Current.AddBookmarkAsync(thread,
                result =>
                {
                    switch (result)
                    {
                        case Awful.Core.Models.ActionResult.Success:
                            MessageBox.Show("Request successful.", ":)", MessageBoxButton.OK);
                            break;

                        case Awful.Core.Models.ActionResult.Failure:
                            MessageBox.Show("Request failed.", ":(", MessageBoxButton.OK);
                            break;
                    }
                    PageLoaded.Fire(this);
                    onFinish();
                });
        }

        public void RemoveThreadFromBookmarks(ThreadData thread, Action onFinish)
        {
            PageLoading.Fire(this);
         
            SomethingAwfulThreadService.Current.RemoveBookmarkAsync(thread,
               result =>
               {
                   switch (result)
                   {
                       case Awful.Core.Models.ActionResult.Success:
                           MessageBox.Show("Thread removed from bookmarks.", ":)", MessageBoxButton.OK);
                           break;

                       case Awful.Core.Models.ActionResult.Failure:
                           MessageBox.Show("Request failed.", ":(", MessageBoxButton.OK);
                           break;
                   }
                   PageLoaded.Fire(this);
                   onFinish();
               });

        }

        private void Load(int newPage, int oldPage)
        {
            // load up the thread from the cache
            var cachedPage = ForumCache.GetPageFromCache(this.CurrentForum as SAForum,
                newPage);

            if (cachedPage != null)
            {
                Threads = null;
                PageLoading.Fire(this);
                IsLoading = true;
                
                Action action = () =>
                    {
                        Threads = cachedPage.Threads;
                        IsLoading = false;
                        PageLoaded.Fire(this);
                    };

                action.DelayThenInvoke(new TimeSpan(0, 0, 2));
            }

            // if not in the cache, get it from the web anyway
            else
            {
                Threads = null;
                RefreshPageData(newPage, oldPage);
            }
        }

        public void Refresh()
        {
            Threads = null;
            RefreshPageData(_currentPageNumber, _currentPageNumber);
        }

        private void RefreshPageData(int page, int oldPage)
        {
            PageLoading.Fire(this);
            IsLoading = true;

            string newLabel = this.PageNumberLabel;
            this.PageNumberLabel = "?";

            SAForumPage fPage = new SAForumPage(this._data, page);
            SAForumPageFactory.BuildAsync(result =>
                {
                    switch (result)
                    {
                        case Awful.Core.Models.ActionResult.Cancelled:
                            _currentPageNumber = oldPage;
                            PageLoaded.Fire(this);
                            break;

                        case Awful.Core.Models.ActionResult.Failure:
                            MessageBox.Show(Globals.Constants.WEB_PAGE_FAILURE,
                               ":(", MessageBoxButton.OK);
                            PageLoaded.Fire(this);
                            _currentPageNumber = oldPage;
                            break;

                        case Awful.Core.Models.ActionResult.Success:
                            currentPage = fPage;
                            newLabel = string.Format("{0}", currentPage.PageNumber);
                            ForumCache.AddPageToCache(fPage);
                            NotifyPropertyChangedAsync("CurrentPage");
                            break;
                    }

                    if (currentPage != null)
                    {
                        ForumCache.AddPageToCache(currentPage as SAForumPage);
                        Threads = currentPage.Threads;
                        PageNumberLabel = newLabel;
                    }

                    IsLoading = false;
                    PageLoaded.Fire(this);

                }, fPage);
        }

        public ThreadData this[int index]
        {
            get { return Threads[index]; }
        }

        public ThreadsViewModel(int pageNumber = 1) 
		{
            if (this.IsInDesignMode) return;

            this._currentPageNumber = pageNumber;
            this._data = new SAForum() { MaxPages = 1 };
            this._threadListDelayTimer.Interval = new TimeSpan(0, 0, 0, 0, 350);
            this._seenThreads.Filter += new FilterEventHandler(SeenThreads_Filter);
            this._newThreads.Filter += new FilterEventHandler(NewThreads_Filter);

            this.InitializeMiscFilter();
		}

        private ThreadGroup GroupThreadsByRating(ThreadData thread)
        {
            int rating = (thread as SAThread).Rating;
            ThreadGroup result = new ThreadGroup();

            switch (rating)
            {
                case 5:
                    result.Name = "gold";
                    result.Priority = 1;
                    break;

                case 4:
                    result.Name = "four stars";
                    result.Priority = 2;
                    break;

                case 3:
                    result.Name = "three stars";
                    result.Priority = 3;
                    break;

                case 2:
                    result.Name = "two stars";
                    result.Priority = 4;
                    break;

                case 1:
                    result.Name = "crap";
                    result.Priority = 5;
                    break;

                default:
                    result.Name = "unrated";
                    result.Priority = 6;
                    break;
            }

            return result;
        }

        private ThreadGroup GroupThreadsBySticky(ThreadData thread)
        {
            SAThread saThread = thread as SAThread;
            if (saThread == null)
                throw new ArgumentException("Expected SAThread.");

            ThreadGroup group = new ThreadGroup();
            if (saThread.IsSticky)
            {
                group.Name = "stickied";
                group.Priority = 0;
            }

            else
            {
                group.Name = "non-stickied";
                group.Priority = 1;
            }

            return group;
        }

        private void InitializeMiscFilter()
        {
            var groupByRating = new GenericGroupDescriptor<ThreadData, ThreadGroup>(GroupThreadsByRating);
            var groupBySticky = new GenericGroupDescriptor<ThreadData, ThreadGroup>(GroupThreadsBySticky);

            this._miscFilter.Add(groupByRating);
            this._allFilter.Add(groupBySticky);
        }

        private void NewThreads_Filter(object sender, FilterEventArgs e)
        {
            SAThread thread = e.Item as SAThread;
            
            if (thread == null)
            {
                e.Accepted = false;
            }

            else
            {
                bool isNew = !thread.ThreadSeen;
                e.Accepted = e.Accepted && isNew;
            }
        }

        private void SeenThreads_Filter(object sender, FilterEventArgs e)
        {
            SAThread thread = e.Item as SAThread;

            if (thread == null)
                e.Accepted = false;

            else
            {
                bool isOld = thread.ThreadSeen;
                e.Accepted = e.Accepted && isOld;
            }
        }

        internal void SetForum(ForumData forum)
        {
            this._data = forum;
        }

        protected override void InitializeForDesignMode()
        {
           
        }
    }

    public class ThreadGroup : IComparable<ThreadGroup>, IEquatable<ThreadGroup>
    {
        public string Name { get; set; }
        public int Priority { get; set; }

        public int CompareTo(ThreadGroup other)
        {
            return this.Priority.CompareTo(other.Priority);
        }

        public override string ToString()
        {
            return this.Name;
        }

        public bool Equals(ThreadGroup other)
        {
            return this.Name.Equals(other.Name);
        }

        public override bool Equals(object obj)
        {
            if (obj is ThreadGroup)
            {
                return this.Equals(obj as ThreadGroup);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }
    }
}
