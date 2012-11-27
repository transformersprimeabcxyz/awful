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
using Awful.Helpers;
using KollaSoft;
using HtmlAgilityPack;

namespace Awful.Models
{
    [DataContractAttribute]
    [Table(Name = "Threads")]
    public class SAThread : PropertyChangedBase, ThreadData
    {
        #region ThreadData Members

        #region Fields

        private int _id;
        private bool m_threadSeen;
        private bool m_threadViewedToday;
        private string m_threadTitle;
        private string m_authorName;
        private int m_newPostCount;
        private int m_maxPages;
        private int m_currentPage;
        private string m_threadURL;
        private int m_rating;
        private int m_replies;
        private bool m_showPostCount;
        private DateTime? m_LastUpdated;
        private EntitySet<SAThreadPage> _Pages;

        #endregion

        #region Properties

        [DataMember]
        public bool HasViewedToday
        {
            get { return this.m_threadViewedToday; }
            set { this.m_threadViewedToday = value; NotifyPropertyChangedAsync("HasViewedToday"); }
        }

        [IgnoreDataMember]
        public HtmlNode Node { get; set; }

        [IgnoreDataMember]
        [Column]
        public bool IsSticky { get; set; }

        [DataMember]
        [Column]
        public int LastViewedPageIndex { get; set; }
        
        [DataMember]
        [Column]
        public int LastViewedPostIndex { get; set; }

        [IgnoreDataMember]
        [Column]
        public int LastViewedPostID { get; set; }

        [DataMember]
        [Column]
        public int ForumID { get; set; }

        [DataMember]
        [Column(IsPrimaryKey = true)]
        public int ID
        {
            get { return this._id; }
            set 
            {
                if (this._id == value) return;
                NotifyPropertyChangingAsync("ID");
                this._id = value;
                NotifyPropertyChangedAsync("ID");
            }
        }

        [IgnoreDataMember]
        [Column]
        public bool ThreadSeen
        {
            get { return m_threadSeen; }
            set
            {
                if (m_threadSeen == value) return;
                NotifyPropertyChangingAsync("ThreadSeen");
                m_threadSeen = value;
                NotifyPropertyChangedAsync("ThreadSeen");
            }
        }
      
        [DataMember]
        [Column]
        public string ThreadTitle
        {
            get
            {
                return m_threadTitle;
            }
            set
            {
                if (this.m_threadTitle == value) return;
                NotifyPropertyChangingAsync("ThreadTitle");
                m_threadTitle = value;
                NotifyPropertyChangedAsync("ThreadTitle");
            }
        }

        [DataMember]
        [Column]
        public string AuthorName
        {
            get
            {
                return this.m_authorName;
            }
            set
            {
                if (this.m_authorName == value) return;
                NotifyPropertyChangingAsync("AuthorName");
                m_authorName = value;
                NotifyPropertyChangedAsync("AuthorName");
            }
        }

        [IgnoreDataMember]
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

        [DataMember]
        [Column]
        public int MaxPages
        {
            get { return this.m_maxPages; }
            set
            {
                if (value != m_maxPages)
                {
                    NotifyPropertyChangingAsync("MaxPages");
                    this.m_maxPages = value;
                    NotifyPropertyChangedAsync("MaxPages");
                }
            }
        }
       
        [IgnoreDataMember]
        public int CurrentPage
        {
            get
            {
                return m_currentPage;
            }
            set
            {
                NotifyPropertyChangingAsync("CurrentPage");
                m_currentPage = value;
                NotifyPropertyChangedAsync("CurrentPage");
            }
        }

        [DataMember]
        [Column(CanBeNull = false)]
        public string ThreadURL
        {
            get { return m_threadURL; }
            set 
            {
                NotifyPropertyChangingAsync("ThreadURL");
                this.m_threadURL = value; 
                NotifyPropertyChangedAsync("ThreadURL"); 
            }
        }

        [IgnoreDataMember]
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

        [IgnoreDataMember]
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

        [IgnoreDataMember]
        public bool ShowPostCount
        {
            get { return m_showPostCount; }
            set
            {
                m_showPostCount = value;
                NotifyPropertyChangedAsync("ShowPostCount");
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

        /*
        [Association(Name = "PK_Thread_ThreadPages", Storage = "_Pages", ThisKey = "ID", OtherKey = "ThreadID")]
        public EntitySet<SAThreadPage> Pages
        {
            get 
            {
                if (this._Pages == null)
                    this._Pages = new EntitySet<SAThreadPage>(this.OnPageAdd, this.OnPageRemove);

                return this._Pages; 
            }
            set { this._Pages.Assign(value); }
        }
        */

        #endregion

        #endregion

        public SAThread(HtmlNode node) 
        {
            this.Node = node;
            this.LastViewedPageIndex = 0; 
            this.LastViewedPostIndex = -1; 
            this.MaxPages = 1;
        }

        public SAThread() : this(null) {  }

        private void OnPageAdd(SAThreadPage page) { page.Thread = this; }
        private void OnPageRemove(SAThreadPage page) { page.Thread = null; }
    }
}
