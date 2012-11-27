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
using KollaSoft;
using System.Collections.Generic;
using Awful.Core.Event;

namespace Awful.Core.Models
{
    [Table]
    public class AwfulForum : PropertyChangedBase, ForumData, IEquatable<AwfulForum>, IComparable<ForumData>
    {
        public const string AWFUL_FORUM_PRIMARY_KEY = "ID";
        public const string AWFUL_SUBFORUM_FOREIGN_KEY = "SubforumID";

        public static readonly AwfulForum Empty;
        public static readonly IComparer<ForumData> DEFAULT_COMPARER = new SortForumByName();

        static AwfulForum() { Empty = new AwfulForum() { ID = -1, ForumName = "EMPTY" }; }

        [Column(IsVersion = true)]
        private Binary _version;

        [Column]
        private string _subtitle;
        public string Subtitle 
        {
            get { return this._subtitle; }
            set
            {
                if (this._subtitle == value) return;
                this.NotifyPropertyChangingAsync("Subtitle");
                this._subtitle = value;
                this.NotifyPropertyChangedAsync("Subtitle");
            }
        }

        private string _forumName;
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
                this._forumName = value; 
                NotifyPropertyChangedAsync("ForumName"); 
            }
        }

        private int _forumID;
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

        private EntityRef<AwfulSubforum> _subforum;
        [Association(
            Storage = "_subforum", 
            ThisKey = AWFUL_SUBFORUM_FOREIGN_KEY, 
            OtherKey = AwfulSubforum.AWFUL_SUBFORUM_PRIMARY_KEY, 
            IsForeignKey = true)]
        public AwfulSubforum Subforum
        {
            get { return this._subforum.Entity; }
            set
            {
                AwfulSubforum prev = this._subforum.Entity;
                if ((prev != value) || (this._subforum.HasLoadedOrAssignedValue == false))
                {
                    NotifyPropertyChangingAsync("Subforum");

                    if (prev != null)
                    {
                        this._subforum.Entity = null;
                        prev.Forums.Remove(this);
                    }

                    this._subforum.Entity = value;

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

        private EntitySet<AwfulForumFavorite> _forumFavorites;
        [Association(
            Storage = "_forumFavorites",
            IsForeignKey = false,
            ThisKey = AWFUL_FORUM_PRIMARY_KEY,
            OtherKey = AwfulForumFavorite.AWFUL_FORUMFAVORITE_FORUM_FOREIGNKEY)]
        public EntitySet<AwfulForumFavorite> ForumFavorites
        {
            get { return this._forumFavorites; }
            set { this._forumFavorites.Assign(value); }
        }

        private int _totalPages;
        public int TotalPages
        {
            get { return this._totalPages; }
            set { this._totalPages = value; NotifyPropertyChangedAsync("MaxPages"); }
        }

        public override string ToString()
        {
            return this.ForumName;
        }

        public bool Equals(AwfulForum other)
        {
            return this.ID == other.ID;
        }

        public override bool Equals(object obj)
        {
            bool equals = false;
            
            if (obj is AwfulForum) { equals = this.Equals(obj as AwfulForum); }
            
            return equals;
        }

        public override int GetHashCode()
        {
            return this.ID.GetHashCode();
        }

        public AwfulForum()
        {
            this._subforum = new EntityRef<AwfulSubforum>();
            this._forumFavorites = new EntitySet<AwfulForumFavorite>(this.OnForumFavoriteAdded, this.OnForumFavoriteRemoved);
        }

        private void OnForumFavoriteAdded(AwfulForumFavorite favorite)
        {
            favorite.Forum = this;
        }

        private void OnForumFavoriteRemoved(AwfulForumFavorite favorite)
        {
            favorite.Forum = null;
        }

        public int CompareTo(ForumData other)
        {
            return DEFAULT_COMPARER.Compare(this, other);
        }

        public virtual ForumPageData GetForumPage(int pageNumber)
        {
            return new AwfulForumPage(this, pageNumber);
        }
    }
}
