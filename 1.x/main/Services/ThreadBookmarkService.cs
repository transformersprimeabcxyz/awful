using System;
using System.Threading;
using System.Net;
using Awful.Models;
using System.IO;
using System.Windows;
using Awful.Helpers;

namespace Awful.Services
{
    public enum BookmarkAction { Add, Remove }
    public class ThreadBookmarkService
    {
        public ThreadBookmarkService() { }

        public void ToggleBookmarkAsync(ThreadData thread, BookmarkAction action, Action<Awful.Core.Models.ActionResult> result)
        {
            Awful.Core.Event.Logger.AddEntry(string.Format("ToggleBookmarkAsync - ThreadID: {0}, Action: {1}", thread.ID, action));
            
            string url = String.Format("{0}/{1}", Globals.Constants.SA_BASE, Globals.Constants.BOOKMARK_THREAD);

            Awful.Core.Event.Logger.AddEntry(string.Format("ToggleBookmarkAsync - Bookmark url: {0}", url));
            
            AutoResetEvent bookmarkSignal = new AutoResetEvent(false);
            bool bookmarkSuccess = false;

            Action<IAsyncResult> ProccessBookmarkGetRequestStream = null;
            Action<IAsyncResult> ProccessBookmarkBeginGetResponse = null;
            ProccessBookmarkGetRequestStream = (asyncResult) =>
            {
                try
                { 
                    Awful.Core.Event.Logger.AddEntry("ToggleBookmarkAsync - Initializingweb request...");
                   
                    HttpWebRequest request = asyncResult.AsyncState as HttpWebRequest;
                    StreamWriter writer = new StreamWriter(request.EndGetRequestStream(asyncResult));
                    var postData = String.Format("{0}&{1}={2}",
                        action == BookmarkAction.Add ? Globals.Constants.ADD_BOOKMARK : Globals.Constants.REMOVE_BOOKMARK,
                        Globals.Constants.THREAD_ID,
                        thread.ID);

                    Awful.Core.Event.Logger.AddEntry(string.Format("ToggleBookmarkAsync - PostData: {0}", postData));
                    
                    writer.Write(postData);
                    writer.Close();
                    bookmarkSuccess = true;
                    
                    Awful.Core.Event.Logger.AddEntry("ToggleBookmarkAsync - Web request initialization successful.");
                }

                catch (Exception) 
                {
                    Awful.Core.Event.Logger.AddEntry("ToggleBookmarkAsync - Web request initialization failed.");
                    bookmarkSuccess = false; 
                }
                bookmarkSignal.Set();
            };

            ProccessBookmarkBeginGetResponse = (asyncResult) =>
            {
                Awful.Core.Event.Logger.AddEntry("ToggleBookmarkAsync - Start Get Response...");
                HttpWebRequest request = asyncResult.AsyncState as HttpWebRequest;
                HttpWebResponse response = request.EndGetResponse(asyncResult) as HttpWebResponse;
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Awful.Core.Event.Logger.AddEntry("ToggleBookmarkAsync - Start Get Response successful.");
                    Awful.Core.Event.Logger.AddEntry(string.Format("ToggleBookmarkAsync - Repsonse url: {0}", response.ResponseUri));
                    bookmarkSuccess = true;
                }
                else
                {
                    Awful.Core.Event.Logger.AddEntry("ToggleBookmarkAsync - Start Get Response failed.");
                    bookmarkSuccess = false;
                }

                bookmarkSignal.Set();
            };

            ThreadPool.QueueUserWorkItem(new WaitCallback((state) =>
            {
                var request = state as HttpWebRequest;
                var callback = new AsyncCallback(ProccessBookmarkGetRequestStream);
                var dispatch = Deployment.Current.Dispatcher;

                request.BeginGetRequestStream(callback, request);
                bookmarkSignal.WaitOne();
                if (!bookmarkSuccess)
                {
                    dispatch.BeginInvoke(() => { result(Awful.Core.Models.ActionResult.Failure); });
                    return;
                }
                callback = new AsyncCallback(ProccessBookmarkBeginGetResponse);
                request.BeginGetResponse(callback, request);
                bookmarkSignal.WaitOne();
                if (!bookmarkSuccess)
                {
                    dispatch.BeginInvoke(() => { result(Awful.Core.Models.ActionResult.Failure); });
                    return;
                }

                dispatch.BeginInvoke(() => { result(Awful.Core.Models.ActionResult.Success); });
                return;
            }),
                InitializePostRequest(url));
        }

        private HttpWebRequest InitializePostRequest(string url)
        {
            HttpWebRequest request = Awful.Core.Web.AwfulWebRequest.CreatePostRequest(url);
            return request;
        }
    }
}
