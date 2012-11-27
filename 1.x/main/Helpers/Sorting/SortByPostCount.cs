using System;
using System.Collections.Generic;
using Awful.Models;

namespace Awful.Helpers
{
    public class SortThreadsByNewPostCount : IComparer<ThreadData>
    {
        public static SortThreadsByNewPostCount Comparer;
        static SortThreadsByNewPostCount() { Comparer = new SortThreadsByNewPostCount(); }
        private SortThreadsByNewPostCount() { }

        public int Compare(ThreadData x, ThreadData y)
        {
            int postXPoints = x.NewPostCount;
            int postYPoints = y.NewPostCount;

            if (!x.ThreadSeen) { postXPoints += 1000000; }
            if (!y.ThreadSeen) { postYPoints += 1000000; }

            Awful.Core.Event.Logger.AddEntry(string.Format("AwfulForumPage - Sorting -> ThreadID {0} score: {1}; ThreadID {2} score: {3}",
                x.ID, postXPoints,
                y.ID, postYPoints));

            if (postXPoints > postYPoints)
                return 1;

            if (postXPoints < postYPoints)
                return -1;

            return 0;
        }
    }
}
