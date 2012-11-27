using System;
using System.Collections.Generic;

namespace Awful.Core.Models
{
    public class SortForumByName : IComparer<ForumData>
    {
        public int Compare(ForumData x, ForumData y)
        {
            string xName = x.ForumName.ToLower();
            string yName = y.ForumName.ToLower();

            return xName.CompareTo(yName);
        }
    }
}
