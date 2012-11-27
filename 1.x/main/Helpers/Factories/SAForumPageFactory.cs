using System;
using System.Linq;
using Awful.Models;
using HtmlAgilityPack;
using System.Threading;
using System.ComponentModel;
using System.Collections.Generic;
using KollaSoft;
using Awful.Core.Web;

namespace Awful.Helpers
{
    public class SAForumPageFactory
    {
        private static readonly SAForumPageFactory Factory = new SAForumPageFactory();

        private readonly AutoResetEvent _signal = new AutoResetEvent(false);
        private readonly BackgroundWorker _worker = new BackgroundWorker();
        private WebGet _web;


        private SAForumPageFactory()
        {
            this._worker.WorkerSupportsCancellation = true;
            this._worker.DoWork += new DoWorkEventHandler(OnDoWork);
        }

        private Awful.Core.Models.ActionResult BuildPageFromHtml(SAForumPage page, WebGetDocumentArgs args)
        {
            try
            {
                var node = args.Document.DocumentNode;
                this.HandleMaxPages(page, node);
                this.HandleThreads(page, node);
                return Awful.Core.Models.ActionResult.Success;
            }

            catch (Exception ex) { return Awful.Core.Models.ActionResult.Failure; }
        }

        private void HandleMaxPages(SAForumPage page, HtmlNode node)
        {
            var maxPagesNode = node.Descendants("div")
                .Where(n => n.GetAttributeValue("class", "").Equals("pages"))
                .FirstOrDefault();

            if (maxPagesNode == null)
            {
                Awful.Core.Event.Logger.AddEntry("AwfulForumPage - Could not parse maxPagesNode.");
                page.Parent.MaxPages = 1;
            }
            else
            {
                page.Parent.MaxPages = this.ExtractMaxForumPages(maxPagesNode);
                Awful.Core.Event.Logger.AddEntry(string.Format("AwfulForumPage - maxPagesNode parsed. Value: {0}", page.Parent.MaxPages));
            }
        }

        private int ExtractMaxForumPages(HtmlNode node)
        {
            var text = node.InnerHtml.Sanitize();
            var tokens = text.Split(' ');

            if (tokens.Length == 1)
                return 1;

            var number = tokens[1];
            number = number.Replace("(", "");
            number = number.Replace("):", "");

            int result;
            Int32.TryParse(number, out result);
            return result == 0 ? 1 : result;
        }

        private void HandleThreads(SAForumPage page, HtmlNode node)
        {
            var forumThreadsTable = node.Descendants("table")
                   .Where(n => n.Id.Equals("forum"))
                   .First();

            var threadList = forumThreadsTable.Descendants("tbody").First();
            var threadsInfo = threadList.Descendants("tr");

            page.Threads = this.GenerateThreadData(page, threadsInfo);
        }

        private IList<ThreadData> GenerateThreadData(SAForumPage page, IEnumerable<HtmlNode> threadsInfo)
        {
            Awful.Core.Event.Logger.AddEntry("AwfulForumPage - Generating thread data...");

            List<ThreadData> data = new List<ThreadData>();
            foreach (var node in threadsInfo)
            {
                SAThread thread = SAThreadFactory.Build(node, page.ForumID);
                data.Add(thread);
            }

            data.Sort(SortThreadsByNewPostCount.Comparer);
            return data;
        }

        private void OnDoWork(object sender, DoWorkEventArgs e)
        {
            SAForumPage page = e.Argument as SAForumPage;
            if (page == null)
            {
                e.Cancel = true;
                return;
            }

            Awful.Core.Models.ActionResult success = Awful.Core.Models.ActionResult.Failure;

            Factory._web = new WebGet();
            Factory._web.LoadAsync(page.Url, (result, doc) =>
                {
                    switch (result)
                    {
                        case Awful.Core.Models.ActionResult.Success:
                            success = Factory.BuildPageFromHtml(page, doc);
                            break;

                        case Awful.Core.Models.ActionResult.Failure:
                            success = Awful.Core.Models.ActionResult.Failure;
                            break;

                        default:
                            success = Awful.Core.Models.ActionResult.Cancelled;
                            e.Cancel = true;
                            break;
                    }

                    Factory._signal.Set();
                });

            Factory._signal.WaitOne();

            if (Factory._worker.CancellationPending) { e.Cancel = true; return; }
            else
            {
                if (success == Awful.Core.Models.ActionResult.Success)
                {
                    e.Result = page;
                    return;
                }
            }
        }

        public static void BuildAsync(Action<Awful.Core.Models.ActionResult> result, SAForumPage page)
        {
            if (Factory._worker.IsBusy)
            {
                result(Awful.Core.Models.ActionResult.Busy);
                return;
            }


            RunWorkerCompletedEventHandler completed = null;
            completed = (obj, args) =>
                {
                    Factory._worker.RunWorkerCompleted -= new RunWorkerCompletedEventHandler(completed);
                    
                    if (args.Cancelled)
                    {
                        result(Awful.Core.Models.ActionResult.Cancelled);
                    }

                    else
                    {
                        SAForumPage loaded = args.Result as SAForumPage;
                        if (loaded == null) { result(Awful.Core.Models.ActionResult.Failure); }
                        else
                        {
                            loaded.LastUpdated = DateTime.Now;
                            result(Awful.Core.Models.ActionResult.Success);
                        }
                    }
                };

            Factory._worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(completed);
            Factory._worker.RunWorkerAsync(page);
        }

        public static void CancelAsync()
        {
            if (Factory._web.IsBusy) { Factory._web.CancelAsync(); }
            else if (Factory._worker.IsBusy)
            {
                Factory._worker.CancelAsync();
                Factory._signal.Set();
            }
        }
    }
}
