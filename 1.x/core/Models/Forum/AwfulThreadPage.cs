using System;
using System.Net;
using System.Linq;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using Microsoft.Phone.Data.Linq;
using Microsoft.Phone.Data.Linq.Mapping;
using System.Collections.Generic;
using System.ComponentModel;
using Awful.Core.Event;
using KollaSoft;
using Awful.Services;

namespace Awful.Core.Models
{
    [Table]
    [Index(Name = "Index_ThreadID_PageNumber", IsUnique = true, Columns = "ThreadID ASC, PageNumber DESC")]
    public class AwfulThreadPage : PropertyChangedBase, ThreadPageData
    {
        public static event EventHandler<HtmlGeneratedEventArgs> PageRefreshed;
        public const string AWFUL_THREAD_FOREIGN_KEY = "ThreadID";
        public const string AWFUL_THREADPAGE_PRIMARY_KEY = "ID";
        
        public AwfulThreadPage()
        {
            this._parent = new EntityRef<AwfulThread>();
            this._postsEntity = new EntitySet<AwfulPost>(this.OnPostAdded, this.OnPostRemoved);
        }

        public AwfulThreadPage(AwfulThread parent, int page)
        {
            this._parent = new EntityRef<AwfulThread>();
            this._postsEntity = new EntitySet<AwfulPost>(this.OnPostAdded, this.OnPostRemoved);
            this._Parent = parent;
            this.PageNumber = page;
            this.Url = this.GenerateUrl(page);
        }

        public AwfulThreadPage(AwfulThread parent) : this(parent, -1) { }

        protected virtual string GenerateUrl(int pageNumber)
        {
            if (pageNumber < -1)
                throw new ArgumentOutOfRangeException("pageNumber must be a positive nonzero value or flagged as new (-1).");

            string value = null;
            
            if (pageNumber != -1)
            {
                value = string.Format("http://forums.somethingawful.com/showthread.php?threadid={0}&perpage=40&pagenumber={1}",
                    this.ThreadID,
                    pageNumber);
            }

            else
                value = string.Format("http://forums.somethingawful.com/showthread.php?threadid={0}&goto=newpost", this.ThreadID);

            return value;
        }

        #region Properties

        private EntityRef<AwfulThread> _parent;
        [Association(
            Storage = "_parent", 
            ThisKey = AWFUL_THREAD_FOREIGN_KEY, 
            OtherKey = AwfulThread.AWFUL_THREAD_PRIMARY_KEY,
            IsForeignKey = true,
            DeleteOnNull = true)]
        public AwfulThread _Parent
        {
            get { return this._parent.Entity; }
            set 
            {
                AwfulThread previous = this._parent.Entity;
                if ((previous != value) || (!this._parent.HasLoadedOrAssignedValue))
                {
                    if (previous != null)
                    {
                        this._parent.Entity = null;
                    }
                    this._parent.Entity = value;
                    if (value != null)
                    {
                        this.ThreadID = value.ID;
                    }
                    else { this.ThreadID = -1; }
                }
            }
        }

        public ThreadData Parent
        {
            get { return this._Parent; }
        }    

        [Column(IsVersion = true)]
        private Binary _version;

        [Column(
            IsPrimaryKey = true,
            DbType = "INT NOT NULL Identity",
            IsDbGenerated = true,
            AutoSync = AutoSync.Default)]
        public int ID { get; set; }

        [Column]
        public int ThreadID { get; set; }

        [Column]
        public int PageNumber { get; set; }

        private EntitySet<AwfulPost> _postsEntity;
        [Association(
            IsForeignKey = false,
            Storage = "_postsEntity",
            ThisKey = AWFUL_THREADPAGE_PRIMARY_KEY,
            OtherKey = AwfulPost.AWFUL_THREADPAGE_FOREIGN_KEY)]
        public EntitySet<AwfulPost> Posts
        {
            get { return this._postsEntity; }
            set { this._postsEntity.Assign(value); }
        }

        [Column]
        public string Url { get; set; }

        [Column]
        public string HtmlPath { get; set; }

        public string Html { get; set; }

        private DateTime? _lastUpdated;
        [Column(CanBeNull = true)]
        public DateTime? LastUpdated
        {
            get
            {
                if (this._lastUpdated.HasValue)
                {
                    DateTime value = this._lastUpdated.Value.ToLocalTime();
                    return value;
                }

                return null;
            }
            set
            {
                if (value.HasValue)
                {
                    DateTime toUTC = value.Value.ToUniversalTime();
                    NotifyPropertyChangingAsync("LastUpdated");
                    this._lastUpdated = toUTC;
                    NotifyPropertyChangedAsync("LastUpdated");
                }
                else
                {
                    NotifyPropertyChangingAsync("LastUpdated");
                    this._lastUpdated = null;
                    NotifyPropertyChangedAsync("LastUpdated");
                }
            }
        }

        IList<PostData> ThreadPageData.Posts
        {
            get
            {
                var posts = new List<PostData>();
                foreach (var item in this.Posts)
                    posts.Add(item);
                return posts;
            }
        }

        #endregion

        private void OnPostAdded(AwfulPost post) { post.ThreadPage = this; }
        private void OnPostRemoved(AwfulPost post) { post.ThreadPage = null; }

        public void RefreshAsync(Action<ThreadPageData> result)
        {
            AwfulForumsService.Service.RefreshThreadPage(this, result);
        }

        internal void Update(AwfulThreadPage page)
        {
            this._Parent = page._Parent;
            this.PageNumber = page.PageNumber;
            this.Posts.Clear();
            this.Posts.Assign(page.Posts);
            this.LastUpdated = DateTime.Now;
        }
    }
}
