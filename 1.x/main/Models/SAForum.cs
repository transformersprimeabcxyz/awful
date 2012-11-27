using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Input;
using HtmlAgilityPack;
using System.Linq;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using Microsoft.Phone.Data.Linq;
using Microsoft.Phone.Data.Linq.Mapping;
using Awful.Helpers;
using KollaSoft;
using Awful.Data;
using System.Collections.Generic;

namespace Awful.Models
{
    public enum SACategory
    {
        Main, Discussion, TheFinerArts, TheCommunity
    }

    [Table (Name = "Forums")]
    public class SAForum : PropertyChangedBase, ForumData, IEquatable<SAForum>
    {    
        private string _forumName;
        private bool _isFavorite;
        private int _forumID;
        private int _maxPages = 1;
        private int _lastViewedPage = 0;
        private EntityRef<Subforum> _Subforum;

        public static readonly SAForum Empty;

        static SAForum() { Empty = new SAForum(); }

        [Column(IsVersion = true)]
        private Binary _version;

        [Column (CanBeNull = false)]
        public string ForumName
        {
            get 
            {
                return _forumName == null 
                    ? "ForumID: " + this.ID
                    : this._forumName;
            }

            set 
            {
                if (this._forumName == value) return;
                NotifyPropertyChangingAsync("ForumName");
                this._forumName = ContentFilter.Censor(value);
                NotifyPropertyChangedAsync("ForumName"); 
            }
        }

        [Column(IsPrimaryKey = true)]
        public int ID
        {
            get { return _forumID; }
            set 
            {
                if (this._forumID == value) return;
                NotifyPropertyChangingAsync("ForumID");
                this._forumID = value; 
                NotifyPropertyChangedAsync("ForumID"); 
            }
        }

        [Column]
        private int SubforumID { get; set; }

        [Association(Storage = "_Subforum", ThisKey = "SubforumID", OtherKey = "ID", IsForeignKey = true)]
        public Subforum Subforum
        {
            get { return this._Subforum.Entity; }
            set
            {
                Subforum prev = this._Subforum.Entity;
                if ((prev != value) || (this._Subforum.HasLoadedOrAssignedValue == false))
                {
                    NotifyPropertyChangingAsync("Subforum");

                    if (prev != null)
                    {
                        this._Subforum.Entity = null;
                        prev.Forums.Remove(this);
                    }

                    this._Subforum.Entity = value;

                    if (value != null)
                    {
                        value.Forums.Add(this);
                        this.SubforumID = value.ID;
                    }

                    else
                    {
                        this.SubforumID = default(int);
                    }

                    NotifyPropertyChangedAsync("Subforum");
                }
            }
        }

        public bool IsFavorite
        {
            get { return this._isFavorite; }
            set
            {
                if (this._isFavorite == value) return;
                var args = new ForumFavoriteChangedEventArgs(this, this._isFavorite, value);
                this._isFavorite = value;
                NotifyPropertyChangedAsync("IsFavorite");
            }
        }

        public int MaxPages
        {
            get { return this._maxPages; }
            set { this._maxPages = value; NotifyPropertyChangedAsync("MaxPages"); }
        }

        public int LastViewedPage
        {
            get { return this._lastViewedPage; }
            set
            {
                if (this._lastViewedPage == value) return;
                NotifyPropertyChangingAsync("LastViewedPage");
                this._lastViewedPage = value;
                NotifyPropertyChangedAsync("LastViewedPage");
            }
        }

        public SAForum() { }

        public override string ToString()
        {
            return this.ForumName;
        }

        public bool Equals(SAForum other)
        {
            return this.ID == other.ID;
        }

        public override bool Equals(object obj)
        {
            bool equals = false;
            
            if (obj is SAForum) { equals = this.Equals(obj as SAForum); }
            
            return equals;
        }

        public override int GetHashCode()
        {
            return this.ID.GetHashCode();
        }
    }

    public class SortForumByName : IComparer<SAForum>
    {
        public int Compare(SAForum x, SAForum y)
        {
            string xName = x.ForumName.ToLower();
            string yName = y.ForumName.ToLower();

            return xName.CompareTo(yName);
        }
    }

}
