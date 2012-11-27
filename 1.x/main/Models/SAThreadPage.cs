using System;
using System.Net;
using System.Linq;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using Microsoft.Phone.Data.Linq;
using Microsoft.Phone.Data.Linq.Mapping;
using Awful.Helpers;
using System.Collections.Generic;
using System.ComponentModel;

namespace Awful.Models
{
    [Table(Name = "ThreadPages")]
    [Index(Name = "Index_ThreadID_PageNumber", IsUnique = true, Columns = "ThreadID ASC, PageNumber DESC")]
    public class SAThreadPage : KollaSoft.PropertyChangedBase, ThreadPage
    {
        #region Fields
     
        private int m_id;
        private string m_url;
        private string m_html;
        private string m_title;
        private string m_htmlPath;
        private int m_page;
        private int m_max;
        private DateTime? m_LastUpdated;
        private SAThread _Thread;
        private IList<PostData> m_posts;
        private bool m_isLoaded;
        private int m_userID;

        #endregion

        public static event EventHandler<HtmlGeneratedEventArgs> PageGenerated;

        public SAThreadPage() { this._Thread = null; }

        public SAThreadPage(string url) : this()
        {
            m_url = url;
        }

        public SAThreadPage(SAThread data, int page, int userID = 0) : this()
        {
            this.ThreadID = data.ID;
            this.ThreadTitle = data.ThreadTitle;
            this.Thread = data;
            this.UserID = userID;
 
            if (page != 0)
            {
                this.PageNumber = page;
                this.Url = String.Format("http://forums.somethingawful.com/showthread.php?threadid={0}&userid={1}&perpage=40&pagenumber={2}",
                    this.ThreadID,
                    userID,
                    page);
            }

            else if (userID == 0)
            {
                this.PageNumber = 0;
                this.Url = String.Format("http://forums.somethingawful.com/showthread.php?threadid={0}&goto=newpost", m_id);
            }

            else
            {
                this.PageNumber = 0;
                this.Url = String.Format("http://forums.somethingawful.com/showthread.php?threadid={0}&userid={1}", m_id, userID);
            }
        }

        public SAThreadPage(SAThread data)
            : this(data, 0) { }

        #region Properties

        public int UserID
        {
            get { return this.m_userID; }
            set { this.m_userID = value; }
        }

        public string ThreadTitle
        {
            get { return m_title; }
            set { this.m_title = value; }
        }

        public SAThread Thread
        {
            get { return this._Thread; }
            set { this._Thread = value; NotifyPropertyChangedAsync("Thread"); }
        }

        [Column(IsVersion = true)]
        private Binary _version;

        [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL Identity", CanBeNull = false, AutoSync = AutoSync.Default)]
        public int ID { get; set; }
        
        [Column]
        public int ThreadID
        {
            get { return this.m_id; }
            set { this.m_id = value; }
        }

        [Column]
        public int PageNumber
        {
            get { return m_page; }
            set { m_page = value; }
        }      
 
        public IList<PostData> Posts
        {
            get { return m_posts; }
            set { this.m_posts = value; }
        }
        
        [Column]
        public string Url
        {
            get { return m_url; }
            set { this.m_url = value; }
        }
        
        [Column]
        public string HtmlPath
        {
            get { return this.m_htmlPath; }
            set { this.m_htmlPath = value; }
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
                    NotifyPropertyChangingAsync("LastUpdated");
                    this.m_LastUpdated = toUTC;
                    this._Html = null;
                    NotifyPropertyChangedAsync("LastUpdated");
                }
                else
                {
                    NotifyPropertyChangingAsync("LastUpdated");
                    this.m_LastUpdated = null;
                    NotifyPropertyChangedAsync("LastUpdated");
                }
            }
        }

        /*
        [Association(Name = "FK_ThreadPage_Thread", Storage = "_Thread", ThisKey = "ThreadID", OtherKey = "ID", IsForeignKey = true)]
        public SAThread Thread
        {
            get { return this._Thread.Entity; }
            set
            {
                SAThread prev = this._Thread.Entity;
                if ((prev != value) || (this._Thread.HasLoadedOrAssignedValue == false))
                {
                    NotifyPropertyChanging("Thread");

                    if (prev != null)
                    {
                        this._Thread.Entity = null;
                        prev.Pages.Remove(this);
                    }

                    this._Thread.Entity = value;

                    if (value != null)
                    {
                        value.Pages.Add(this);
                        this.ThreadID = value.ID;
                    }

                    else
                    {
                        this.ThreadID = default(int);
                    }

                    NotifyPropertyChanged("Thread");
                }
            }
        }
        */

        public string Html
        {
            get
            {
                if (this._Html == null)
                {
                    this._Html = GetFormattedHtml();
                }

                return this._Html;
            }
        }

        protected string _Html
        {
            get { return this.m_html; }
            set
            {
                if (this.m_html == value) return;
                this.m_html = value;
            }
        }
        
        public int MaxPages
        {
            get { return m_max; }
            set { this.m_max = value; }
        }

        public bool IsLoaded
        {
            get { return m_isLoaded; }
            set { m_isLoaded = value; }
        }

        #endregion

        private string GetFormattedHtml()
        {
            Awful.Core.Event.Logger.AddEntry("SAThreadPage - Thread content requested.");
            string content = null;
            content = KollaSoft.Extensions.LoadTextFromFile(this.HtmlPath);

            Awful.Core.Event.Logger.AddEntry("SAThreadPage - Thread content creation complete. Begin Output:");
            Awful.Core.Event.Logger.AddEntry("--------------------------------------------------------------");
            Awful.Core.Event.Logger.AddEntry(content);
            Awful.Core.Event.Logger.AddEntry("--------------------------------------------------------------");
            Awful.Core.Event.Logger.AddEntry("SAThreadPage - Thread content creation completed. End Output.");

            return content;
        }

        #region Explicitly Inherited Members

        ThreadData ThreadPage.Thread
        {
            get { return this.Thread; }
        }

        #endregion
    }
}
