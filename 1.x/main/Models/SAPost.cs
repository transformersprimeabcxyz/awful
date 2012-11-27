using System;
using System.Linq;
using System.Data.Linq.Mapping;
using Microsoft.Phone.Data.Linq;
using Microsoft.Phone.Data.Linq.Mapping;
using System.ComponentModel;
using System.Windows.Controls;
using Awful.Helpers;
using System.Collections.Generic;
using HtmlAgilityPack;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Media;
using System.Net;
using System.Windows;
using KollaSoft;

namespace Awful.Models
{
    [Table (Name = "Posts")]
    public class SAPost : PropertyChangedBase, PostData
    {
        public const int NULL_POSTID = -1;

        #region Fields

        private string markSeenUrl;
        private int m_postId;
        private string _icon;
        private int m_index;
        private string _author = String.Empty;
        private DateTime _date;
        private bool _hasSeen;
        private string _url;
        private string _html;
        private HtmlNode _contentNode;
        private int _threadPageID;
        private bool _displayIcon;
        private bool _isActive;
        private int m_AccountType;
       
        #endregion

        #region Properties

        #region Database Fields

        [Column(IsPrimaryKey = true)]
        public int ID
        {
            get { return m_postId; }
            set { m_postId = value; }
        }

        [Column]
        public int ThreadPageID
        {
            get { return this._threadPageID; }
            set { this._threadPageID = value; }
        }

        [Column]
        private int AccountTypeID
        {
            get { return this.m_AccountType; }
            set
            {
                if (this.m_AccountType == value) return;
                NotifyPropertyChanging("AccountTypeID");
                this.m_AccountType = value;
                NotifyPropertyChanged("AccountTypeID");
            }
        }

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
        public string MarkSeenUrl
        {
            get { return this.markSeenUrl; }
            set
            {
                NotifyPropertyChangingAsync("MarkSeenUrl");
                this.markSeenUrl = value;
                NotifyPropertyChangedAsync("MarkSeenUrl");
            }
        }

        [Column]
        public string PostIconUri
        {
            get { return this._icon; }
            set
            {
                NotifyPropertyChangingAsync("PostIcon");
                _icon = value;
                NotifyPropertyChangedAsync("PostIcon");
            }
        }

        [Column(CanBeNull = false)]
        public string PostAuthor
        {
            get { return this._author; }
            set
            {
                if (this._author == value) return;
                NotifyPropertyChangingAsync("PostAuthor");
                this._author = value;
                NotifyPropertyChangedAsync("PostAuthor");
            }
        }

        [Column]
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

        public int UserID
        {
            get;
            set;
        }

        #endregion

        #region Non Database

        public HtmlNode ContentNode
        {
            get { return this._contentNode; }
            set { this._contentNode = value; }
        }

        public bool IsEditable
        {
            get
            {
                var username = App.CurrentUser;
                return username.Equals(PostAuthor);
            }
        }

        public AccountType AccountType
        {
            get { return (AccountType)this.AccountTypeID; }
            set 
            {
                this.AccountTypeID = (int)value;
                NotifyPropertyChanged("AccountType");
            }
        }

        public string PostNumber
        {
            get { return string.Format("#{0}", m_index); }
        }

        public bool ShowPostIcon
        {
            get { return _displayIcon; }
            set { _displayIcon = value; }
        }

        public string Url
        {
            get { return _url; }
        }

        public string Html
        {
            get { return _html; }
        }

        public bool HasSeen
        {
            get { return _hasSeen; }
            set { _hasSeen = value; NotifyPropertyChangedAsync("HasSeen"); }
        }

        #endregion

        #endregion

        public SAPost() { }
    }
}
