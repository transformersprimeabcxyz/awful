using System;
using System.Linq;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using Microsoft.Phone.Data.Linq;
using Microsoft.Phone.Data.Linq.Mapping;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows;
using System.Runtime.Serialization;
using KollaSoft;
using HtmlAgilityPack;
using System.Collections.Generic;

namespace Awful.Core.Models
{
    [Table]
    public class AwfulThread : PropertyChangedBase, ThreadData
    {
        #region ThreadData Members

        #region Fields

        private bool m_threadSeen;
        private bool m_threadViewedToday;
        private string m_threadTitle;
        private string m_authorName;
        private int m_newPostCount;
        private int m_maxPages;
        private int m_rating;
        private int m_replies;
        private bool m_showPostCount;
        private DateTime? m_LastUpdated;
        private EntitySet<AwfulThreadPage> _pages;
        private EntityRef<AwfulForum> _forum;
        public const string AWFUL_THREAD_PRIMARY_KEY = "ID";
        public const string AWFUL_FORUM_FOREIGN_KEY = "ForumID";
        public const int NULL_THREAD_ID = -1;
        #endregion

        #region Properties

        #region Table Columns

        [Column(IsVersion = true)]
        private Binary _version;

        [Column]
        public bool IsSticky { get; set; }
        
        [Column(IsPrimaryKey = true)]
        public int ID { get; set; }
        
        [Column]
        public bool LastRead
        {
            get { return m_threadSeen; }
            set
            {
                if (m_threadSeen == value) return;
                NotifyPropertyChangingAsync("LastRead");
                m_threadSeen = value;
                NotifyPropertyChangedAsync("LastRead");
            }
        }
        
        [Column]
        public string Title
        {
            get
            {
                return m_threadTitle;
            }
            set
            {
                if (this.m_threadTitle == value) return;
                NotifyPropertyChangingAsync("Title");
                m_threadTitle = value;
                NotifyPropertyChangedAsync("Title");
            }
        }
        
        [Column]
        public string Author
        {
            get
            {
                return this.m_authorName;
            }
            set
            {
                if (this.m_authorName == value) return;
                NotifyPropertyChangingAsync("Author");
                m_authorName = value;
                NotifyPropertyChangedAsync("Author");
            }
        }
        
        [Column]
        public int TotalPages
        {
            get { return this.m_maxPages; }
            set
            {
                if (value != m_maxPages)
                {
                    NotifyPropertyChangingAsync("TotalPages");
                    this.m_maxPages = value;
                    NotifyPropertyChangedAsync("TotalPages");
                }
            }
        }
        
        [Column]
        public int Rating
        {
            get
            {
                return this.m_rating;
            }
            set
            {
                NotifyPropertyChangingAsync("Rating");
                m_rating = value;
                NotifyPropertyChangedAsync("Rating");
                //NotifyPropertyChanged("TitleColor");
            }
        }
        
        [Column]
        public int Replies
        {
            get
            {
                return this.m_replies;
            }
            set
            {
                NotifyPropertyChangingAsync("Replies");
                this.m_replies = value;
                NotifyPropertyChangedAsync("Replies");
            }
        }
        
        [Column(CanBeNull = true)]
        public DateTime? LastUpdated
        {
            get
            {
                if (this.m_LastUpdated.HasValue)
                {
                    DateTime value = this.m_LastUpdated.Value.ToLocalTime();
                    return value;
                }

                return null;
            }
            set
            {
                if (value.HasValue)
                {
                    DateTime toUTC = value.Value.ToUniversalTime();
                    this.m_LastUpdated = toUTC;
                    NotifyPropertyChangedAsync("LastUpdated");
                }
                else
                {
                    this.m_LastUpdated = null;
                    NotifyPropertyChangedAsync("LastUpdated");
                }
            }
        }

        [Column]
        public int ForumPageNumber { get; set; }
        
        [Column]
        public int NewPostCount
        {
            get
            {
                return this.m_newPostCount;
            }
            set
            {
                if (this.m_newPostCount == value) return;
                NotifyPropertyChangingAsync("NewPostCount");
                this.m_newPostCount = value;
                NotifyPropertyChangedAsync("NewPostCount");
            }
        }

        [Association(
            Storage = "_pages",
            ThisKey = AWFUL_THREAD_PRIMARY_KEY,
            OtherKey = AwfulThreadPage.AWFUL_THREAD_FOREIGN_KEY,
            IsForeignKey = false)]
        internal EntitySet<AwfulThreadPage> _Pages
        {
            get { return this._pages; }
            set { this._pages.Assign(value); }
        }

        public IList<AwfulThreadPage> Pages
        {
            get { return this._Pages; }
        }

        private EntitySet<AwfulThreadBookmark> _threadBookmarks;
        [Association(
            Storage = "_threadBookmarks",
            ThisKey = AWFUL_THREAD_PRIMARY_KEY,
            OtherKey = AwfulThreadBookmark.AWFUL_THREAD_BOOKMARK_THREAD_FOREIGN_KEY)]
        public EntitySet<AwfulThreadBookmark> ThreadBookmarks
        {
            get { return this._threadBookmarks; }
        }

        #endregion

        public string Url 
        {
            get { return string.Format("http://forums.somethingawful.com/showthread.php?threadid={0}", this.ID); }
        }
        
        public bool HasViewedToday
        {
            get { return this.m_threadViewedToday; }
            set { this.m_threadViewedToday = value; NotifyPropertyChangedAsync("HasViewedToday"); }
        }

        public HtmlNode Node { get; set; }
        
        public bool ShowPostCount
        {
            get { return m_showPostCount; }
            set
            {
                m_showPostCount = value;
                NotifyPropertyChangedAsync("ShowPostCount");
            }
        }
        
        #endregion

        #endregion

        public AwfulThread(HtmlNode node) 
        {
            this.Node = node;
            this.TotalPages = 1;
            this._pages = new EntitySet<AwfulThreadPage>(this.OnPageAdd, this.OnPageRemove);
            this._threadBookmarks = new EntitySet<AwfulThreadBookmark>(this.OnBookmarkAdd, this.OnBookmarkRemove);
            this._forum = new EntityRef<AwfulForum>();
        }

        public AwfulThread() : this(null) {  }

        private void OnPageAdd(AwfulThreadPage page) { page._Parent = this; }
        private void OnPageRemove(AwfulThreadPage page) { page._Parent = null; }
        private void OnBookmarkAdd(AwfulThreadBookmark mark) { mark.Thread = this; }
        private void OnBookmarkRemove(AwfulThreadBookmark mark) { mark.Thread = null; }

        public bool HasBeenReadByUser
        {
            get { return this.LastRead; }
        }

        public bool HasNoNewPosts() { return this.NewPostCount == int.MaxValue; }

        public void AddToBookmarks()
        {
            throw new NotImplementedException();
        }

        public void RemoveFromBookmarks()
        {
            throw new NotImplementedException();
        }

        public void MarkThreadAsReadByUser()
        {
            throw new NotImplementedException();
        }

        public void MarkThreadAsNewByUser()
        {
            throw new NotImplementedException();
        }

        public void BeginNewThreadRequest(Action<NewThreadRequest> action)
        {
            throw new NotImplementedException();
        }

        public void FilterThreadByPostAuthorAsync(PostData post, Action<ThreadData> action)
        {
            throw new NotImplementedException();
        }

        public void MarkAsUnreadAsync(Action<ActionResult> action)
        {
            throw new NotImplementedException();
        }


        public ThreadPageData GetThreadPage(int pageNumber)
        {
            if (pageNumber < -1) throw new ArgumentOutOfRangeException("Page number is out of range.");
            
            ThreadPageData page = null;
            int index = -1;
            index = Math.Min(pageNumber, this.TotalPages);
            index = Math.Max(-1, index);
            page = new AwfulThreadPage(this, index);
            return page;
        }

        internal void Update(AwfulThread thread)
        {
            if (this.ID != thread.ID) { throw new Exception("Cannot update thread with a different thread id."); }
            else
            {
                this.Title = thread.Title;
                this.Rating = thread.Rating;
                this.Replies = thread.Replies;
                this.NewPostCount = thread.NewPostCount;
                this.Author = thread.Author;
                this.TotalPages = thread.TotalPages;
                this.LastRead = thread.LastRead;
                this.IsSticky = thread.IsSticky;
                this.ForumPageNumber = thread.ForumPageNumber;
                this.LastUpdated = DateTime.Now;
            }
        }
    }
}
