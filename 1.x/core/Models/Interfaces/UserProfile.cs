// -----------------------------------------------------------------------
// <copyright file="Profile.cs" company="">
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
    public interface UserProfile
    {
        int ID { get; }
        string Username { get; }
        bool IsBanned { get; }
        bool IsOnProbation { get; }
        bool IsPlatinumUser { get; }
        bool IsOriginalPosterOfThread(ThreadData thread);

        void GetUserBookmarksAsync(Action<IList<ThreadData>> action);
        void AddThreadToBookmarksAsync(ThreadData thread, Action<ActionResult> action);
        void RemoveThreadFromBookmarksAsync(ThreadData thread, Action<ActionResult> action);
        
        void AddForumToFavorites(ForumData forum);
        void RemoveForumFromFavorites(ForumData forum);
    }
}
