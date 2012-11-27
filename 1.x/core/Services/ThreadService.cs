using System;
using System.Threading;
using System.Windows;
using System.Net;
using System.IO;
using Awful.Core.Web;
using Awful.Core.Models;
using Awful.Core.Event;

namespace Awful.Services
{
    public interface ThreadService
    {
        void AddBookmarkAsync(ThreadData data, Action<ActionResult> result);
        void RemoveBookmarkAsync(ThreadData data, Action<ActionResult> result);
        void RateThreadAsync(ThreadData data, int rating, Action<ActionResult> result);
        void ReplyAsync(ThreadData data, string message, Action<ActionResult> result);
        void GetQuoteAsync(string postID, Action<ActionResult, string> result);
        void RunURLTaskAsync(string url, Action<ActionResult> result);
    }

    public class SomethingAwfulThreadService : ThreadService
    {
        // static variables
        private static readonly SomethingAwfulThreadService instance = new SomethingAwfulThreadService(DEFAULT_TIMEOUT);
        public static SomethingAwfulThreadService Current { get { return instance; } }

        // fields
        private readonly ThreadReplyService replySvc = new ThreadReplyService();
        private readonly ThreadBookmarkService bookmarkSvc = new ThreadBookmarkService();
        private int _defaultServiceTimeout;
        private const int DEFAULT_TIMEOUT = 10000;

        private SomethingAwfulThreadService(int timeout) { this._defaultServiceTimeout = timeout; }

        public void MarkThreadToPostAsync(AwfulPost post, Action<ActionResult> result)
        {
            if (Logger.IsEnabled)
            {
                Logger.AddEntry(string.Format("MarkThreadToPost - PostID: {0}", post.ID));
            }

            var markUrl = post.MarkSeenUrl;
            RunURLTaskAsync(markUrl, result);
        }

        public void AddBookmarkAsync(ThreadData data, Action<ActionResult> result)
        {
            bookmarkSvc.ToggleBookmarkAsync(data, BookmarkAction.Add, result);
        }

        public void RemoveBookmarkAsync(ThreadData data, Action<ActionResult> result)
        {
            bookmarkSvc.ToggleBookmarkAsync(data, BookmarkAction.Remove, result);
        }

        public void GetEditPostTextAsync(string postID, Action<ActionResult, string> result)
        {
            replySvc.GetEdit(postID, result);
        }

        public void SendEditPostTextAsync(string postID, string text, Action<ActionResult> result)
        {
            replySvc.SendEditAsync(postID, text, result);
        }

        public void RateThreadAsync(ThreadData data, int rating, Action<ActionResult> result)
        {
            var url = string.Format("http://forums.somethingawful.com/threadrate.php?vote={0}&threadid={1}",
                rating, data.ID);

            RunURLTaskAsync(url, result);
        }

        public void ReplyAsync(ThreadData data, string message, Action<ActionResult> result)
        {
            replySvc.ReplyAsync(data, message, result);
        }

        public void GetQuoteAsync(string postID, Action<ActionResult, string> result)
        {
            replySvc.GetQuote(postID, result);
        }

        public void GetEditTextAsync(string postID, Action<ActionResult, string> result)
        {
            replySvc.GetEdit(postID, result);
        }

        public void SendEditTextAsync(string postID, string text, Action<ActionResult> result)
        {
            replySvc.SendEditAsync(postID, text, result);
        }

        public void ClearMarkedPostsAsync(ThreadData thread, Action<ActionResult> result)
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
                                   result(ActionResult.Success);
                               else
                                   result(ActionResult.Failure);
                           });
                    }

                    catch (Exception ex)
                    {
                        Logger.AddEntry("ClearMarkedPosts: An error occured:", ex);
                        Deployment.Current.Dispatcher.BeginInvoke(() => { result(ActionResult.Failure); });
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
                    HttpWebRequest request = AwfulWebRequest.CreateFormDataPostRequest("http://forums.somethingawful.com/showthread.php", "application/x-www-form-urlencoded");
                    request.BeginGetRequestStream(getRequest, request);
                }, 
                null);
        }

        public void RunURLTaskAsync(string url, Action<ActionResult> result)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback((state) =>
            {
                var web = new WebGet();
                var dispatch = Deployment.Current.Dispatcher;
                var signal = new AutoResetEvent(false);
                bool success = false;
                web.LoadAsync(url, (obj, args) =>
                {
                    if (args.Document != null)
                        success = true;

                    signal.Set();
                });

                signal.WaitOne(this._defaultServiceTimeout);

                if (success)
                {
                    dispatch.BeginInvoke(() => {result(ActionResult.Success); });
                }
                else
                {
                    dispatch.BeginInvoke(() => { result(ActionResult.Failure); });
                }
            }), null);
        }
    }
}
