using System;
using System.Collections.Generic;
using Awful.Core.Models;
using Awful.Core.Event;

namespace Awful.Core.Models.Sort
{
    public class SortThreadsByNewPostCount : IComparer<ThreadData>, IComparer<AwfulThread>
    {
        public static SortThreadsByNewPostCount Comparer;
        static SortThreadsByNewPostCount() { Comparer = new SortThreadsByNewPostCount(); }
        private SortThreadsByNewPostCount() { }

        public int Compare(ThreadData x, ThreadData y)
        {
            return this.Compare(x, y);
        }

        public int Compare(AwfulThread x, AwfulThread y)
        {
            int postXPoints = x.NewPostCount;
            int postYPoints = y.NewPostCount;

            if (!x.HasBeenReadByUser) { postXPoints += 1000000; }
            if (!y.HasBeenReadByUser) { postYPoints += 1000000; }

            Logger.AddEntry(string.Format("AwfulForumPage - Sorting -> ThreadID {0} score: {1}; ThreadID {2} score: {3}",
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

