using System;
using Awful.Core.Models;
using System.Threading;
using System.ComponentModel;
using System.Collections.Generic;
using Awful.Core.Web;
using Awful.Core.Models.Factory;
using Awful.Core.Event;

namespace Awful.Core.Services
{
    public class AwfulSmileyService
    {
        private static readonly AwfulSmileyService Service = new AwfulSmileyService(DEFAULT_TIMEOUT);
        private WebGet _web;
        private readonly BackgroundWorker _task = new BackgroundWorker();
        private readonly AutoResetEvent _signal = new AutoResetEvent(false);
        private int _serviceRequestTimeout;

        private const string SMILEY_REQUEST_URI = "http://forums.somethingawful.com/misc.php?action=showsmilies";
        private const int DEFAULT_TIMEOUT = 10000;

        class AwfulSmileyRequest
        {
            public IList<AwfulSmiley> List { get; set; }
            public ActionResult Status { get; set; }
            public Action<ActionResult, IList<AwfulSmiley>> Result { get; set; }
        }

        private AwfulSmileyService(int timeout)
        {
            this._serviceRequestTimeout = timeout;
            this._task.WorkerSupportsCancellation = true;
            this._task.DoWork += new DoWorkEventHandler(OnTaskDoWork);
            this._task.RunWorkerCompleted += new RunWorkerCompletedEventHandler(OnTaskRunWorkerCompleted);
        }

        public static void FetchSmiliesFromWebAsync(Action<ActionResult, IList<AwfulSmiley>> result)
        {
            if (Service._task.IsBusy)
            {
                result(ActionResult.Busy, null);
            }

            var response = new AwfulSmileyRequest() { Result = result };
            Service._task.RunWorkerAsync(response);
        }

        private void OnTaskRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var response = e.Result as AwfulSmileyRequest;
            response.Result(response.Status, response.List);
        }

        private void OnTaskDoWork(object sender, DoWorkEventArgs e)
        {
            var request = e.Argument as AwfulSmileyRequest;
            if (request == null)
            {
                e.Result = new AwfulSmileyRequest() { Status = ActionResult.Failure };
                return;
            }

            request.Status = ActionResult.Cancelled;

            this._web = new WebGet();
            this._web.LoadAsync(SMILEY_REQUEST_URI, (result, args) =>
                {
                    switch (result)
                    {
                        case ActionResult.Success:
                            this.ProcessRequest(request, args);
                            break;

                        default:
                            request.Status = result;
                            break;
                    }

                    this._signal.Set();
                });

            int timeout = this._serviceRequestTimeout;

           
#if DEBUG
            timeout = -1;
#endif
            if (timeout > 0) { this._signal.WaitOne(timeout); }
            else { this._signal.WaitOne(); }
            e.Result = request;
        }

        private void ProcessRequest(AwfulSmileyRequest request, WebGetDocumentArgs args)
        {
            if (request == null) return;

            try
            {
                request.List = AwfulSmileyFactory.Build(args.Document);
                request.Status = ActionResult.Success;
            }

            catch (Exception ex)
            {
                string error = string.Format("An error occurred while processing a smiley request: [{0}] {1}", 
                    ex.Message, ex.StackTrace);
                
                Logger.AddEntry(error);
                request.Status = ActionResult.Failure;
            }
        }

        public void CancelAsync()
        {
            if (this._web.IsBusy) { this._web.CancelAsync(); }
            if (this._task.IsBusy) { this._task.CancelAsync(); }
        }
    }
}
