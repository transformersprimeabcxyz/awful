using System;
using Awful.Models;
using System.Collections.Generic;

namespace Awful.Helpers
{
    public class ForumPageCache : Dictionary<int, SAForumPage>
    {
        private readonly SAForum _Forum;
        public SAForum Forum { get { return this._Forum; } }
        public ForumPageCache(SAForum Forum)
            : base()
        {
            this._Forum = Forum;
        }
    }

    public class ForumCache : Dictionary<int, ForumPageCache>
    {
        private static readonly ForumCache Cache = new ForumCache();
        private ForumCache() : base() { }

        public static SAForumPage GetPageFromCache(SAForum Forum, int pageNumber)
        {
            int ForumID = Forum.ID;
            SAForumPage page = null;
            ForumPageCache pageCache = null;
            if (Cache.TryGetValue(ForumID, out pageCache))
            {
                pageCache.TryGetValue(pageNumber, out page);
            }

            return page;
        }

        public static void AddPageToCache(SAForumPage page)
        {
            int ForumID = page.ForumID;
            if (Cache.ContainsKey(ForumID))
            {
                var pCache = Cache[ForumID];
                if (pCache.ContainsKey(page.PageNumber)) { pCache[page.PageNumber] = page; }
                else { pCache.Add(page.PageNumber, page); }
            }
            else
            {
                var pCache = new ForumPageCache(page.Parent as SAForum);
                pCache.Add(page.PageNumber, page);
                Cache.Add(ForumID, pCache);
            }
        }
    }
}
