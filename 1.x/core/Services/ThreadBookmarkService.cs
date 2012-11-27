using System;
using System.Threading;
using System.Net;
using System.IO;
using System.Windows;
using Awful.Core.Models;
using Awful.Core.Event;
using Awful.Core.Web;

namespace Awful.Services
{
    public enum BookmarkAction { Add, Remove }
    public class ThreadBookmarkService
    {
        public void ToggleBookmarkAsync(ThreadData thread, BookmarkAction action, Action<ActionResult> result)
        {
            Logger.AddEntry(string.Format("ToggleBookmarkAsync - ThreadID: {0}, Action: {1}", thread.ID, action));
            
            string url = String.Format("{0}/{1}", Constants.BASE_URL, Constants.BOOKMARK_THREAD_URI);

            Logger.AddEntry(string.Format("ToggleBookmarkAsync - Bookmark url: {0}", url));
            
            AutoResetEvent bookmarkSignal = new AutoResetEvent(false);
            bool bookmarkSuccess = false;

            Action<IAsyncResult> ProccessBookmarkGetRequestStream = null;
            Action<IAsyncResult> ProccessBookmarkBeginGetResponse = null;
            ProccessBookmarkGetRequestStream = (asyncResult) =>
            {
                try
                { 
                    Logger.AddEntry("ToggleBookmarkAsync - Initializingweb request...");
                   
                    HttpWebRequest request = asyncResult.AsyncState as HttpWebRequest;
                    StreamWriter writer = new StreamWriter(request.EndGetRequestStream(asyncResult));
                    var postData = String.Format("{0}&{1}={2}",
                        action == BookmarkAction.Add ? Constants.BOOKMARK_ADD : Constants.BOOKMARK_REMOVE,
                        Constants.THREAD_ID,
                        thread.ID);

                    Logger.AddEntry(string.Format("ToggleBookmarkAsync - PostData: {0}", postData));
                    
                    writer.Write(postData);
                    writer.Close();
                    bookmarkSuccess = true;
                    
                    Logger.AddEntry("ToggleBookmarkAsync - Web request initialization successful.");
                }

                catch (Exception) 
                {
                    Logger.AddEntry("ToggleBookmarkAsync - Web request initialization failed.");
                    bookmarkSuccess = false; 
                }
                bookmarkSignal.Set();
            };

            ProccessBookmarkBeginGetResponse = (asyncResult) =>
            {
                Logger.AddEntry("ToggleBookmarkAsync - Start Get Response...");
                HttpWebRequest request = asyncResult.AsyncState as HttpWebRequest;
                HttpWebResponse response = request.EndGetResponse(asyncResult) as HttpWebResponse;
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Logger.AddEntry("ToggleBookmarkAsync - Start Get Response successful.");
                    Logger.AddEntry(string.Format("ToggleBookmarkAsync - Repsonse url: {0}", response.ResponseUri));
                    bookmarkSuccess = true;
                }
                else
                {
                    Logger.AddEntry("ToggleBookmarkAsync - Start Get Response failed.");
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
                    dispatch.BeginInvoke(() => { result(ActionResult.Failure); });
                    return;
                }
                callback = new AsyncCallback(ProccessBookmarkBeginGetResponse);
                request.BeginGetResponse(callback, request);
                bookmarkSignal.WaitOne();
                if (!bookmarkSuccess)
                {
                    dispatch.BeginInvoke(() => { result(ActionResult.Failure); });
                    return;
                }

                dispatch.BeginInvoke(() => { result(ActionResult.Success); });
                return;
            }),
                
            InitializePostRequest(url));
        }

        private HttpWebRequest InitializePostRequest(string url)
        {
            HttpWebRequest request = AwfulWebRequest.CreateFormDataPostRequest(url, "application/x-www-form-urlencoded");
            return request;
        }
    }
}
