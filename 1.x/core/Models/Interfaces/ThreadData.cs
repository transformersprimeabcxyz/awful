// -----------------------------------------------------------------------
// <copyright file="ThreadData.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Awful.Core.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.IO;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public interface ThreadData
    {
        int ID { get; }
        string Title { get; }
        string Author{ get; }
        int TotalPages { get; set; }
        int NewPostCount { get; set; }
        int ForumPageNumber { get; }
        bool HasBeenReadByUser { get; }
        bool HasNoNewPosts();
        void BeginNewThreadRequest(Action<NewThreadRequest> action);
        void FilterThreadByPostAuthorAsync(PostData post, Action<ThreadData> action);
        void MarkAsUnreadAsync(Action<ActionResult> action);
        ThreadPageData GetThreadPage(int pageNumber);
    }

    public interface ThreadPageData : Interfaces.Refreshable<ThreadPageData>, Interfaces.WebBrowseable
    {
        int ID { get; }
        ThreadData Parent { get; }
        int PageNumber { get; }
        IList<PostData> Posts { get; }
        string Html { get; }
    }

    public interface ThreadTag
    {
        string Title { get; }
        string Value { get; }
        string IconUri { get; }
    }

    public interface NewThreadRequest
    {
        string Message { get; set; }
        ThreadTag SelectedTag { get; set; }
        void SendRequestAsync(Action<ActionResult> action);
    }

    public interface ThreadReplyRequest
    {
        string Message { get; set; }
        Stream Attachement { get; set; }
        void SendRequestAsync(Action<ActionResult> action);
    }
}
