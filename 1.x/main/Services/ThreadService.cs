using Awful.Models;
using System;
using System.Threading;
using System.Windows;
using System.Net;
using System.IO;
using Awful.Helpers;

namespace Awful.Services
{
    public interface ThreadService
    {
        void AddBookmarkAsync(ThreadData data, Action<Awful.Core.Models.ActionResult> result);
        void RemoveBookmarkAsync(ThreadData data, Action<Awful.Core.Models.ActionResult> result);
        void RateThreadAsync(ThreadData data, int rating, Action<Awful.Core.Models.ActionResult> result);
        void ReplyAsync(ThreadData data, string message, Action<Awful.Core.Models.ActionResult> result);
        Cancellable GetQuoteAsync(string postID, Action<Awful.Core.Models.ActionResult, string> result);
        void RunURLTaskAsync(string url, Action<Awful.Core.Models.ActionResult> result);
    }

    public class SomethingAwfulThreadService : ThreadService
    {
        // static variables
        private static readonly SomethingAwfulThreadService instance = new SomethingAwfulThreadService();
        public static SomethingAwfulThreadService Current { get { return instance; } }

        // fields
        private readonly ThreadReplyService replySvc = new ThreadReplyService();
        private readonly ThreadBookmarkService bookmarkSvc = new ThreadBookmarkService();

        private SomethingAwfulThreadService() { }
       
        public void MarkThreadToPostAsync(SAPost post, Action<Awful.Core.Models.ActionResult> result)
        {
            if (Awful.Core.Event.Logger.IsEnabled)
            {
                Awful.Core.Event.Logger.AddEntry(
                    string.Format("MarkThreadToPost - PostID: {0}", post.ID));
            }

            var markUrl = post.MarkSeenUrl;
            RunURLTaskAsync(markUrl, result);
        }

        public void AddBookmarkAsync(ThreadData data, Action<Awful.Core.Models.ActionResult> result)
        {
            bookmarkSvc.ToggleBookmarkAsync(data, BookmarkAction.Add, result);
        }

        public void RemoveBookmarkAsync(ThreadData data, Action<Awful.Core.Models.ActionResult> result)
        {
            bookmarkSvc.ToggleBookmarkAsync(data, BookmarkAction.Remove, result);
        }

        public void GetEditPostTextAsync(string postID, Action<Awful.Core.Models.ActionResult, string> result)
        {
            replySvc.GetEdit(postID, result);
        }

        public void SendEditPostTextAsync(string postID, string text, Action<Awful.Core.Models.ActionResult> result)
        {
            replySvc.SendEditAsync(postID, text, result);
        }

        public void RateThreadAsync(ThreadData data, int rating, Action<Awful.Core.Models.ActionResult> result)
        {
            var url = string.Format("http://forums.somethingawful.com/threadrate.php?vote={0}&threadid={1}",
                rating, data.ID);

            RunURLTaskAsync(url, result);
        }

        public void ReplyAsync(ThreadData data, string message, Action<Awful.Core.Models.ActionResult> result)
        {
            replySvc.ReplyAsync(data, message, result);
        }

        public Cancellable GetQuoteAsync(string postID, Action<Awful.Core.Models.ActionResult, string> result)
        {
            replySvc.GetQuote(postID, result);
            return replySvc;
        }

        public void GetEditTextAsync(string postID, Action<Awful.Core.Models.ActionResult, string> result)
        {
            replySvc.GetEdit(postID, result);
        }

        public void SendEditTextAsync(string postID, string text, Action<Awful.Core.Models.ActionResult> result)
        {
            replySvc.SendEditAsync(postID, text, result);
        }

        public void ClearMarkedPostsAsync(ThreadData thread, Action<Awful.Core.Models.ActionResult> result)
        {
            AsyncCallback getResponse = (callback) =>
                {
                    try
                    {
                        HttpWebRequest request = callback.AsyncState as HttpWebRequest;
                        HttpWebResponse response = request.EndGetResponse(callback) as HttpWebResponse;
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                           {
                               if (response.StatusCode == HttpStatusCode.OK)
                                   result(Awful.Core.Models.ActionResult.Success);
                               else
                                   result(Awful.Core.Models.ActionResult.Failure);
                           });
                    }

                    catch (Exception ex)
                    {
                        Awful.Core.Event.Logger.AddEntry(string.Format("ClearMarkedPosts: An error occured. [{0}] - {1}", ex.Message, ex.StackTrace));
                        Deployment.Current.Dispatcher.BeginInvoke(() => { result(Awful.Core.Models.ActionResult.Failure); });
                    }
                };

            AsyncCallback getRequest = (callback) =>
                {
                    HttpWebRequest request = callback.AsyncState as HttpWebRequest;
                    using (StreamWriter writer = new StreamWriter(request.EndGetRequestStream(callback)))
                    {
                        string postData = string.Format("json=1&action=resetseen&threadid={0}", thread.ID.ToString());
                        writer.Write(postData);
                        writer.Close();
                    }
                    request.BeginGetResponse(getResponse, request);
                };

            ThreadPool.QueueUserWorkItem(state =>
                {
                    HttpWebRequest request = Awful.Core.Web.AwfulWebRequest.CreatePostRequest("http://forums.somethingawful.com/showthread.php");
                    request.BeginGetRequestStream(getRequest, request);
                }, 
                null);
        }

        public void RunURLTaskAsync(string url, Action<Awful.Core.Models.ActionResult> result)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback((state) =>
            {
                var web = new Awful.Core.Web.WebGet();
                var dispatch = Deployment.Current.Dispatcher;
                var signal = new AutoResetEvent(false);
                bool success = false;
                web.LoadAsync(url, (obj, args) =>
                {
                    if (args.Document != null)
                        success = true;

                    signal.Set();
                });

                signal.WaitOne(App.Settings.ThreadTimeout);

                if (success)
                {
                    dispatch.BeginInvoke(() => {result(Awful.Core.Models.ActionResult.Success); });
                }
                else
                {
                    dispatch.BeginInvoke(() => { result(Awful.Core.Models.ActionResult.Failure); });
                }
            }), null);
        }
    }
}
