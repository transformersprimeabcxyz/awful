using System;
using Awful.Helpers;
using Awful.Models;
using System.Threading;
using System.ComponentModel;
using System.Collections.Generic;
using Awful.Core.Web;
using Awful.Core.Models;

namespace Awful.Services
{
    public class AwfulSmileyService : Cancellable
    {
        private static readonly AwfulSmileyService Service = new AwfulSmileyService();
        private WebGet _web;
        private readonly BackgroundWorker _task = new BackgroundWorker();
        private readonly AutoResetEvent _signal = new AutoResetEvent(false);

        private const string SMILEY_REQUEST_URI = "http://forums.somethingawful.com/misc.php?action=showsmilies";

        class AwfulSmileyRequest
        {
            public IList<Awful.Models.AwfulSmiley> List { get; set; }
            public Awful.Core.Models.ActionResult Status { get; set; }
            public Action<Awful.Core.Models.ActionResult, IList<Awful.Models.AwfulSmiley>> Result { get; set; }
        }

        private AwfulSmileyService()
        {
            this._task.WorkerSupportsCancellation = true;
            this._task.DoWork += new DoWorkEventHandler(OnTaskDoWork);
            this._task.RunWorkerCompleted += new RunWorkerCompletedEventHandler(OnTaskRunWorkerCompleted);
        }

        public static Cancellable FetchSmiliesFromWebAsync(Action<Awful.Core.Models.ActionResult, IList<Awful.Models.AwfulSmiley>> result)
        {
            if (Service._task.IsBusy)
            {
                result(Awful.Core.Models.ActionResult.Busy, null);
                return null;
            }

            var response = new AwfulSmileyRequest() { Result = result };
            Service._task.RunWorkerAsync(response);
            return Service;
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
                e.Result = new AwfulSmileyRequest() { Status = Awful.Core.Models.ActionResult.Failure };
                return;
            }

            request.Status = Awful.Core.Models.ActionResult.Cancelled;

            this._web = new WebGet();
            this._web.LoadAsync(SMILEY_REQUEST_URI, (result, args) =>
                {
                    switch (result)
                    {
                        case Awful.Core.Models.ActionResult.Success:
                            this.ProcessRequest(request, args);
                            break;

                        default:
                            request.Status = result;
                            break;
                    }

                    this._signal.Set();
                });

            int timeout = AwfulSettings.THREAD_TIMEOUT_DEFAULT;

           
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
                request.List = SASmileyFactory.Build(args.Document);
                request.Status = Awful.Core.Models.ActionResult.Success;
            }

            catch (Exception ex)
            {
                string error = string.Format("An error occurred while processing a smiley request: [{0}] {1}", 
                    ex.Message, ex.StackTrace);
                
                Awful.Core.Event.Logger.AddEntry(error);
                request.Status = Awful.Core.Models.ActionResult.Failure;
            }
        }

        public void CancelAsync()
        {
            if (this._web.IsBusy) { this._web.CancelAsync(); }
            if (this._task.IsBusy) { this._task.CancelAsync(); }
        }
    }
}
