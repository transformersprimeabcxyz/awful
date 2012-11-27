using System;
using System.Linq;
using System.Data.Linq.Mapping;
using Microsoft.Phone.Data.Linq;
using Microsoft.Phone.Data.Linq.Mapping;
using System.Collections.Generic;
using HtmlAgilityPack;
using System.Net;
using System.Windows;
using KollaSoft;
using System.Data.Linq;

namespace Awful.Core.Models
{
    [Table]
    public class AwfulPost : PropertyChangedBase, PostData
    {
        public const int NULL_POSTID = -1;
        public const string AWFUL_THREADPAGE_FOREIGN_KEY = "ThreadPageID";
        public const int UNKNOWN_POST_INDEX = -1;

        #region Fields

        private int m_index;
        private string _author = String.Empty;
        private DateTime _date;
        private bool _hasSeen;       
        #endregion

        #region Properties

        #region Database Fields

        [Column(IsVersion = true)]
        private Binary _version;
        
        [Column(IsPrimaryKey = true)]
        public int ID { get; set; }

        [Column]
        public int ThreadPageID { get; set; }

        [Column]
        private int AccountTypeID { get; set; }

        [Column]
        public int PostIndex
        {
            get { return this.m_index; }
            set
            {
                if (this.m_index == value) return;
                NotifyPropertyChanging("PostIndex");
                this.m_index = value;
                NotifyPropertyChanged("PostIndex");
                NotifyPropertyChangedAsync("PostNumber");
            }
        }

        [Column]
        public string PostIconUri { get; set; }

        [Column(CanBeNull = false)]
        public string PostAuthor { get; set; }

        [Column(CanBeNull = false)]
        public DateTime PostDate
        {
            get { return this._date.ToLocalTime(); }
            set
            {
                NotifyPropertyChangingAsync("PostDate");
                this._date = value.ToUniversalTime();
                NotifyPropertyChangedAsync("PostDate");
            }
        }

        [Column]
        public int UserID { get; set; }

        [Column]
        public int ThreadIndex { get; set; }

        private EntityRef<AwfulThreadPage> _threadPage;
        [Association(
            Storage = "_threadPage",
            ThisKey = AWFUL_THREADPAGE_FOREIGN_KEY,
            OtherKey = AwfulThreadPage.AWFUL_THREADPAGE_PRIMARY_KEY,
            IsForeignKey = true,
            DeleteOnNull = true)]
        public AwfulThreadPage ThreadPage
        {
            get { return this._threadPage.Entity; }
            set
            {
                AwfulThreadPage previous = this._threadPage.Entity;
                if ((previous != value) || (!this._threadPage.HasLoadedOrAssignedValue))
                {
                    if (previous != null)
                    {
                        this._threadPage.Entity = null;
                    }
                    this._threadPage.Entity = value;
                    if (value != null)
                    {
                        this.ThreadPageID = value.ID;
                    }
                    else { this.ThreadPageID = -1; }
                }
            }
        }

        #endregion

        #region Non Database

        // TODO: Implement mark seen url
        public string MarkSeenUrl { get { return this.GenerateMarkSeenUrl(); } }

        public AccountType AccountType
        {
            get { return (AccountType)this.AccountTypeID; }
            set 
            {
                this.AccountTypeID = (int)value;
                NotifyPropertyChanged("AccountType");
            }
        }

        public HtmlNode ContentNode { get; set; }
        public string PostNumber{ get { return string.Format("#{0}", this.PostIndex); } }
        public bool ShowPostIcon { get; set; }
        public string Url
        {
            get
            {
                // TODO: Automatically generate Url based on threadID and threadPageID.
                return null;
            }
        }
        public bool HasSeen
        {
            get { return _hasSeen; }
            set { _hasSeen = value; NotifyPropertyChangedAsync("HasSeen"); }
        }

        #endregion

        #endregion

        public AwfulPost() 
        {
            this.PostDate = DateTime.Now;
            this._threadPage = new EntityRef<AwfulThreadPage>();
        }

        public void BeginQuoteRequest(Action<ThreadReplyRequest> action)
        {
            throw new NotImplementedException();
        }

        public void BeginEditRequest(Action<PostEditRequest> action)
        {
            throw new NotImplementedException();
        }

        public void MarkLastReadAsync(Action<ActionResult> action)
        {
            throw new NotImplementedException();
        }

        public void IgnorePostAuthorAsync(Action<ActionResult> action)
        {
            throw new NotImplementedException();
        }

        public void ReportAsync(Action<ActionResult> action)
        {
            throw new NotImplementedException();
        }

        private string GenerateMarkSeenUrl()
        {
            throw new NotImplementedException();
        }
    }
}
