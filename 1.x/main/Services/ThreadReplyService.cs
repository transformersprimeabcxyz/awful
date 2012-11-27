using System;
using System.Linq;
using System.Net;
using System.ComponentModel;
using System.Threading;
using Awful.Models;
using HtmlAgilityPack;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Awful.Helpers;
using System.Windows;

namespace Awful.Services
{
    public class ThreadReplyService : Cancellable
    {
        private readonly AutoResetEvent sendReplySignal = new AutoResetEvent(false);
        private readonly AutoResetEvent getTextSignal = new AutoResetEvent(false);
        private readonly AutoResetEvent sendEditSignal = new AutoResetEvent(false);
        private bool getTextCancelled;
  
        // responses
        private const string REPLY_ACTION_REQUEST = "postreply";
        private const string REPLY_SUBMIT_REQUEST = "Submit Reply";
        private const string PARSEURL_REQUEST = "yes";

        private const string EDIT_SUBMIT_REQUEST = "Save Changes";
        private const string EDIT_ACTION_REQUEST = "updatepost";
        private const string BOOKMARK_REQUEST = "yes";

        // http request form data
        private const string REPLY_BOUNDARY = "----WebKitFormBoundary3E3ejPiM23mrvvPz";
        private const string EDIT_BOUNDARY = "----WebKitFormBoundaryzIodq9mnS9RU1Kgc";
        private const string REPLY_CONTENT_TYPE = "multipart/form-data; boundary=" + REPLY_BOUNDARY;
        private const string EDIT_CONTENT_TYPE = "multipart/form-data; boundary=" + EDIT_BOUNDARY;
        private readonly string REPLY_HEADER = String.Format("--{0}", REPLY_BOUNDARY);
        private readonly string REPLY_FOOTER = String.Format("--{0}--", REPLY_BOUNDARY);
        private readonly string EDIT_HEADER = String.Format("--{0}", EDIT_BOUNDARY);
        private readonly string EDIT_FOOTER = String.Format("--{0}--", EDIT_BOUNDARY);

        // web client
        private HttpWebRequest request;
        private readonly Encoding encoding = Encoding.UTF8;

        struct ThreadReplyData
        {
            public string THREADID;
            public string TEXT;
            public string FORMKEY;
            public string FORMCOOKIE;
        }

        struct PostEditData
        {
            public string POSTID;
            public string TEXT;
        }

        public ThreadReplyService()
        {

        }

        public void GetQuote(string postID, Action<Awful.Core.Models.ActionResult, string> result)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(state =>
                {
                    var url = string.Format("http://forums.somethingawful.com/newreply.php?action=newreply&postid={0}", postID);
                    BeginGetTextFromWebForm(url, result);
                }), 
                null);
        }

        #region Post Replying Methods

        public void ReplyAsync(ThreadData thread, string text, Action<Awful.Core.Models.ActionResult> onFinish)
        {
            ThreadPool.QueueUserWorkItem(state =>
                {
                    var threadID = thread.ID.ToString();
                    ThreadReplyData? data = GetReplyData(threadID, text);

                    if (data.HasValue)
                    {
                        Awful.Core.Event.Logger.AddEntry(string.Format("ThreadReplyService - Reply data: {0}|{1}|{2}{3}",
                            data.Value.THREADID,
                            data.Value.TEXT,
                            data.Value.FORMKEY,
                            data.Value.FORMCOOKIE));

                        InitiateReply(data.Value, onFinish);
                    }

                    else
                    {
                        Awful.Core.Event.Logger.AddEntry("ThreadReplyService - ReplyAsync failed on null ThreadReplyData.");
                        onFinish(Awful.Core.Models.ActionResult.Failure);
                    }

                }, null);
        }

        private ThreadReplyData? GetReplyData(string threadID, string text)
        {
            string url = String.Format("http://forums.somethingawful.com/newreply.php?action=newreply&threadid={0}",
                threadID);

            AutoResetEvent replyDataSignal = new AutoResetEvent(false);
            HtmlDocument doc = null;
            bool success = false;
            var web = new Awful.Core.Web.WebGet();
            web.LoadAsync(url, (obj, loadAsyncArgs) =>
                {
                    if (loadAsyncArgs.Document != null)
                    {
                        doc = loadAsyncArgs.Document;
                        success = true;
                    }
                    replyDataSignal.Set();
                });
            
            replyDataSignal.WaitOne(App.Settings.ThreadTimeout);
            
            if (success == true)
            {
                ThreadReplyData data = GetReplyFormInfo(threadID, doc);
                data.TEXT = text;
                return data;
            }

            return null;
        }

        private void InitiateReply(ThreadReplyData data, Action<Awful.Core.Models.ActionResult> result)
        {
            var dispatch = Deployment.Current.Dispatcher;
            Awful.Core.Event.Logger.AddEntry("ThreadReplyService - Initiating Reply...");

            try
            {
                string url = "http://forums.somethingawful.com/newreply.php";
                request = Awful.Core.Web.AwfulWebRequest.CreateFormDataPostRequest(url, REPLY_CONTENT_TYPE);

                if (request == null)
                    throw new NullReferenceException("request is not an http request.");

                request.AllowAutoRedirect = true;

                var keys = request.Headers.AllKeys;
                foreach (var key in keys)
                    Debug.WriteLine(key);

                request.BeginGetRequestStream((asyncResult) =>
                    {
                        byte[] formData = GetReplyMultipartFormData(data);
                        var requestSuccessful = HandleGetRequest(asyncResult, formData);
                        if (!requestSuccessful)
                        {
                            Awful.Core.Event.Logger.AddEntry("ThreadReplyService - Reply failed @ GetRequest.");
                            dispatch.BeginInvoke(() => { result(Awful.Core.Models.ActionResult.Failure); });
                            return;
                        }

                        HttpWebRequest webRequest = asyncResult.AsyncState as HttpWebRequest;
                        webRequest.BeginGetResponse(getResponseResult =>
                        {
                            var responseSuccessful = HandleGetResponse(getResponseResult);
                            if (!responseSuccessful)
                            {
                                Awful.Core.Event.Logger.AddEntry("ThreadReplyService - Reply failed @ GetResponse.");
                                dispatch.BeginInvoke(() => { result(Awful.Core.Models.ActionResult.Failure); });
                            }
                            else
                            {
                                Awful.Core.Event.Logger.AddEntry("ThreadReplyService - Reply successful.");
                                dispatch.BeginInvoke(() => { result(Awful.Core.Models.ActionResult.Success); });
                            }

                        }, webRequest);
                    }, request);
            }

            catch (Exception) {
                Awful.Core.Event.Logger.AddEntry("ThreadReplyService - Reply failed.");
                dispatch.BeginInvoke(() => { result(Awful.Core.Models.ActionResult.Failure); }); }
        }

        private byte[] GetReplyMultipartFormData(ThreadReplyData data)
        {
            Stream dataStream = new MemoryStream();
            StringBuilder sb = new StringBuilder();

            AddFormDataString(sb, "action", REPLY_ACTION_REQUEST, REPLY_HEADER);
            AddFormDataString(sb, "threadid", data.THREADID, REPLY_HEADER);
            AddFormDataString(sb, "formkey", data.FORMKEY, REPLY_HEADER);
            AddFormDataString(sb, "form_cookie", data.FORMCOOKIE, REPLY_HEADER);
            AddFormDataString(sb, "message", data.TEXT, REPLY_HEADER);
            AddFormDataString(sb, "parseurl", PARSEURL_REQUEST, REPLY_HEADER);
            AddFormDataString(sb, "submit", REPLY_SUBMIT_REQUEST, REPLY_HEADER);
            sb.AppendLine(REPLY_FOOTER);

            string content = sb.ToString();

            dataStream.Write(encoding.GetBytes(content), 0, content.Length);

            dataStream.Position = 0;
            byte[] formData = new byte[dataStream.Length];
            dataStream.Read(formData, 0, formData.Length);
            dataStream.Close();

            return formData;
        }

        private ThreadReplyData GetReplyFormInfo(string threadID, HtmlDocument doc)
        {
            ThreadReplyData data = new ThreadReplyData();
            data.THREADID = threadID;

            var formNodes = doc.DocumentNode.Descendants("input").ToArray();

            var formKeyNode = formNodes
                .Where(node => node.GetAttributeValue("name", "").Equals("formkey"))
                .FirstOrDefault();

            var formCookieNode = formNodes
                .Where(node => node.GetAttributeValue("name", "").Equals("form_cookie"))
                .FirstOrDefault();

            try
            {
                data.FORMKEY = formKeyNode.GetAttributeValue("value", "");
                data.FORMCOOKIE = formCookieNode.GetAttributeValue("value", "");
            }
            catch (Exception)
            {
                throw new InvalidOperationException("Could not parse newReply form data.");
            }

            return data;
        }

        #endregion

        #region Post Editing Methods

        public void GetEdit(string postID, Action<Awful.Core.Models.ActionResult, string> result)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(state =>
            {
                var url = string.Format("http://forums.somethingawful.com/editpost.php?action=editpost&postid={0}", postID);
                BeginGetTextFromWebForm(url, result);
            }),
                null);
        }

        public void SendEditAsync(string postID, string text, Action<Awful.Core.Models.ActionResult> result)
        {
            PostEditData data = new PostEditData() { POSTID = postID, TEXT = text };

            ThreadPool.QueueUserWorkItem(new WaitCallback(state =>
                {
                    InitiateEditRequest(data, result);
                }),
                null);
        }

        private void InitiateEditRequest(PostEditData data, Action<Awful.Core.Models.ActionResult> result)
        {
            var dispatch = Deployment.Current.Dispatcher;

            Awful.Core.Event.Logger.AddEntry("ThreadReplyService - EditRequest initiated.");
            try
            {
                string url = "http://forums.somethingawful.com/editpost.php";
                request = Awful.Core.Web.AwfulWebRequest.CreateFormDataPostRequest(url, EDIT_CONTENT_TYPE);

                if (request == null)
                    throw new NullReferenceException("request is not an http request.");

                request.AllowAutoRedirect = true;
                
                var keys = request.Headers.AllKeys;
                foreach (var key in keys)
                    Debug.WriteLine(key);

                request.BeginGetRequestStream((getRequestAsyncResult) =>
                {
                    byte[] formData = GetEditMultipartFormData(data);
                    if (HandleGetRequest(getRequestAsyncResult, formData))
                    {
                        var webRequest = getRequestAsyncResult.AsyncState as HttpWebRequest;
                        webRequest.BeginGetResponse((getResponseAsyncResult) =>
                        {
                            if (HandleGetResponse(getResponseAsyncResult))
                            {
                                Awful.Core.Event.Logger.AddEntry("ThreadReplyService - EditRequest successful.");
                                dispatch.BeginInvoke(() => { result(Awful.Core.Models.ActionResult.Success); });
                            }
                            else
                            {
                                Awful.Core.Event.Logger.AddEntry("ThreadReplyService - EditRequest failed.");
                                dispatch.BeginInvoke(() => { result(Awful.Core.Models.ActionResult.Failure); });
                            }

                        }, webRequest);
                    }
                    else
                    {
                        Awful.Core.Event.Logger.AddEntry("ThreadReplyService - EditRequest failed.");
                        dispatch.BeginInvoke(() => { result(Awful.Core.Models.ActionResult.Failure); });
                    }

                }, request);
            }

            catch (Exception) {
                Awful.Core.Event.Logger.AddEntry("ThreadReplyService - EditRequest failed.");
                dispatch.BeginInvoke(() => { result(Awful.Core.Models.ActionResult.Failure); }); }
        }

        private byte[] GetEditMultipartFormData(PostEditData data)
        {
            Stream dataStream = new MemoryStream();
            StringBuilder sb = new StringBuilder();

            AddFormDataString(sb, "action", EDIT_ACTION_REQUEST, EDIT_HEADER);
            AddFormDataString(sb, "postid", data.POSTID, EDIT_HEADER);
            AddFormDataString(sb, "message", data.TEXT, EDIT_HEADER);
            AddFormDataString(sb, "parseurl", PARSEURL_REQUEST, EDIT_HEADER);
            AddFormDataString(sb, "bookmark", BOOKMARK_REQUEST, EDIT_HEADER);
            AddFormDataString(sb, "submit", EDIT_SUBMIT_REQUEST, EDIT_HEADER);
            sb.AppendLine(EDIT_FOOTER);

            string content = sb.ToString();

            dataStream.Write(encoding.GetBytes(content), 0, content.Length);

            dataStream.Position = 0;
            byte[] formData = new byte[dataStream.Length];
            dataStream.Read(formData, 0, formData.Length);
            dataStream.Close();

            return formData;
        }

        #endregion

        private void BeginGetTextFromWebForm(string webUrl, Action<Awful.Core.Models.ActionResult, string> result)
        {
            var web = new Awful.Core.Web.WebGet();
            var url = webUrl;

            Awful.Core.Event.Logger.AddEntry(string.Format("ThreadReplyServer - Retrieving text from '{0}'.", url));

            Awful.Core.Models.ActionResult success = Awful.Core.Models.ActionResult.Failure;
            string bodyText = String.Empty;
            HtmlNode docNode = null;

            web.LoadAsync(url, (obj, args) =>
            {
                switch (obj)
                {
                    case Awful.Core.Models.ActionResult.Success:
                        if (args.Document != null)
                        {
                            success = Awful.Core.Models.ActionResult.Success;
                            docNode = args.Document.DocumentNode;
                        }
                        else { success = Awful.Core.Models.ActionResult.Failure; }
                        break;
                }

                getTextSignal.Set();
            });

            getTextSignal.WaitOne(App.Settings.ThreadTimeout);

            if (getTextCancelled)
            {
                getTextCancelled = false;
                success = Awful.Core.Models.ActionResult.Cancelled;
            }

            if (success == Awful.Core.Models.ActionResult.Success)
            {
                bodyText = GetFormText(docNode);
                bodyText = HttpUtility.HtmlDecode(bodyText);
            }

            Awful.Core.Event.Logger.AddEntry(string.Format("ThreadReplyService - Get text result: success: {0}", success));
            var dispatcher = Deployment.Current.Dispatcher;
            
            dispatcher.BeginInvoke(() =>
            {
                result(success, bodyText);
            });
        }

        private string GetFormText(HtmlNode node)
        {
            var textArea = node.Descendants("textarea").FirstOrDefault();
            if (textArea == null) return String.Empty;

            var text = textArea.InnerText;
            return text;
        }

        private bool HandleGetRequest(IAsyncResult result, byte[] formData)
        {
            Awful.Core.Event.Logger.AddEntry("ThreadReplyService - GetRequest initiated.");

            try
            {
                HttpWebRequest webRequest = result.AsyncState as HttpWebRequest;
                Stream postStream = webRequest.EndGetRequestStream(result);
                postStream.Write(formData, 0, formData.Length);
                postStream.Close();
                Awful.Core.Event.Logger.AddEntry("ThreadReplyService - GetRequest successful.");
                return true;
            }
            catch (Exception ex) {
                Awful.Core.Event.Logger.AddEntry(string.Format("ThreadReplyService - GetRequest failed: {0}", ex.Message));
                return false; }
        }

        private bool HandleGetResponse(IAsyncResult result)
        {
            Awful.Core.Event.Logger.AddEntry("ThreadReplyService - GetResponse initiated.");
            try
            {
                HttpWebRequest webRequest = result.AsyncState as HttpWebRequest;
                HttpWebResponse response = webRequest.EndGetResponse(result) as HttpWebResponse;
                Stream responseStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);
                string html = String.Empty;
                html = reader.ReadToEnd();
                reader.Close();
                responseStream.Close();
                response.Close();
                Awful.Core.Event.Logger.AddEntry("ThreadReplyService - GetResponse successful.");
                return true;
            }

            catch (WebException ex) {
                Awful.Core.Event.Logger.AddEntry(string.Format("ThreadReplyService - GetResponse failed: {0}", ex.Message));
                return false; 
            }
        }

        private void AddFormDataString(StringBuilder sb, string name, string data, string header)
        {
            sb.AppendLine(header);
            sb.AppendLine(String.Format("Content-Disposition: form-data; name=\"{0}\"", name));
            sb.AppendLine();
            sb.AppendLine(data);            
        }

        public void CancelAsync()
        {
            if (!getTextCancelled)
            {
                getTextCancelled = true;
                getTextSignal.Set();
            }
        }
    }
}
