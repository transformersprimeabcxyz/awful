using System;
using System.Linq;
using Awful.Core.Models;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Awful.Core.Event;
using KollaSoft;

namespace Awful.Core.Database
{
    public class AwfulProfileDAO : IDisposable
    {
        private AwfulDataContext _context;
        private bool _isDataContextDisposable;

        private AwfulProfileDAO(AwfulDataContext context, Boolean isDisposable)
        {
            this._context = context;
            this._isDataContextDisposable = isDisposable;
        }

        public AwfulProfileDAO(AwfulDataContext context) : this(context, false) { }
        public AwfulProfileDAO() : this(new AwfulDataContext(), true) { }

        public void Dispose()
        {
            if (this._isDataContextDisposable) { this._context.Dispose(); }
            this._context = null;
        }
        
        public AwfulProfile GetProfileByID(int id)
        {
            var query = this._context.Profiles.Where(profile => profile.ID == id);
            var result = query.SingleOrDefault();
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public AwfulProfile GetProfileByUsername(string username)
        {
            var query = this._context.Profiles.Where(profile => profile.Username == username);
            var result = query.SingleOrDefault();
            return result;
        }

        /// <summary>
        /// Creates a new profile and persists it to the database.
        /// </summary>
        /// <param name="username">The profile's username.</param>
        /// <param name="password">The profile's password.</param>
        /// <param name="cookies">Session cookies provided by SA's servers.</param>
        /// <returns>A persisted UserProfile; null if the persistence fails.</returns>
        public AwfulProfile CreateProfile(string username, string password, IList<Cookie> cookies)
        {
            AwfulProfile profile = null;
            try
            {
                profile = new AwfulProfile();
                profile.Username = username;
                profile.Password = password;
                foreach (var cookie in cookies)
                {
                    var token = new AwfulAuthenticationToken(cookie);
                    profile.Tokens.Add(token);
                }
                // insert and submit changes to database
                this._context.Profiles.InsertOnSubmit(profile);
                this._context.SubmitChanges();
            }
            catch (Exception ex) 
            { 
                Event.Logger.AddEntry("An error occured while creating a profile:", ex);
                profile = null;
            }
            return profile;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="cookies"></param>
        /// <returns></returns>
        public AwfulProfile SaveAuthenticationCookiesToProfile(AwfulProfile profile, IList<Cookie> cookies)
        {
            AwfulProfile profileToUpdate = null;
            try
            {
                profileToUpdate = this.GetProfileByUsername(profile.Username);
                if (profileToUpdate != null)
                {
                    foreach (var cookie in cookies)
                    {
                        var token = new AwfulAuthenticationToken(cookie);
                        profileToUpdate.Tokens.Add(token);
                    }
                }

                this._context.SubmitChanges();
            }
            catch (Exception ex)
            {
                Event.Logger.AddEntry("Error while saving cookies to profile:", ex);
                profileToUpdate = null;
            }
            return profileToUpdate;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        public AwfulProfile RenameProfile(AwfulProfile profile, string username)
        {
            var query = this._context.Profiles.Where(p => p.ID == profile.ID);
            var profileToUpdate = query.SingleOrDefault();
            if (profileToUpdate != null)
            {
                try
                {
                    profileToUpdate.Username = username;
                    this._context.SubmitChanges(System.Data.Linq.ConflictMode.FailOnFirstConflict);
                }
                catch (Exception ex)
                {
                    Event.Logger.AddEntry("An error occured while renaming the profile:", ex);
                    profileToUpdate = null;
                }
            }
            return profileToUpdate;
        }

        public AwfulProfile UpdateProfileForumFavorites(AwfulProfile profile, ICollection<AwfulForumFavorite> favorites)
        {
            AwfulProfile result = null;
            try
            {
                result = this.GetProfileByUsername(profile.Username);
                var forums = new List<AwfulForum>(favorites.Count);
                foreach (var item in favorites)
                {
                    var forum = this._context.Forums.Where(f => f.ID == item.Forum.ID).Single();
                    if (forum != null) forums.Add(forum);
                }

                var forumFavorites = forums.Select(f => new AwfulForumFavorite() { Profile = result, Forum = f });

                if (result != null)
                {
                    result.ForumFavorites.Clear();
                    result.ForumFavorites.AddRange(forumFavorites);
                    this._context.SubmitChanges();
                }
            }

            catch (Exception ex) {
                result = null;
                Logger.AddEntry("An error occurred while updating forum favorites:", ex); 
            }

            return result;
        }

        public void UpdateUserBookmarksAsync(AwfulProfile user, bool refresh, Action<AwfulProfile> result)
        {
            // check database first
            var bookmarks = new List<AwfulThreadBookmark>();
            if (!refresh)
            {
                user = this.GetProfileByUsername(user.Username);
                bookmarks.AddRange(user.ThreadBookmarks);
            }
            if (bookmarks.IsNullOrEmpty())
            {
                // fetch from web
                AwfulForumPage cp = new AwfulControlPanel().GetBookmarks();
                cp.RefreshAsync(cpRefreshed =>
                    {
                        using (var dao = new AwfulProfileDAO())
                        {
                            var profile = dao.SaveBookmarksToProfile(user, cpRefreshed.Threads);
                            if (profile == null)
                            {
                                // if we're here, then there's a database issue, but the show must go on
                                using (var dao2 = new AwfulProfileDAO())
                                {
                                    user = dao2.GetProfileByUsername(user.Username);
                                    user.AddRangeToBookmarks(cpRefreshed.Threads);
                                    result(user);
                                }
                            }
                            else { result(profile); }
                        }
                    });
            }
            else
            {
                result(user);
            }
        }

        private AwfulProfile SaveBookmarksToProfile(AwfulProfile user, IList<AwfulThread> threads)
        {
            AwfulProfile result = null;

            try
            {
                user = this.GetProfileByUsername(user.Username);
                if (user != null)
                {
                    this.RemoveUserBookmarks(user);
                    user = this.GetProfileByUsername(user.Username);
                    
                    foreach (var thread in threads)
                    {
                        AwfulThread threadInDB = this._context.Threads.Where(t => t.ID == thread.ID).SingleOrDefault();
                        if (threadInDB != null) { user.AddToBookmarks(threadInDB); }
                        else { user.AddToBookmarks(thread); }
                    }
                    user.LastBookmarkRefresh = DateTime.Now;
                    this._context.SubmitChanges();
                    result = user;
                }
            }
            catch (Exception ex)
            {
                Logger.AddEntry("An error occurred while saving bookmarks to profile:", ex);
                result = null;
            }

            return result;
        }

        private void RemoveUserBookmarks(AwfulProfile user)
        {
            var bookmarks = this._context.ThreadBookmarks.Where(mark => mark.Profile.ID == user.ID).ToList();
            this._context.ThreadBookmarks.DeleteAllOnSubmit(bookmarks);
            this._context.SubmitChanges();
            user.ThreadBookmarks.Clear();
            this._context.SubmitChanges();
        }
    }
}
