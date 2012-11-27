using System;
using System.Collections.Generic;
using Awful.Models;

namespace Awful.Helpers
{
    public class ThreadPageCache : Dictionary<int, SAThreadPage>
    {
        private readonly SAThread _thread;
        public SAThread Thread { get { return this._thread; } }
        public ThreadPageCache(SAThread thread) : base()
        {
            this._thread = thread;
        }
    }

    public class ThreadCache : Dictionary<int, ThreadPageCache>
    {
        private static readonly ThreadCache Cache = new ThreadCache();
        private ThreadCache() : base() { }

        public static SAThreadPage GetPageFromCache(SAThread thread, int pageNumber)
        {
            int threadID = thread.ID;
            SAThreadPage page = null;
            ThreadPageCache pageCache = null;
            if (Cache.TryGetValue(threadID, out pageCache))
            {
                pageCache.TryGetValue(pageNumber, out page);
            }

            return page;
        }

        public static void AddPageToCache(SAThreadPage page)
        {
            int threadID = page.ThreadID;
            if (threadID < 1) return;

            if (Cache.ContainsKey(threadID))
            {
                var pCache = Cache[threadID];
                if (pCache.ContainsKey(page.PageNumber)) { pCache[page.PageNumber] = page; }
                else { pCache.Add(page.PageNumber, page); }
            }
            else
            {
                var pCache = new ThreadPageCache(page.Thread);
                pCache.Add(page.PageNumber, page);
                Cache.Add(threadID, pCache);
            }
        }
    }
}
