using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using System.Collections;

namespace Awful.Models
{
    public interface Browsable
    {
        string Url { get; }
        string Html { get; }
    }
    public interface Refreshable
    {
        DateTime? LastUpdated { get; }
    }

    public interface Cancellable
    {
        void CancelAsync();
    }

    public interface ForumData
    {
        string ForumName { get; set; }
        int ID { get; set; }
        bool IsFavorite { get; set; }
        int MaxPages { get; set; }
        int LastViewedPage { get; set; }
    }
    public interface ThreadData
    {
        int ID { get; }
        string ThreadTitle { get; }
        string AuthorName { get; }
        bool ThreadSeen { get; }
        int NewPostCount { get; set; }
        int MaxPages { get; set; }
        int LastViewedPageIndex { get; set; }
        int LastViewedPostIndex { get; set; }
        int LastViewedPostID { get; set; }
    }

    public enum AccountType { Normal = 0, Admin = 1, Moderator = 2 }

    public interface PostData : Browsable
    {
        string PostIconUri { get; set; }
        string PostAuthor { get; set; }
        DateTime PostDate { get; set; }
        int PostIndex { get; set; }
    }
   
    public interface ForumPage : Browsable, Refreshable
    {
        int ForumID { get; }
        int PageNumber { get; }
        IList<ThreadData> Threads { get; }
    }

    public interface ThreadPage : Browsable, Refreshable
    {
        int ThreadID { get; }
        ThreadData Thread { get; }
        string ThreadTitle { get; }
        int PageNumber { get; }
        IList<PostData> Posts { get; }
    }
      
    public interface IThreadDataService
    {
        void GetThreadPageAsync(ThreadData thread, Action<Awful.Core.Models.ActionResult, ThreadPage> page, int pageNumber);
        void ClearSeenPostsAsync(ThreadData thread, Action<Awful.Core.Models.ActionResult> result);
    }
}
