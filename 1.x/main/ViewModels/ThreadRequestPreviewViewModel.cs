using System;
using Awful.Models;
using System.Threading;

namespace Awful.ViewModels
{
    public class ThreadRequestPreviewViewModel : ThreadViewerViewModel
    {
        public ThreadRequestPreviewViewModel()
            : base()
        {
            this.ThreadTitle = "Please wait...";
            this._CurrentPage = 1;
            this._TotalPages = 1;
            this._SaveIndex = "www\\preview.html";
            this.HtmlUri = "preview.html";
        }

        public override void LoadThreadPageAsync(Models.ThreadData thread, int pageNumber = 0, int postNumber = -1)
        {
           // do nothing here.
        }

        public override void ReloadCurrentPageAsync(Models.ThreadData thread, int postNumber = -1)
        {
           // do nothing here.
        }

        public void ShowPreviewAsync(Awful.Core.Models.ActionResult result, SAThreadPage preview)
        {
           
            ThreadPool.QueueUserWorkItem(state =>
                {
                    this.HandleResult(result, preview, 0);

                }, null);
        }
    }
}
