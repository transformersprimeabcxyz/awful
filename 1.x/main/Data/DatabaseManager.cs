using System;
using System.Linq;
using Microsoft.Phone.Data.Linq;
using System.Data.Linq;
using Awful.Models;
using System.Threading;
using Awful.Services;
using Awful.Helpers;
using System.Collections.Generic;
using Awful.Commands;
using Awful.Core.Web;
using Awful.Core.Models;

namespace Awful.Data
{
    public class DatabaseManager
    {
        static DatabaseManager() { BindEvents(); }

        #region Event Handling of DB Functions

        private static void BindEvents()
        {
            AwfulAuthenticator.LoginSuccessful += 
                new EventHandler<Awful.Core.Event.ProfileChangedEventArgs>(Authenticator_LoginSuccessful);
            ThreadView.ThreadSelected += new EventHandler<ValueEventArgs<SAThread>>(OnThreadSelected);
            
            //SAThreadPageFactory.PageCreated += new EventHandler(OnThreadPageCreated);
            //SAThreadPage.PageGenerated += new EventHandler<Helpers.HtmlGeneratedEventArgs>(OnThreadPageGenerated);
            
            ToggleFavoritesCommand.FavoriteAdded += new EventHandler(OnFavoriteAdded);
            ToggleFavoritesCommand.FavoriteRemoved += new EventHandler(OnFavoriteRemoved);
            ForumDataRequest.ForumsRefreshed += new EventHandler<Helpers.ForumsRefreshedEventArgs<SAForum>>(OnForumListRefreshed);
        }

        static void Authenticator_LoginSuccessful(object sender, Awful.Core.Event.ProfileChangedEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(state =>
                {
                    try
                    {
                        OnProfileChanged(sender, e);
                        using (var db = new SAForumDB())
                        {
                            var count = db.Smilies.Count();
                            if (count == 0)
                            {
                                var signal = new AutoResetEvent(false);
                                Services.AwfulSmileyService.FetchSmiliesFromWebAsync((result, list) =>
                                    {
                                        if (result == Awful.Core.Models.ActionResult.Success)
                                        {
                                            db.Smilies.InsertAllOnSubmit(list);
                                        }
                                        signal.Set();
                                    });

                                signal.WaitOne();
                                db.SubmitChanges(ConflictMode.FailOnFirstConflict);
                            }
                        }
                    }

                    catch (Exception ex)
                    {
                        string error = string.Format("An error occured while updating the smiley database: [{0}] {1}", ex.Message, ex.StackTrace);
                        Awful.Core.Event.Logger.AddEntry(error);
                    }

                }, null);
        }

        static void OnFavoriteAdded(object sender, EventArgs e)
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                try
                {
                    SAForum forum = sender as SAForum;
                    using (var db = new SAForumDB())
                    {
                        Profile current = db.Profiles.SingleOrDefault(profile => profile.ID == App.Settings.CurrentProfileID);
                        if (current == null) throw new ArgumentException("Current profile can not be null.");

                        var query = from fav in db.ForumFavorites
                                    where fav.ProfileID == current.ID && fav.ForumID == forum.ID
                                    select fav;

                        if (query.Count() == 0)
                        {
                            ForumFavorite favorite = new ForumFavorite() { ForumID = forum.ID, ProfileID = current.ID, IsFavorite = true };
                            db.ForumFavorites.InsertOnSubmit(favorite);
                            db.SubmitChanges(ConflictMode.FailOnFirstConflict);
                        }
                    }
                }
                catch (Exception ex)
                {
                    string error = string.Format("An error occured while trying to modify the Favorites DB: [{0}] {1}", ex.Message, ex.StackTrace);
                    Awful.Core.Event.Logger.AddEntry(error);
                }

            }, null);
        }

        static void OnFavoriteRemoved(object sender, EventArgs e)
        {
            ThreadPool.QueueUserWorkItem(state =>
                {
                    try
                    {
                        SAForum forum = sender as SAForum;
                        using (var db = new SAForumDB())
                        {
                            Profile current = db.Profiles.SingleOrDefault(profile => profile.ID == App.Settings.CurrentProfileID);
                            if (current == null) throw new ArgumentException("Current profile can not be null.");

                            var query = from f in db.ForumFavorites
                                        where f.ProfileID == current.ID && f.ForumID == forum.ID
                                        select f;

                            ForumFavorite toRemove = query.SingleOrDefault();
                            if (toRemove != null) 
                            {
                                toRemove.IsFavorite = false;
                                db.SubmitChanges(ConflictMode.FailOnFirstConflict);
                            }
                            
                        }
                    }

                    catch (Exception ex)
                    {
                        string error = string.Format("An error occurred while updating the favorites DB: [{0}] {1}", ex.Message, ex.StackTrace);
                        Awful.Core.Event.Logger.AddEntry(error);
                    }

                }, null);
        }

        static void OnThreadSelected(object sender, ValueEventArgs<SAThread> e)
        {
            if (e.Value == null) return;

            ThreadPool.QueueUserWorkItem(state =>
            {
                try
                {
                    using (var db = new SAForumDB())
                    {
                        var query = from t in db.Threads
                                    where t.ID == e.Value.ID
                                    select t;

                        var selected = query.SingleOrDefault();
                        if (selected == null)
                        {
                            db.Threads.InsertOnSubmit(e.Value);
                            db.SubmitChanges(ConflictMode.FailOnFirstConflict);
                        }
                    }
                }

                catch (Exception ex)
                {
                    string error = string.Format("An error occurred while updating the Thread DB: [{0}] {1}", ex.Message, ex.StackTrace);
                    Awful.Core.Event.Logger.AddEntry(error);
                }

            }, null);
        }

        static void OnForumPageUpdated(object sender, CollectionUpdatedEventArgs<Awful.Models.ThreadData> e)
        {
            SAForumPage page = sender as SAForumPage;
            if (page == null) return;

            ThreadPool.QueueUserWorkItem(state =>
                {
                    try
                    {
                        using (var db = new SAForumDB())
                        {
                            foreach (var item in e.Collection)
                            {
                                var thread = item as SAThread;
                                if (thread != null)
                                {
                                    var query = from t in db.Threads
                                                where t.ID == thread.ID
                                                select t;

                                    var single = query.SingleOrDefault();

                                    if (single == null) { db.Threads.InsertOnSubmit(thread); }
                                }
                            }

                            db.SubmitChanges(ConflictMode.FailOnFirstConflict);
                        }
                    }

                    catch (Exception ex)
                    {
                        string error = string.Format("An error occurred while updating the Thread DB: [{0}] {1}", ex.Message, ex.StackTrace);
                        Awful.Core.Event.Logger.AddEntry(error);
                    }

                }, null);
        }

        static void OnThreadPageCreated(object sender, EventArgs e)
        {
            SAThreadPage page = sender as SAThreadPage;
            if (page == null) return;

            ThreadPool.QueueUserWorkItem(state =>
                {
                    try
                    {
                        using (var db = new SAForumDB())
                        {
                            var query = from p in db.ThreadPages
                                        where (p.ThreadID == page.ThreadID) && (p.PageNumber == page.PageNumber)
                                        select p;

                            var single = query.SingleOrDefault();

                            if (single == null)
                            {
                                db.ThreadPages.InsertOnSubmit(page);
                            }

                            db.SubmitChanges(ConflictMode.FailOnFirstConflict);
                        }
                    }

                    catch (Exception ex)
                    {
                        string error = string.Format("An error occurred while updating the ThreadPage DB: [{0}] {1}", ex.Message, ex.StackTrace);
                        Awful.Core.Event.Logger.AddEntry(error);
                    }

                }, null);
        }

        static void OnThreadUpdated(object sender, EventArgs e)
        {
            SAThread thread = sender as SAThread;
            if (thread == null) return;

            ThreadPool.QueueUserWorkItem(state =>
                {
                    try
                    {
                        using (var db = new SAForumDB())
                        {
                            var query = from t in db.Threads
                                        where t.ID == thread.ID
                                        select t;

                            var selected = query.SingleOrDefault();

                            if (selected == null)
                            {
                                db.Threads.InsertOnSubmit(thread);
                            }

                            db.SubmitChanges(ConflictMode.FailOnFirstConflict);
                        }
                    }
                    catch (Exception ex)
                    {
                        string error = string.Format("An error occured while trying to modify the Thread DB: [{0}] - {1}", ex.Message, ex.StackTrace);
                        Awful.Core.Event.Logger.AddEntry(error);
                    }
                }, 
                null);
        }

        static void OnThreadPageGenerated(object sender, Helpers.HtmlGeneratedEventArgs e)
        {
            SAThreadPage page = sender as SAThreadPage;
            if (page == null) return;

            ThreadPool.QueueUserWorkItem(state => 
            {
                try 
                {
                    using (var db = new SAForumDB())
                    {
                        db.SubmitChanges(ConflictMode.FailOnFirstConflict);
                    }
                }

                catch (Exception ex)
                {
                    string error = string.Format("An error occured while modifying the Thread Page DB: [{0}] - {1}", 
                        ex.Message, ex.StackTrace);

                    Awful.Core.Event.Logger.AddEntry(error);
                }

            }, null);
        }

        private static void OnProfileChanged(object sender, Awful.Core.Event.ProfileChangedEventArgs e)
        {
            try
            {
                using (var db = new SAForumDB())
                {
                    var profile = db.Profiles.Where(p => p.Username.Equals(e.Value.Username)).SingleOrDefault();
                    if (profile == null)
                    {
                        profile = new Profile() { Username = e.Value.Username, Password = e.Value.Password };
                        profile.AssignTokens(e.Cookies);
                        db.Profiles.InsertOnSubmit(profile);
                        db.SubmitChanges(ConflictMode.FailOnFirstConflict);
                        App.Settings.CurrentProfileID = profile.ID;
                    }
                    else
                    {
                        App.Settings.CurrentProfileID = profile.ID;
                        App.CurrentUser = profile.Username;
                        foreach (var cookie in e.Cookies)
                        {
                            var token = new SAAuthToken(cookie) { Profile = profile };
                            db.Tokens.InsertOnSubmit(token);
                        }

                        db.SubmitChanges(ConflictMode.FailOnFirstConflict);

                        var profile2 = db.Profiles.Where(p => p.ID == profile.ID).SingleOrDefault();
                    }
                }
            }

            catch (Exception ex)
            {
                string error = string.Format(
                    "There was an error while trying to save profile to DB. [{0}] {1}",
                    ex.Message,
                    ex.StackTrace);

                Awful.Core.Event.Logger.AddEntry(error);
            }
        }

        private static void OnForumListRefreshed(object sender, Helpers.ForumsRefreshedEventArgs<SAForum> e)
        {
            ThreadPool.QueueUserWorkItem(state =>
                {
                    try
                    {
                        using (var db = new SAForumDB())
                        {
                            foreach (var item in e.Forums)
                            {
                                var query = from forum in db.Forums
                                            where forum.ID == item.ID
                                            select forum;

                                var selected = query.SingleOrDefault();
                                if (selected == null) { db.Forums.InsertOnSubmit(item); }
                                else
                                {
                                    selected.ForumName = item.ForumName;
                                }
                            }

                            db.SubmitChanges(ConflictMode.FailOnFirstConflict);
                        }
                    }

                    catch (Exception ex) 
                    {
                        string error = string.Format("An error occured while trying to modify the Forums DB: [{0}] {1}", ex.Message, ex.StackTrace);
                        Awful.Core.Event.Logger.AddEntry(error);
                    }

                }, null);
        }

        #endregion

        public void ManageDatabase()
        {
            using (var db = new SAForumDB())
            {
                if (!db.DatabaseExists()) 
                { 
                    db.CreateDatabase();
                    this.SetDefaults(db);
                }

                // manage versions
                var schemaUpdater = db.CreateDatabaseSchemaUpdater();
                int version = schemaUpdater.DatabaseSchemaVersion;
                try
                {
                    switch (version)
                    {
                        case 1:
                            this.UpdateToVersionTwo(schemaUpdater);
                            break;
                    }
                }
                catch (Exception ex) { Awful.Core.Event.Logger.AddEntry("An error occurred while updating schema:", ex); }
            }
        }

        private void UpdateToVersionTwo(DatabaseSchemaUpdater updater)
        {
            updater.AddTable<SAAuthToken>();
            updater.AddAssociation<SAAuthToken>("Profile");
            updater.AddAssociation<Profile>("Tokens");
            updater.DatabaseSchemaVersion = 2;
            updater.Execute();
        }

        private void SetDefaults(SAForumDB db)
        {
            db.Profiles.InsertOnSubmit(SAForumDB.DefaultProfile);
            db.Subforums.InsertAllOnSubmit(SAForumDB.DefaultSubforums);
            db.SubmitChanges(ConflictMode.FailOnFirstConflict);
        }
    }
}
