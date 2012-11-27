// -----------------------------------------------------------------------
// <copyright file="PostData.cs" company="">
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
    public interface PostData
    {
        string PostIconUri { get; set; }
        string PostAuthor { get; set; }
        AccountType AccountType { get; set; }
        DateTime PostDate { get; set; }
        int PostIndex { get; set; }
        void BeginQuoteRequest(Action<ThreadReplyRequest> action);
        void BeginEditRequest(Action<PostEditRequest> action);
        void MarkLastReadAsync(Action<ActionResult> action);
        void IgnorePostAuthorAsync(Action<ActionResult> action);
        void ReportAsync(Action<ActionResult> action);
        bool HasSeen { get; }
    }

    public enum AccountType
    {
        NORMAL=0,
        MODERATOR,
        ADMIN
    }

    public interface PostEditRequest
    {
        string Message { get; set; }
        void SendRequestAsync(Action<ActionResult> action);
    }
}
