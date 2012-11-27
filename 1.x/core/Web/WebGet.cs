using System;
using System.Net;
using System.ComponentModel;
using System.Threading;
using System.IO;
using HtmlAgilityPack;
using System.Diagnostics;
using System.Windows;
using Awful.Core.Models;
using Awful.Core.Event;
using KollaSoft;

namespace Awful.Core.Web
{
    public class WebGet
    {
        private readonly BackgroundWorker worker = new BackgroundWorker();
        private readonly AutoResetEvent signal = new AutoResetEvent(false);
        private Action newRequest;
        private string _result;
        private bool _success;
        private WebGetDocumentArgs _args;
        private CookieContainer _webCookies;
        private int _threadTimeoutInMilliseconds;
        private const int DEFAULT_THREAD_TIMEOUT = 20000;
 
        public bool IsBusy { get { return this.worker.IsBusy; } }

        public WebGet(int threadTimeoutInMilliseconds)
        {
            this._threadTimeoutInMilliseconds = threadTimeoutInMilliseconds;
            worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;
            worker.WorkerReportsProgress = true;
            worker.DoWork += new DoWorkEventHandler(WebGet_DoWork);
            signal = new AutoResetEvent(false);
            newRequest = DoNothing;
        }

        public WebGet() : this(DEFAULT_THREAD_TIMEOUT) { }

        private void DoNothing() { }

        public void CancelAsync()
        {
            if (worker.IsBusy)
            {
                worker.CancelAsync();
            }
        }

        public void LoadAsync(string url, Action<ActionResult, WebGetDocumentArgs> loadDocAsync)
        {
            Logger.AddEntry(string.Format("WebGet: Recieved LoadAsync request for url '{0}'...", url));

            if (worker.IsBusy)
            {
                Logger.AddEntry("WebGet: The service is currently busy processing another request.");
                loadDocAsync(ActionResult.Busy, null);
                return;
            }

            RunWorkerCompletedEventHandler handler = null;
            handler = (obj, args) =>
                {
                    worker.RunWorkerCompleted -= new RunWorkerCompletedEventHandler(handler);
                    if (args.Cancelled)
                    {
                        loadDocAsync(ActionResult.Cancelled, null);
                        return;
                    }

                    WebGetDocumentArgs docArgs = args.Result as WebGetDocumentArgs;

                    if (docArgs == null)
                    {
                        Logger.AddEntry("WebGet: Could not generate HtmlDocument.");
                        loadDocAsync(ActionResult.Failure, null);
                    }
                    
                    else
                    {
                        Logger.AddEntry("WebGet: html request successful!");
                        loadDocAsync(ActionResult.Success, docArgs);
                    }
                };

            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(handler);
            worker.RunWorkerAsync(url);
        }

        private void WebGet_DoWork(object sender, DoWorkEventArgs e)
        {
            var url = e.Argument as string;
            if (url == null)
            {
                string error = "WebGet Error: Expected string, recieved " + e.Argument.GetType();
                Logger.AddEntry(error);
                throw new ArgumentException(error);
            }

            // add a cancel check
            if (worker.CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            this._result = string.Empty;
            this._success = true;

            HttpWebRequest request = AwfulWebRequest.CreateGetRequest(url);
            request.BeginGetResponse(ProcessGetResponse, request);
            
            signal.WaitOne(this._threadTimeoutInMilliseconds);

            if (this._success == false)
            {
                e.Result = null;
                return;
            }

            else if (worker.CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            e.Result = this._args;
        }

        private void ProcessGetResponse(IAsyncResult callback)
        {
            try
            {
                var request = (HttpWebRequest)callback.AsyncState;
                var response = (HttpWebResponse)request.EndGetResponse(callback);
                using (StreamReader reader = new StreamReader(response.GetResponseStream(),
                    System.Text.Encoding.GetEncoding(Constants.WEB_RESPONSE_ENCODING)))
                {
                    this._result = reader.ReadToEnd();
                    this._success = true;
                    reader.Close();
                }

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(this._result);
                string uri = response.ResponseUri.ToString();
                this._args = new WebGetDocumentArgs(doc, uri);
                signal.Set();
            }

            catch (Exception ex)
            {
                Logger.AddEntry(string.Format("WebGet: There was an error while processing the response. [{0}] {1}",
                    ex.Message, ex.StackTrace));

                this._success = false;
                this._args = null;
                this._result = null;

                signal.Set();
            }
        }
    }

    public class WebGetDocumentArgs : EventArgs
    {
        public readonly HtmlDocument Document;
        public readonly string Url;

        public WebGetDocumentArgs(HtmlDocument doc, string url)
        {
            Document = doc;
            Url = url;
        }
    }
}
