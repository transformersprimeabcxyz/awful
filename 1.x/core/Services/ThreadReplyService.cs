using System;
using System.Linq;
using System.Net;
using System.ComponentModel;
using System.Threading;
using HtmlAgilityPack;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Windows;
using Awful.Core.Models;
using Awful.Core.Event;
using Awful.Core.Web;

namespace Awful.Services
{
    public class ThreadReplyService
    {
        private readonly AutoResetEvent sendReplySignal = new AutoResetEvent(false);
        private readonly AutoResetEvent getTextSignal = new AutoResetEvent(false);
        private readonly AutoResetEvent sendEditSignal = new AutoResetEvent(false);
        private bool getTextCancelled;
        private int _defaultTimeout;
        private const int DEFAULT_TIMEOUT = 10000;

        // responses
        private const string REPLY_ACTION_REQUEST = "postreply";
        private const string SUBMIT_REQUEST = "Submit Reply";
        private const string PARSEURL_REQUEST = "yes";

        private const string EDIT_ACTION_REQUEST = "updatepost";
        private const string BOOKMARK_REQUEST = "yes";

        // http request form data
        private const string METHOD = "POST";
        private const string REPLY_BOUNDARY = "----WebKitFormBoundaryYRBJZZBPUZAdxj3S";
        private const string EDIT_BOUNDARY = "----WebKitFormBoundaryksMFcMGBHc3jdB0P";
        private const string REPLY_CONTENT_TYPE = "multipart/form-data; boundary=" + REPLY_BOUNDARY;
        private const string EDIT_CONTENT_TYPE = "multipart/form-data; boundary=" + EDIT_BOUNDARY;
        private const string USERAGENT = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.106 Safari/535.2";
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

        public ThreadReplyService(int timeout) { this._defaultTimeout = timeout; }
        public ThreadReplyService() : this(DEFAULT_TIMEOUT) { }

        public void GetQuote(string postID, Action<ActionResult, string> result)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(state =>
                {
                    var url = string.Format("http://forums.somethingawful.com/newreply.php?action=newreply&postid={0}", postID);
                    BeginGetTextFromWebForm(url, result);
                }), 
                null);
        }

        #region Post Replying Methods

        public void ReplyAsync(ThreadData thread, string text, Action<ActionResult> onFinish)
        {
            ThreadPool.QueueUserWorkItem(state =>
                {
                    var threadID = thread.ID.ToString();
                    ThreadReplyData? data = GetReplyData(threadID, text);

                    if (data.HasValue)
                    {
                        Logger.AddEntry(string.Format("ThreadReplyService - Reply data: {0}|{1}|{2}{3}",
                            data.Value.THREADID,
                            data.Value.TEXT,
                            data.Value.FORMKEY,
                            data.Value.FORMCOOKIE));

                        InitiateReply(data.Value, onFinish);
                    }

                    else
                    {
                        Logger.AddEntry("ThreadReplyService - ReplyAsync failed on null ThreadReplyData.");
                        onFinish(ActionResult.Failure);
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
            WebGet web = new WebGet();
            web.LoadAsync(url, (obj, loadAsyncArgs) =>
                {
                    if (loadAsyncArgs.Document != null)
                    {
                        doc = loadAsyncArgs.Document;
                        success = true;
                    }
                    replyDataSignal.Set();
                });
            
            replyDataSignal.WaitOne(this._defaultTimeout);
            
            if (success == true)
            {
                ThreadReplyData data = GetReplyFormInfo(threadID, doc);
                data.TEXT = text;
                return data;
            }

            return null;
        }

        private void InitiateReply(ThreadReplyData data, Action<ActionResult> result)
        {
            var dispatch = Deployment.Current.Dispatcher;
            Logger.AddEntry("ThreadReplyService - Initiating Reply...");

            try
            {
                string url = "http://forums.somethingawful.com/newreply.php";
                request = AwfulWebRequest.CreateFormDataPostRequest(url, REPLY_CONTENT_TYPE);
                if (request == null)
                    throw new NullReferenceException("request is not an http request.");

                request.AllowAutoRedirect = true;
                
                request.BeginGetRequestStream((asyncResult) =>
                    {
                        byte[] formData = GetReplyMultipartFormData(data);
                        var requestSuccessful = HandleGetRequest(asyncResult, formData);
                        if (!requestSuccessful)
                        {
                            Logger.AddEntry("ThreadReplyService - Reply failed @ GetRequest.");
                            dispatch.BeginInvoke(() => { result(ActionResult.Failure); });
                            return;
                        }

                        HttpWebRequest webRequest = asyncResult.AsyncState as HttpWebRequest;
                        webRequest.BeginGetResponse(getResponseResult =>
                        {
                            var responseSuccessful = HandleGetResponse(getResponseResult);
                            if (!responseSuccessful)
                            {
                                Logger.AddEntry("ThreadReplyService - Reply failed @ GetResponse.");
                                dispatch.BeginInvoke(() => { result(ActionResult.Failure); });
                            }
                            else
                            {
                                Logger.AddEntry("ThreadReplyService - Reply successful.");
                                dispatch.BeginInvoke(() => { result(ActionResult.Success); });
                            }

                        }, webRequest);
                    }, request);
            }

            catch (Exception) {
                Logger.AddEntry("ThreadReplyService - Reply failed.");
                dispatch.BeginInvoke(() => { result(ActionResult.Failure); }); }
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
            AddFormDataString(sb, "submit", SUBMIT_REQUEST, REPLY_HEADER);
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

        public void GetEdit(string postID, Action<ActionResult, string> result)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(state =>
            {
                var url = string.Format("http://forums.somethingawful.com/editpost.php?action=editpost&postid={0}", postID);
                BeginGetTextFromWebForm(url, result);
            }),
                null);
        }

        public void SendEditAsync(string postID, string text, Action<ActionResult> result)
        {
            PostEditData data = new PostEditData() { POSTID = postID, TEXT = text };

            ThreadPool.QueueUserWorkItem(new WaitCallback(state =>
                {
                    InitiateEditRequest(data, result);
                }),
                null);
        }

        private void InitiateEditRequest(PostEditData data, Action<ActionResult> result)
        {
            var dispatch = Deployment.Current.Dispatcher;

            Logger.AddEntry("ThreadReplyService - EditRequest initiated.");
            try
            {
                string url = "http://forums.somethingawful.com/editpost.php";
                request = AwfulWebRequest.CreateFormDataPostRequest(url, EDIT_CONTENT_TYPE);
                if (request == null)
                    throw new NullReferenceException("request is not an http request.");

                request.AllowAutoRedirect = true;
                
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
                                Logger.AddEntry("ThreadReplyService - EditRequest successful.");
                                dispatch.BeginInvoke(() => { result(ActionResult.Success); });
                            }
                            else
                            {
                                Logger.AddEntry("ThreadReplyService - EditRequest failed.");
                                dispatch.BeginInvoke(() => { result(ActionResult.Failure); });
                            }

                        }, webRequest);
                    }
                    else
                    {
                        Logger.AddEntry("ThreadReplyService - EditRequest failed.");
                        dispatch.BeginInvoke(() => { result(ActionResult.Failure); });
                    }

                }, request);
            }

            catch (Exception) {
                Logger.AddEntry("ThreadReplyService - EditRequest failed.");
                dispatch.BeginInvoke(() => { result(ActionResult.Failure); }); }
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
            AddFormDataString(sb, "submit", SUBMIT_REQUEST, EDIT_HEADER);
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

        private void BeginGetTextFromWebForm(string webUrl, Action<ActionResult, string> result)
        {
            var web = new WebGet();
            var url = webUrl;

            Logger.AddEntry(string.Format("ThreadReplyServer - Retrieving text from '{0}'.", url));

            ActionResult success = ActionResult.Failure;
            string bodyText = String.Empty;
            HtmlNode docNode = null;

            web.LoadAsync(url, (obj, args) =>
            {
                switch (obj)
                {
                    case ActionResult.Success:
                        if (args.Document != null)
                        {
                            success = ActionResult.Success;
                            docNode = args.Document.DocumentNode;
                        }
                        else { success = ActionResult.Failure; }
                        break;
                }

                getTextSignal.Set();
            });

            getTextSignal.WaitOne(this._defaultTimeout);

            if (getTextCancelled)
            {
                getTextCancelled = false;
                success = ActionResult.Cancelled;
            }

            if (success == ActionResult.Success)
            {
                bodyText = GetFormText(docNode);
                bodyText = HttpUtility.HtmlDecode(bodyText);
            }

            Logger.AddEntry(string.Format("ThreadReplyService - Get text result: success: {0}", success));
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
            Logger.AddEntry("ThreadReplyService - GetRequest initiated.");

            try
            {
                HttpWebRequest webRequest = result.AsyncState as HttpWebRequest;
                Stream postStream = webRequest.EndGetRequestStream(result);
                postStream.Write(formData, 0, formData.Length);
                postStream.Close();
                Logger.AddEntry("ThreadReplyService - GetRequest successful.");
                return true;
            }
            catch (Exception ex) {
                Logger.AddEntry(string.Format("ThreadReplyService - GetRequest failed: {0}", ex.Message));
                return false; }
        }

        private bool HandleGetResponse(IAsyncResult result)
        {
            Logger.AddEntry("ThreadReplyService - GetResponse initiated.");
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
                Logger.AddEntry("ThreadReplyService - GetResponse successful.");
                return true;
            }

            catch (WebException ex) {
                Logger.AddEntry(string.Format("ThreadReplyService - GetResponse failed: {0}", ex.Message));
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
