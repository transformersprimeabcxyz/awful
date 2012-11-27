// -----------------------------------------------------------------------
// <copyright file="ICancellable.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Awful.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.ComponentModel;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public interface ICancellable
    {
        void Cancel();
    }

    public delegate void DoWorkDelegate(object obj, DoWorkEventArgs args);
    public delegate void TaskCompletedDelegate(object obj, RunWorkerCompletedEventArgs args);

    public class CancellableTask : ICancellable
    {
        private readonly BackgroundWorker _worker;
        private readonly DoWorkDelegate _doWork;
        private readonly TaskCompletedDelegate _completed;

        public BackgroundWorker Worker { get { return this._worker; } }
      
        public CancellableTask(BackgroundWorker worker) { this._worker = worker; worker.WorkerSupportsCancellation = true; }

        public CancellableTask(DoWorkDelegate doWork, TaskCompletedDelegate completed) : this(new BackgroundWorker())
        {
            this._doWork = doWork;
            this._completed = completed;
            this._worker.DoWork += new DoWorkEventHandler(this._doWork);
            this._worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this._completed);
        }

        public void Cancel()
        {
            if (this._worker.IsBusy) { this._worker.CancelAsync(); }
        }
    }

}
