// -----------------------------------------------------------------------
// <copyright file="Interface1.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Awful.Core.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public interface ForumData
    {
        string ForumName { get; }
        string Subtitle { get; }
        int ID { get; }
        int TotalPages { get; set; }
        ForumPageData GetForumPage(int pageNumber);
    }

    public interface ForumPageData : Interfaces.WebBrowseable
    {
        int ForumID { get; }
        ForumData Parent { get; }
        int PageNumber { get; }
        IList<ThreadData> Threads { get; }
    }

    public interface SubforumData<T> where T : ForumData
    {
        IList<T> Forums { get; }
        string Name { get; }
    }
}