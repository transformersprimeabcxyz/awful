using System;
using System.Runtime.Serialization;
using KollaSoft;
using System.Linq;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using Microsoft.Phone.Data.Linq;
using Microsoft.Phone.Data.Linq.Mapping;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace Awful.Core.Models
{
    [Table]
    [Index(Name = "INDEX_USERNAME", IsUnique = true, Columns = "Username ASC")]
    public class AwfulProfile : PropertyChangedBase, UserProfile
    {
        public const string AWFUL_PROFILE_PRIMARY_KEY = "ID";

        [Column(IsVersion = true)]
        private Binary _version;

        [Column(
            IsPrimaryKey = true, 
            IsDbGenerated = true, 
            DbType = "INT NOT NULL Identity", 
            CanBeNull = false, 
            AutoSync = AutoSync.OnInsert)]
        public int ID { get; set; }

        private string _username;
        [Column(CanBeNull = false)]
        public string Username
        {
            get { return this._username; }
            set
            {
                if (this._username == value) return;
                NotifyPropertyChangingAsync("Username");
                this._username = value;
                NotifyPropertyChangedAsync("Username");
            }
        }

        private string _password;
        [Column(CanBeNull = false)]
        public string Password
        {
            get { return this._password; }
            set
            {
                if (this._password == value) return;
                NotifyPropertyChangingAsync("Password");
                this._password = value;
                NotifyPropertyChangedAsync("Password");
            }
        }

        private DateTime? _lastBookmarkRefresh;
        [Column(CanBeNull = true)]
        public DateTime? LastBookmarkRefresh
        {
            get { return this._lastBookmarkRefresh; }
            set
            {
                if (this._lastBookmarkRefresh == value) return;
                this.NotifyPropertyChanging("LastBookmarkRefresh");
                if (value.HasValue) { this._lastBookmarkRefresh = value.Value.ToUniversalTime(); }
                else { this._lastBookmarkRefresh = null; }
                this.NotifyPropertyChanged("LastBookmarkRefresh");
            }
        }

        private EntitySet<AwfulAuthenticationToken> _tokensEntity;
        [Association(
            Storage = "_tokensEntity", 
            IsForeignKey = false, 
            ThisKey = AWFUL_PROFILE_PRIMARY_KEY, 
            OtherKey = AwfulAuthenticationToken.AWFUL_PROFILE_FOREIGN_KEY)]
        private EntitySet<AwfulAuthenticationToken> _Tokens
        {
            get { return this._tokensEntity; }
            set { this._tokensEntity.Assign(value); }
        }

        public ICollection<AwfulAuthenticationToken> Tokens { get { return this._Tokens; } }

        private EntitySet<AwfulThreadBookmark> _threadBookmarks;
        [Association(
            Storage = "_threadBookmarks",
            ThisKey = AWFUL_PROFILE_PRIMARY_KEY,
            OtherKey = AwfulThreadBookmark.AWFUL_THREAD_BOOKMARK_PROFILE_FOREIGN_KEY)]
        public EntitySet<AwfulThreadBookmark> ThreadBookmarks
        {
            get { return this._threadBookmarks; }
        }

        private EntitySet<AwfulForumFavorite> _forumFavorites;
        [Association(
            Storage = "_forumFavorites", 
            ThisKey = AWFUL_PROFILE_PRIMARY_KEY, 
            OtherKey = "ForumID")]
        public EntitySet<AwfulForumFavorite> ForumFavorites
        {
            get { return this._forumFavorites; }
        }

        public bool IsBanned
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsOnProbation
        {
            get { throw new NotImplementedException(); }
        }

        public AwfulProfile()
        {
            this._forumFavorites = new EntitySet<AwfulForumFavorite>(this.OnAddForumFavorite, this.OnRemoveForumFavorite);
            this._threadBookmarks = new EntitySet<AwfulThreadBookmark>(this.OnBookmarkAdd, this.OnBookmarkRemove);
            this._tokensEntity = new EntitySet<AwfulAuthenticationToken>(this.OnTokenAdd, this.OnTokenRemove);
        }

        private void OnTokenAdd(AwfulAuthenticationToken token) { token.AwfulProfile = this; }
        private void OnTokenRemove(AwfulAuthenticationToken token) { token.AwfulProfile = null; }
        private void OnBookmarkAdd(AwfulThreadBookmark mark) { mark.Profile = this; }
        private void OnBookmarkRemove(AwfulThreadBookmark mark) 
        { 
            mark.Profile = null; 
            mark.Thread = null; 
        }

        private void OnAddForumFavorite(AwfulForumFavorite favorite)
        {
            favorite.Profile = this;
        }

        private void OnRemoveForumFavorite(AwfulForumFavorite favorite)
        {
            favorite.Profile = null;
        }

        public bool IsOriginalPosterOfThread(ThreadData thread)
        {
            return thread.Author == this.Username;
        }

        public void AddThreadToBookmarks(AwfulThread thread)
        {
            foreach (var mark in this.ThreadBookmarks)
            {
                if (mark.Thread.ID == thread.ID) return;
            }

            this.ThreadBookmarks.Add(new AwfulThreadBookmark() { Profile = this, Thread = thread });
        }

        public void RemoveThreadFromBookmarks(AwfulThread thread)
        {
            var toRemove = this.ThreadBookmarks.Where(t => t.Thread.ID == thread.ID).SingleOrDefault();
            if (toRemove != null) { this.ThreadBookmarks.Remove(toRemove); }
        }

        public void MarkThreadAsUnread(ThreadData thread)
        {
            throw new NotImplementedException();
        }

        public IList<AwfulForum> GetForumFavorites()
        {
            var favorites = this.ForumFavorites
                .Select(f => f.Forum)
                .ToList();

            return favorites;
        }

        public void AddForumToFavorites(ForumData forum)
        {
            AwfulForumFavorite favorite = new AwfulForumFavorite();
            favorite.Profile = this;
            favorite.Forum = forum as AwfulForum;
            this.ForumFavorites.Add(favorite);
        }

        public void RemoveForumFromFavorites(ForumData forum)
        {
            var favorite = from f in this.ForumFavorites
                           where f.Forum == forum
                           select f;

            this.ForumFavorites.Remove(favorite.SingleOrDefault());
        }

        public void MarkPostAsReadAsync(PostData post)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<System.Net.Cookie> GetTokensAsCookies()
        {
            List<System.Net.Cookie> cookies = new List<System.Net.Cookie>();
            foreach (var token in this.Tokens)
            { 
                cookies.Add(token.AsCookie());
            }
            return cookies;
        }


        public bool IsPlatinumUser
        {
            get { throw new NotImplementedException(); }
        }

        public void GetUserBookmarksAsync(Action<IList<ThreadData>> action)
        {
            throw new NotImplementedException();
        }

        public void AddThreadToBookmarksAsync(ThreadData thread, Action<ActionResult> action)
        {
            throw new NotImplementedException();
        }

        public void RemoveThreadFromBookmarksAsync(ThreadData thread, Action<ActionResult> action)
        {
            throw new NotImplementedException();
        }

        internal void AddRangeToBookmarks(IEnumerable<AwfulThread> iList)
        {
            foreach (var item in iList) { this.AddThreadToBookmarks(item); }
        }
    }
}
