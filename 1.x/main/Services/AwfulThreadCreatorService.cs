using System;
using System.Linq;
using Awful.Helpers;
using Awful.Models;
using System.Threading;
using System.Collections.Generic;
using HtmlAgilityPack;
using System.Windows;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Net;
using System.Text;
using System.IO;
using Awful.Core.Web;

namespace Awful.Services
{
    public interface AwfulThreadRequest
    {
        Awful.Models.ForumData Forum { get; set; }
        IList<SAThreadIcon> Icons { get; set; }
        string FormKey { get; }
        string FormCookie { get; }
        string Title { get; set; }
        string Text { get; set; }
        SAThreadIcon SelectedIcon { get; set; }
    }

    public class NewThreadRequest : KollaSoft.PropertyChangedBase, AwfulThreadRequest
    {
        private SAThreadIcon _selectedIcon;
        private string _title;
        private string _text;

        public Awful.Models.ForumData Forum { get; set; }
        
        public IList<SAThreadIcon> Icons { get; set; }
        public string Text
        {
            get { return this._text; }
            set { this._text = value; NotifyPropertyChangedAsync("Text"); }
        }

        public string Title
        {
            get { return this._title; }
            set { this._title = value; NotifyPropertyChangedAsync("Title"); }
        }

        public SAThreadIcon SelectedIcon
        {
            get { return this._selectedIcon; }
            set
            {
                this._selectedIcon = value;
                NotifyPropertyChangedAsync("SelectedIcon");
            }
        }

        public string FormKey { get; set; }

        public string FormCookie { get; set; }

        public NewThreadRequest() { this.Text = string.Empty;  }
    }

    public class AwfulThreadCreatorService
    {
        private WebGet _web;
        private readonly BackgroundWorker _requestThread = new BackgroundWorker();
        private readonly Encoding _encoding = Encoding.UTF8;

        // http request strings

        private const string METHOD = "POST";
        private const string USERAGENT = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.106 Safari/535.2";
        private const string NEW_THREAD_URL = "http://forums.somethingawful.com/newthread.php";
        private const string NEW_THREAD_ACTION = "action=newthread";

        #region Command Thread Strings

        private const string THREAD_FORUMID_KEY = "forumid";
        private const string THREAD_ACTION_KEY = "action";
        private const string THREAD_ACTION_VALUE = "postthread";
        private const string THREAD_FORMKEY_KEY = "formkey";
        private const string THREAD_FORMCOOKIE_KEY = "form_cookie";
        private const string THREAD_SUBJECT_KEY = "subject";
        private const string THREAD_ICONID_KEY = "iconid";
        private const string THREAD_MESSAGE_KEY = "message";
        private const string THREAD_PARSEURL_KEY = "parseurl";
        private const string THREAD_PARSEURL_VALUE = "yes";

        #endregion

        #region Send Thread Strings

        private const string SEND_THREAD_BOUNDARY = "----WebKitFormBoundarygq9tJ7tz6Wp6n01d";
        private const string SEND_THREAD_CONTENT_TYPE = "multipart/form-data; boundary=" + SEND_THREAD_BOUNDARY;
        private const string SEND_THREAD_SUBMIT_KEY = "submit";
        private const string SEND_THREAD_SUBMIT_VALUE = "Submit New Thread";
        
        private readonly string SEND_THREAD_HEADER = String.Format("--{0}", SEND_THREAD_BOUNDARY);
        private readonly string SEND_THREAD_FOOTER = String.Format("--{0}--", SEND_THREAD_BOUNDARY);

        #endregion

        #region Preview Thread Strings

        private const string PREVIEW_THREAD_BOUNDARY = "----WebKitFormBoundaryCx06tdPLlzT4ibWy";
        private const string PREVIEW_THREAD_CONTENT_TYPE = "multipart/form-data; boundary=" + PREVIEW_THREAD_BOUNDARY;
        private const string PREVIEW_THREAD_SUBMIT_KEY = "preview";
        private const string PREVIEW_THERAD_SUBMIT_VALUE = "Preview Post";

        private readonly string PREVIEW_THREAD_HEADER = String.Format("--{0}", PREVIEW_THREAD_BOUNDARY);
        private readonly string PREVIEW_THREAD_FOOTER = String.Format("--{0}--", PREVIEW_THREAD_BOUNDARY);

        #endregion

        public AwfulThreadCreatorService()
        {
            this._requestThread.WorkerSupportsCancellation = true;
            this._requestThread.WorkerReportsProgress = false;
            this._requestThread.DoWork += new DoWorkEventHandler(Thread_RequestNewThread);
        }

        #region Thread Posting Methods

        public void SendNewThreadRequestAsync(AwfulThreadRequest request, Action<Awful.Core.Models.ActionResult> result)
        {
            ThreadPool.QueueUserWorkItem(state => { this.InitiateSend(request, result); }, null);
        }

        private void InitiateSend(AwfulThreadRequest data, Action<Awful.Core.Models.ActionResult> result)
        {
            var dispatch = Deployment.Current.Dispatcher;
            Awful.Core.Event.Logger.AddEntry("ThreadReplyService - Initiating Reply...");

            try
            {
                string url = NEW_THREAD_URL;
                HttpWebRequest request = Awful.Core.Web.AwfulWebRequest.CreatePostRequest(url);
                
                if (request == null)
                    throw new NullReferenceException("request is not an http request.");

                request.AllowAutoRedirect = true;

                var keys = request.Headers.AllKeys;

                foreach (var key in keys)
                    Awful.Core.Event.Logger.AddEntry(key.ToString());

                request.BeginGetRequestStream((asyncResult) =>
                {
                    byte[] formData = this.GetSendMultipartFormData(data);
                    var requestSuccessful = this.HandleGetRequest(asyncResult, formData);
                    if (!requestSuccessful)
                    {
                        Awful.Core.Event.Logger.AddEntry("ThreadCreatorService - Send failed @ GetRequest.");
                        Deployment.Current.Dispatcher.BeginInvoke(() => { result(Awful.Core.Models.ActionResult.Failure); });
                        return;
                    }

                    HttpWebRequest webRequest = asyncResult.AsyncState as HttpWebRequest;
                    webRequest.BeginGetResponse(getResponseResult =>
                    {
                        var responseSuccessful = HandleGetResponse(getResponseResult);
                        if (!responseSuccessful)
                        {
                            Awful.Core.Event.Logger.AddEntry("ThreadCreatorService - Get failed @ GetResponse.");
                            dispatch.BeginInvoke(() => { result(Awful.Core.Models.ActionResult.Failure); });
                        }
                        else
                        {
                            Awful.Core.Event.Logger.AddEntry("ThreadReplyService - Get successful.");
                            dispatch.BeginInvoke(() => { result(Awful.Core.Models.ActionResult.Success); });
                        }

                    }, webRequest);
                }, request);
            }

            catch (Exception)
            {
                Awful.Core.Event.Logger.AddEntry("ThreadGetService - Get failed.");
                dispatch.BeginInvoke(() => { result(Awful.Core.Models.ActionResult.Failure); });
            }
        }

        private byte[] GetSendMultipartFormData(AwfulThreadRequest data)
        {
            Stream dataStream = new MemoryStream();
            StringBuilder sb = new StringBuilder();

            string header = SEND_THREAD_HEADER;
            string footer = SEND_THREAD_FOOTER;

            // add forum id
            this.AddFormDataString(sb, THREAD_FORUMID_KEY, data.Forum.ID.ToString(), header);
            // add action
            this.AddFormDataString(sb, THREAD_ACTION_KEY, THREAD_ACTION_VALUE, header);
            // add form key
            this.AddFormDataString(sb, THREAD_FORMKEY_KEY, data.FormKey, header);
            // add form cookie
            this.AddFormDataString(sb, THREAD_FORMCOOKIE_KEY, data.FormCookie, header);
            // add subject
            this.AddFormDataString(sb, THREAD_SUBJECT_KEY, data.Title, header);
            // add icon id
            this.AddFormDataString(sb, THREAD_ICONID_KEY, data.SelectedIcon.Value, header);
            // add message
            this.AddFormDataString(sb, THREAD_MESSAGE_KEY, data.Text, header);
            // add parse url
            this.AddFormDataString(sb, THREAD_PARSEURL_KEY, THREAD_PARSEURL_VALUE, header);
            // add submit
            this.AddFormDataString(sb, SEND_THREAD_SUBMIT_KEY, SEND_THREAD_SUBMIT_VALUE, header);
            
            sb.AppendLine(footer);
            string content = sb.ToString();

            dataStream.Write(this._encoding.GetBytes(content), 0, content.Length);
            dataStream.Position = 0;
            byte[] formData = new byte[dataStream.Length];
            dataStream.Read(formData, 0, formData.Length);
            dataStream.Close();

            return formData;
        }

        #endregion

        #region Thread Preview Methods

        public void GetPreviewAsync(AwfulThreadRequest request, Action<Awful.Core.Models.ActionResult, SAThreadPage> result)
        {
            ThreadPool.QueueUserWorkItem(state => { this.InitiatePreview(request, result); }, null);
        }

        private void InitiatePreview(AwfulThreadRequest data, Action<Awful.Core.Models.ActionResult, SAThreadPage> result)
        {
            var dispatch = Deployment.Current.Dispatcher;
            Awful.Core.Event.Logger.AddEntry("ThreadReplyService - Initiating Reply...");

            try
            {
                string url = NEW_THREAD_URL;
                HttpWebRequest request = Awful.Core.Web.AwfulWebRequest.CreatePostRequest(url);

                if (request == null)
                    throw new NullReferenceException("request is not an http request.");

                request.AllowAutoRedirect = true;

                var keys = request.Headers.AllKeys;

                foreach (var key in keys)
                    Awful.Core.Event.Logger.AddEntry(key.ToString());

                request.BeginGetRequestStream((asyncResult) =>
                {
                    byte[] formData = this.GetPreviewMultipartFormData(data);
                    var requestSuccessful = this.HandleGetRequest(asyncResult, formData);
                    if (!requestSuccessful)
                    {
                        Awful.Core.Event.Logger.AddEntry("ThreadCreatorService - Send failed @ GetRequest.");
                        Deployment.Current.Dispatcher.BeginInvoke(() => { result(Awful.Core.Models.ActionResult.Failure, null); });
                        return;
                    }

                    HttpWebRequest webRequest = asyncResult.AsyncState as HttpWebRequest;
                    webRequest.BeginGetResponse(getResponseResult =>
                    {
                        string html = HandleGetResponseHtml(getResponseResult);
                        if (html == null)
                        {
                            Awful.Core.Event.Logger.AddEntry("ThreadCreatorService - Get failed @ GetResponse.");
                            dispatch.BeginInvoke(() => { result(Awful.Core.Models.ActionResult.Failure, null); });
                        }
                        else
                        {
                            Awful.Core.Event.Logger.AddEntry("ThreadReplyService - Get successful.");
                            SAThreadPage page = new SAThreadPreviewPage(html);
                            dispatch.BeginInvoke(() => { result(Awful.Core.Models.ActionResult.Success, page); });
                        }

                    }, webRequest);
                }, request);
            }

            catch (Exception)
            {
                Awful.Core.Event.Logger.AddEntry("ThreadGetService - Get failed.");
                dispatch.BeginInvoke(() => { result(Awful.Core.Models.ActionResult.Failure, null); });
            }
        }

        private byte[] GetPreviewMultipartFormData(AwfulThreadRequest data)
        {
            Stream dataStream = new MemoryStream();
            StringBuilder sb = new StringBuilder();

            string header = PREVIEW_THREAD_HEADER;
            string footer = PREVIEW_THREAD_FOOTER;

            // add forum id
            this.AddFormDataString(sb, THREAD_FORUMID_KEY, data.Forum.ID.ToString(), header);
            // add action
            this.AddFormDataString(sb, THREAD_ACTION_KEY, THREAD_ACTION_VALUE, header);
            // add form key
            this.AddFormDataString(sb, THREAD_FORMKEY_KEY, data.FormKey, header);
            // add form cookie
            this.AddFormDataString(sb, THREAD_FORMCOOKIE_KEY, data.FormCookie, header);
            // add subject
            this.AddFormDataString(sb, THREAD_SUBJECT_KEY, data.Title, header);
            // add icon id
            this.AddFormDataString(sb, THREAD_ICONID_KEY, data.SelectedIcon.Value, header);
            // add message
            this.AddFormDataString(sb, THREAD_MESSAGE_KEY, data.Text, header);
            // add parse url
            this.AddFormDataString(sb, THREAD_PARSEURL_KEY, THREAD_PARSEURL_VALUE, header);
            // add preview
            this.AddFormDataString(sb, PREVIEW_THREAD_SUBMIT_KEY, PREVIEW_THERAD_SUBMIT_VALUE, header);

            sb.AppendLine(footer);
            string content = sb.ToString();

            dataStream.Write(this._encoding.GetBytes(content), 0, content.Length);
            dataStream.Position = 0;
            byte[] formData = new byte[dataStream.Length];
            dataStream.Read(formData, 0, formData.Length);
            dataStream.Close();

            return formData;
        }

        #endregion

        #region Thread Requesting Methods

        private void Thread_RequestNewThread(object sender, DoWorkEventArgs e)
        {
            var forum = e.Argument as ForumData;
            if (forum == null)
            {
                e.Cancel = true;
                return;
            }

            AutoResetEvent signal = new AutoResetEvent(false);

            string threadRequestUrl = string.Format("{0}?{1}&forumid={2}", NEW_THREAD_URL,
                NEW_THREAD_ACTION, forum.ID);

            NewThreadRequest threadRequest = null;
            WebGetDocumentArgs doc = null;

            this._web = new WebGet();

            this._web.LoadAsync(threadRequestUrl, (webResult, document) =>
            {
                switch (webResult)
                {
                    case Awful.Core.Models.ActionResult.Success:
                        doc = document;
                        break;

                    case Awful.Core.Models.ActionResult.Cancelled:
                        e.Cancel = true;
                        break;

                    case Awful.Core.Models.ActionResult.Busy:
                        e.Cancel = true;
                        break;
                }

                signal.Set();
            });

            signal.WaitOne();

            threadRequest = this.GenerateNewThreadRequest(doc);

            if (threadRequest != null)
            {
                threadRequest.Forum = forum;
                e.Result = threadRequest;
            }
        }

        private NewThreadRequest GenerateNewThreadRequest(WebGetDocumentArgs document)
        {
            if (document == null || document.Document == null) return null;

            NewThreadRequest request = new NewThreadRequest();
            HtmlNode parent = document.Document.DocumentNode;

            if (this._requestThread.CancellationPending)
                return null;

            this.SetRequestFormInfo(request, document.Document);

            var icons = parent.Descendants("div")
                .Where(node => node.GetAttributeValue("class", "").Equals("posticon"));

            request.Icons = new List<SAThreadIcon>(30);
            request.Icons.Add(SAThreadIcon.Empty);

            foreach (var icon in icons)
            {
                try
                {
                    if (this._requestThread.CancellationPending)
                        return null;

                    var inputNode = icon.Descendants("input").SingleOrDefault();
                    var imgNode = icon.Descendants("img").SingleOrDefault();

                    string title = imgNode.GetAttributeValue("alt", "");
                    string value = inputNode.GetAttributeValue("value", "");
                    string uri = imgNode.GetAttributeValue("src", "");

                    request.Icons.Add(new SAThreadIcon(title, value, uri));
                }

                catch (Exception ex)
                {
                    Awful.Core.Event.Logger.AddEntry(string.Format("There was an error while parsing post icons. [{0}] {1}",
                        ex.Message, ex.StackTrace));
                }
            }

            request.SelectedIcon = SAThreadIcon.Empty;
            return request;
        }

        private void SetRequestFormInfo(NewThreadRequest data, HtmlDocument doc)
        {
            var formNodes = doc.DocumentNode.Descendants("input").ToArray();

            var formKeyNode = formNodes
                .Where(node => node.GetAttributeValue("name", "").Equals("formkey"))
                .FirstOrDefault();

            var formCookieNode = formNodes
                .Where(node => node.GetAttributeValue("name", "").Equals("form_cookie"))
                .FirstOrDefault();

            try
            {
                data.FormKey = formKeyNode.GetAttributeValue("value", "");
                data.FormCookie = formCookieNode.GetAttributeValue("value", "");
            }
            catch (Exception)
            {
                throw new InvalidOperationException("Could not parse newReply form data.");
            }
        }

        public void RequestNewThreadAsync(ForumData forum, Action<Awful.Core.Models.ActionResult, AwfulThreadRequest> result)
        {
            if (_requestThread.IsBusy)
            {
                result(Awful.Core.Models.ActionResult.Busy, null);
                return;
            }

            RunWorkerCompletedEventHandler handler = null;
            handler = (obj, args) =>
            {
                this._requestThread.RunWorkerCompleted -= new RunWorkerCompletedEventHandler(handler);
                if (args.Cancelled)
                {
                    result(Awful.Core.Models.ActionResult.Cancelled, null);
                }
                else
                {
                    NewThreadRequest request = args.Result as NewThreadRequest;

                    if (request == null)
                        result(Awful.Core.Models.ActionResult.Failure, null);
                    else
                        result(Awful.Core.Models.ActionResult.Success, request);
                }
            };

            _requestThread.RunWorkerCompleted += new RunWorkerCompletedEventHandler(handler);
            _requestThread.RunWorkerAsync(forum);
        }

        #endregion

        private bool HandleGetRequest(IAsyncResult result, byte[] formData)
        {
            Awful.Core.Event.Logger.AddEntry("ThreadCreatorService - GetRequest initiated.");

            try
            {
                HttpWebRequest webRequest = result.AsyncState as HttpWebRequest;
                Stream postStream = webRequest.EndGetRequestStream(result);
                postStream.Write(formData, 0, formData.Length);
                postStream.Close();
                Awful.Core.Event.Logger.AddEntry("ThreadCreatorService - GetRequest successful.");
                return true;
            }
            catch (Exception ex)
            {
                Awful.Core.Event.Logger.AddEntry(string.Format("ThreadCreatorService - GetRequest failed: {0}", ex.Message));
                return false;
            }
        }

        private bool HandleGetResponse(IAsyncResult result)
        {
            Awful.Core.Event.Logger.AddEntry("ThreadCreatorService - GetResponse initiated.");
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
                Awful.Core.Event.Logger.AddEntry("ThreadCreatorService - GetResponse successful.");
                return true;
            }

            catch (WebException ex)
            {
                Awful.Core.Event.Logger.AddEntry(string.Format("ThreadCreatorService - GetResponse failed: {0}", ex.Message));
                return false;
            }
        }

        private string HandleGetResponseHtml(IAsyncResult result)
        {
            Awful.Core.Event.Logger.AddEntry("ThreadCreatorService - GetResponse initiated.");
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
                Awful.Core.Event.Logger.AddEntry("ThreadCreatorService - GetResponse successful.");
                return html;
            }

            catch (WebException ex)
            {
                Awful.Core.Event.Logger.AddEntry(string.Format("ThreadCreatorService - GetResponse failed: {0}", ex.Message));
                return null;
            }
        }

        private void AddFormDataString(StringBuilder sb, string name, string data, string header)
        {
            sb.AppendLine(header);
            sb.AppendLine(String.Format("Content-Disposition: form-data; name=\"{0}\"", name));
            sb.AppendLine();
            sb.AppendLine(data);
        }
    }
}
