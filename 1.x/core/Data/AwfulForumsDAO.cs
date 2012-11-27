using System;
using System.Linq;
using Awful.Core.Models;
using System.Collections.Generic;
using KollaSoft;
using Awful.Services;
using Awful.Core.Event;

namespace Awful.Core.Database
{
    public class AwfulForumsDAO : IDisposable
    {
        private AwfulDataContext _context;
        private bool _isDisposable;

        private static readonly AwfulForum NULL_FORUM = new AwfulForum() { ID = -1, ForumName = "NULL" };
        private static readonly AwfulThread NULL_THREAD = new AwfulThread() { ID = -1, Title = "NULL" };
        private static readonly AwfulThreadPage NULL_THREAD_PAGE = new AwfulThreadPage() { ID = -1, _Parent = NULL_THREAD, PageNumber = -1 };

        private AwfulForumsDAO(AwfulDataContext context, bool disposable)
        {
            this._context = context;
            this._isDisposable = disposable;
        }

        public AwfulForumsDAO(AwfulDataContext context) : this(context, false) { }
        public AwfulForumsDAO() : this(new AwfulDataContext(), true) { }

        public void Dispose()
        {
            if (this._isDisposable)
            {
                this._context.Dispose();
            }

            this._context = null;
        }

        public bool SetDatabaseToDefaults()
        {
            bool success = false;
            try
            {
                success = this.ClearAllForumData();
                this.CreateSubforumCache();
                this.CreateForumCache();
                this.CreateForumThreadCache();
                this.CreateThreadPostCache();
                this._context.SubmitChanges();               
                success = true;
            }
            catch (Exception ex)
            {
                Event.Logger.AddEntry("An error occured while staging the database:", ex);
                success = false;
            }
            return success;
        }

        public bool ClearAllForumData()
        {
            bool success = false;
            try
            {
                this._context.Posts.DeleteAllOnSubmit(this._context.Posts);
                this._context.ThreadPages.DeleteAllOnSubmit(this._context.ThreadPages);
                this._context.Threads.DeleteAllOnSubmit(this._context.Threads);
                this._context.Forums.DeleteAllOnSubmit(this._context.Forums);
                this._context.SubmitChanges();
                success = true; 
            }
            catch (Exception ex)
            {
                Event.Logger.AddEntry("An error occured while clearing forum data:", ex);
                success = false;
            }
            return success;
        }

        private void CreateThreadPostCache()
        { 
            // add null thread page
            this._context.Posts.DeleteAllOnSubmit(this._context.Posts);
            this._context.ThreadPages.DeleteAllOnSubmit(this._context.ThreadPages);
            this._context.ThreadPages.InsertOnSubmit(NULL_THREAD_PAGE); 
        }

        private void CreateForumThreadCache()
        {
            // add null thread
            this._context.Threads.DeleteAllOnSubmit(this._context.Threads);
            this._context.Threads.InsertOnSubmit(NULL_THREAD);
        }

        private void CreateForumCache()
        {
          // remove all forums
            this._context.Forums.DeleteAllOnSubmit(this._context.Forums);
            var nullForum = AwfulForum.Empty;
            this._context.Forums.InsertOnSubmit(nullForum);
        }

        private void CreateSubforumCache()
        {
            // add default subforums
            var subforums = AwfulSubforum.GenerateDefaultSubforums();
            this._context.Subforums.DeleteAllOnSubmit(this._context.Subforums);
            this._context.Subforums.InsertAllOnSubmit(subforums);
        }

        public AwfulThreadPage AddOrUpdateThreadPage(AwfulThreadPage page)
        {
            AwfulThreadPage pageToUpdate = null;
            try
            {
                var query = this._context.ThreadPages
                    .Where(p => p.ID == page.ID);

                pageToUpdate = query.SingleOrDefault();
                // page is null, so add it to database
                if (pageToUpdate == null) { this._context.ThreadPages.InsertOnSubmit(page); }
                else { pageToUpdate.Update(page); }
                this._context.SubmitChanges(System.Data.Linq.ConflictMode.FailOnFirstConflict);
            }
            catch (Exception ex)
            {
                Event.Logger.AddEntry("An error occurred while updating the thread page table:", ex);
                pageToUpdate = null;
            }

            return pageToUpdate;
        }

        public void FetchAllForumsAsync(Action<ICollection<ForumData>> result)
        {
            // fetch from db first
            List<ForumData> forums = new List<ForumData>();
            var dbForums = this._context.Forums;
            foreach(var item in dbForums) 
            {
                if (!item.Equals(AwfulForum.Empty)) 
                forums.Add(item); 
            }

            if (forums.IsNullOrEmpty())
            {
                // fetch from web
                AwfulForumsService.Service.FetchAllForums((actionResult, forumList) =>
                    {
                        forums.AddRange(forumList);
                        this.SaveForumsToDatabase(forums);
                        result(forums);
                    });
            }
            else { result(forums); }
        }

        private void SaveForumsToDatabase(ICollection<ForumData> forums)
        {
            try
            {
                foreach (var forum in forums)
                {
                    if (forum is AwfulForum) { this._context.Forums.InsertOnSubmit((AwfulForum)forum); }
                }

                this._context.SubmitChanges();
            }
            catch (Exception ex) { Logger.AddEntry("An error occurred while saving forums to database:", ex); }
        }

        public bool AddOrUpdateThreads(ICollection<AwfulThread> iCollection)
        {
            bool result = false;
            try
            {
                foreach (var item in iCollection) { this.AddOrUpdateThread(item); }
                this._context.SubmitChanges();
                result = true;
            }
            catch (Exception ex) 
            { 
                Logger.AddEntry("An error occurred while updating or adding threads to database:", ex);
                result = false;
            }
            return result;
        }

        private void AddOrUpdateThread(AwfulThread thread)
        {
            var exists = this._context.Threads.Where(t => t.ID == thread.ID).SingleOrDefault();
            if (exists != null)
            {
                exists.Update(thread);
            }
            else { this._context.Threads.InsertOnSubmit(thread); }
        }
    }
}
