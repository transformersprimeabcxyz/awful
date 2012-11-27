using System;
using System.Linq;
using HtmlAgilityPack;
using Awful.Models;
using System.Threading;
using System.ComponentModel;
using System.Collections.Generic;
using System.Data.Linq;
using System.Windows;
using KollaSoft;
using System.Net;
using Awful.Core.Web;

namespace Awful.Helpers
{
    public class SAThreadPageFactory
    {
        private static readonly SAThreadPageFactory Factory = new SAThreadPageFactory();       
        private SAThreadPageFactory() 
        {
          
        }

        private HtmlDocument LoadHtmlFromWeb(SAThreadPage page)
        {
            HtmlDocument doc = null;
            var signal = new AutoResetEvent(false);
            var web = new WebGet();
            web.LoadAsync(page.Url, (result, docArgs) =>
            {
                switch (result)
                {
                    case Awful.Core.Models.ActionResult.Success:
                        doc = docArgs.Document;
                        if (page.PageNumber == 0) { page.PageNumber = this.ProcessPageNumber(docArgs.Url); }
                        break;

                    case Awful.Core.Models.ActionResult.Busy:
                        break;

                    case Awful.Core.Models.ActionResult.Failure:
                        break;
                }

                signal.Set();
            });

            signal.WaitOne();

            return doc;
        }

        private Awful.Core.Models.ActionResult ProcessData(SAThreadPage page, HtmlDocument args)
        {
            try
            {
                this.ProcessThreadPage(page, args.DocumentNode);
                string content = PostWebViewContentItemBuilder.MergePostsToHtml(page.Posts);
                string savedHtmlPath = string.Format("{0}\\{1}_{2}.html", 
                    Globals.Constants.THREADPAGE_DIRECTORY,
                    page.ThreadID, 
                    page.PageNumber);

                content.SaveTextToFile(savedHtmlPath, "\\");
                page.HtmlPath = savedHtmlPath;
                
                return Awful.Core.Models.ActionResult.Success;
            }

            catch (Exception ex)
            {
                string error = string.Format("An error occured while processing thread page data. [{0}] {1}",
                    ex.Message, ex.StackTrace);

                Awful.Core.Event.Logger.AddEntry(error);
                return Awful.Core.Models.ActionResult.Failure;
            }
        }

        private int ProcessPageNumber(string url)
        {
            Awful.Core.Event.Logger.AddEntry(string.Format("SAThreadPage - Parsing Page Number from '{0}'...", url));

            string pageNumberQuery = url.Split('#').First();
            pageNumberQuery = pageNumberQuery.Split('&').Last();
            pageNumberQuery = pageNumberQuery.Replace("pagenumber=", "");

            int pageNumber = 0;
            if (!int.TryParse(pageNumberQuery, out pageNumber))
                pageNumber = 1;
            
            Awful.Core.Event.Logger.AddEntry(string.Format("SAThreadPage - Page number is {0}.", pageNumber));
            return pageNumber;
        }

        public static void Process(SAThreadPage page, HtmlNode parent)
        {
            Factory.ProcessThreadPage(page, parent);
        }

        private void ProcessThreadPage(SAThreadPage page, HtmlNode parent)
        {
            Awful.Core.Event.Logger.AddEntry("SAThreadPage - Parsing HTML for posts...");

            var maxPagesNode = parent.Descendants("div")
               .Where(node => node.GetAttributeValue("class", "").Equals("pages top"))
               .FirstOrDefault();

            var threadTitleNode = parent.Descendants("a")
                .Where(node => node.GetAttributeValue("class", "").Contains("bclast"))
                .FirstOrDefault();

            if (threadTitleNode != null)
            {
                // decode this title
                string inner = HttpUtility.HtmlDecode(threadTitleNode.InnerText);
                (page.Thread as SAThread).ThreadTitle = inner;
                page.ThreadTitle = inner;
            }

            if (maxPagesNode != null)
            {
                page.Thread.MaxPages = ParseMaxPagesNode(maxPagesNode);
                page.MaxPages = page.Thread.MaxPages;

                Awful.Core.Event.Logger.AddEntry(string.Format("SAThreadPage - maxPagesNode found: {0}", page.MaxPages));
            }

            page.Posts = new List<PostData>();

            var postArray = parent.Descendants("table")
                .Where(tables => tables.GetAttributeValue("class", "").Equals("post"))
                .ToArray();

            int index = 1;

            foreach (var postNode in postArray)
            {
                SAPost post = SAPostFactory.Build(postNode, page);
                post.PostIndex = index;
                page.Posts.Add(post);
                index++;
            }

            int postID = page.Thread.LastViewedPostID;
            if (postID != SAPost.NULL_POSTID)
            {
                PostData selected = null;
                selected = page.Posts.Where(post => (post as SAPost).ID == postID).FirstOrDefault();

                if (selected != null)
                {
                    page.Thread.LastViewedPostIndex = (selected as SAPost).PostIndex - 1;
                    page.Thread.LastViewedPostID = SAPost.NULL_POSTID;
                }
            }
        }

        private int ParseMaxPagesNode(HtmlNode maxPagesNode)
        {
            // should look something like "Pages ([numbers])"...
            string text = maxPagesNode.InnerText;
            var tokens = text.Split(' ');

            // tokens should be ["Pages"], ["(<Last Page Number>)"], ...
            string pageToken = tokens[1];

            // remove garbage characters
            pageToken = pageToken.Replace("(", "");
            pageToken = pageToken.Replace(")", "");
            pageToken = pageToken.Replace(":", "");

            // extract page number
            int result = 1;
            Int32.TryParse(pageToken, out result);
            return result == 0 ? 1 : result;
        }

        private SAThreadPage LoadFromDatabase(SAThread thread, int pageNumber)
        {
            SAThreadPage page = null;
            
            /// only do work if the pageNumber is nonzero and positive. Should probably throw an exception here...
            if (pageNumber >= 1)
            {
                page = ThreadCache.GetPageFromCache(thread, pageNumber);
            }

            return page;
        }

        private SAThreadPage LoadFromDatabase(SAThreadPage page)
        {
            if (page.PageNumber >= 1)
            {
                page = ThreadCache.GetPageFromCache(page.Thread, page.PageNumber);
            }

            return page;
        }

        private SAThreadPage LoadFromWeb(SAThread thread, int pageNumber, int userID)
        {
            SAThreadPage page = null;
            
            /// Only do work on nonnegative page numbers. I think I should throw an exception here, to be honest...
            if (pageNumber >= 0)
            {
                page = new SAThreadPage(thread, pageNumber, userID);
                HtmlDocument doc = this.LoadHtmlFromWeb(page);
                if (doc != null)
                {
                    Awful.Core.Models.ActionResult result = this.ProcessData(page, doc);
                    if (result == Awful.Core.Models.ActionResult.Failure) return null;
                }
            }

            if (page.PageNumber > 0 && userID == 0)
            {
                ThreadCache.AddPageToCache(page);
            }

            return page;
        }

        private Awful.Core.Models.ActionResult LoadFromWeb(SAThreadPage page)
        {
            HtmlDocument doc = this.LoadHtmlFromWeb(page);
            if (doc != null)
            {
                Awful.Core.Models.ActionResult result = this.ProcessData(page, doc);
                return result;
            }

            return Awful.Core.Models.ActionResult.Failure;
        }

        private void AddDoWorkEvent(BackgroundWorker worker, SAThread thread, bool refresh, int pageNumber, int userID)
        {
            DoWorkEventHandler doWork = null;
            doWork = (doWorkObj, doWorkArgs) =>
            {
                BackgroundWorker bgWorker = doWorkObj as BackgroundWorker;
                bgWorker.DoWork -= new DoWorkEventHandler(doWork);

                SAThreadPage page = null;
                bool finished = false;

                ThreadPool.QueueUserWorkItem(state =>
                {
                    page = Build(thread, refresh, pageNumber, userID);
                    finished = true;

                }, null);

                while (!finished && !doWorkArgs.Cancel)
                {
                    if (bgWorker.CancellationPending) { doWorkArgs.Cancel = true; }
                }

                doWorkArgs.Result = page;
            };

            worker.DoWork += new DoWorkEventHandler(doWork);
        }

        private void AddRunWorkCompletedEvent(BackgroundWorker worker, Action<Awful.Core.Models.ActionResult, SAThreadPage> result)
        {
            RunWorkerCompletedEventHandler completed = null;
            completed = (obj, args) =>
            {
                BackgroundWorker bgWorker = obj as BackgroundWorker;
                bgWorker.RunWorkerCompleted -= new RunWorkerCompletedEventHandler(completed);

                if (args.Cancelled)
                {
                    result(Awful.Core.Models.ActionResult.Cancelled, null);
                }
                else if (args.Result == null)
                {
                    result(Awful.Core.Models.ActionResult.Failure, null);
                }
                else
                {
                    SAThreadPage page = args.Result as SAThreadPage;
                    result(Awful.Core.Models.ActionResult.Success, page);
                }
            };

            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(completed);
        }

        public static SAThreadPage Build(SAThread thread, bool refresh = false, int pageNumber = 0, int userID = 0)
        {
            SAThreadPage page = null;

            // check database for page first (if not refreshing).
            if (userID == 0 && refresh == false)
            {
                page = Factory.LoadFromDatabase(thread, pageNumber);
            }
            
            if (page == null)
            {
                page = Factory.LoadFromWeb(thread, pageNumber, userID);
            }

            return page;
        }

        public static ICancellable BuildAsync(Action<Awful.Core.Models.ActionResult, SAThreadPage> result, SAThread thread,
            bool refresh = false, int pageNumber = 0, int userID = 0)
        {
            BackgroundWorker worker = new BackgroundWorker();

            Factory.AddDoWorkEvent(worker, thread, refresh, pageNumber, userID);
            Factory.AddRunWorkCompletedEvent(worker, result);

            CancellableTask task = new CancellableTask(worker);
            worker.RunWorkerAsync();
            return task;
        }

        public static Awful.Core.Models.ActionResult Build(SAThreadPage page)
        {
            // check database for page first (if not refreshing).

            var result = Awful.Core.Models.ActionResult.Failure;
            result = Factory.LoadFromWeb(page);
            return result;
        }

        public static ICancellable BuildAsync(Action<Awful.Core.Models.ActionResult, SAThreadPage> result, SAThreadPage page)
        {
            TaskCompletedDelegate completed = (obj, args) =>
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            if (args.Cancelled) { result(Awful.Core.Models.ActionResult.Cancelled, page); }
                            else
                            {
                                Awful.Core.Models.ActionResult ar = (Awful.Core.Models.ActionResult)args.Result;
                                result(ar, page);
                            }
                        });
                };

            DoWorkDelegate work = Factory.BuildAsync_DoWork;
            CancellableTask task = new CancellableTask(work, completed);
            task.Worker.RunWorkerAsync(page);
            return task;
        }

        private void BuildAsync_DoWork(object sender, DoWorkEventArgs args)
        {
            SAThreadPage page = args.Argument as SAThreadPage;
            if (page == null) { args.Result = Awful.Core.Models.ActionResult.Failure; return; }
            else
            {
                BackgroundWorker worker = sender as BackgroundWorker;
                if (worker == null) { args.Result = Awful.Core.Models.ActionResult.Failure; return; }

                Awful.Core.Models.ActionResult result = Awful.Core.Models.ActionResult.Failure;
                bool finished = false;

                ThreadPool.QueueUserWorkItem(state => 
                { 
                    result = Factory.LoadFromWeb(page); 
                    finished = true; 
                }, 
                null);

                while (!args.Cancel && !finished)
                {
                    if (worker.CancellationPending) { args.Cancel = true; }
                }

                args.Result = result;
            }
        }
    }
}
