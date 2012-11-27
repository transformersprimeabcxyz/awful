using System;
using System.Linq;
using Awful.Models;
using System.Collections.Generic;
using System.ComponentModel;
using HtmlAgilityPack;
using Awful.Helpers;
using System.Threading;
using System.Net;
using System.Data.Linq;
using KollaSoft;
using Awful.Core.Web;

namespace Awful.Services
{
    public interface IForumDataResponse
    {
        ICollection<SAForum> Forums { get; }
        ICollection<SAForum> Favorites { get; }
    }

    public class ForumDataResponse : IForumDataResponse
    {
        public ICollection<SAForum> Forums { get; set; }
        public ICollection<SAForum> Favorites { get; set; }
    }

    public class ForumDataRequest
    {
        private readonly BackgroundWorker worker = new BackgroundWorker();
        private readonly Data.SAForumDB db = new Data.SAForumDB();
        private bool _forceReload = false;
        private WebGet _webGet;

        public static event EventHandler<ForumsRefreshedEventArgs<SAForum>> ForumsRefreshed;

        private readonly List<string> forumBlackList = new List<string>()
        {
            "Main",
            "Discussion",
            "The Finer Arts",
            "The Community",
            "Archives",
            "The Crackhead Clubhouse",
            "Retarded Forum for Assholes",
        };

        public ForumDataRequest()
        {
            this.worker.WorkerSupportsCancellation = true;
            this.worker.DoWork += new DoWorkEventHandler(worker_DoWork);
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            ForumDataResponse response = new ForumDataResponse();

            if (this._forceReload == false)
            {
                ICollection<SAForum> forums = db.Forums.ToList();
                if (forums.Count != 0)
                {
                    response.Forums = forums;
                }
            }

            if (response.Forums == null)
            {
                response.Forums = this.LoadForumsFromWeb();
            }

            if (this.worker.CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            this.UpdateFavorites(response.Forums);
            e.Result = response;
        }

        private List<SAForum> LoadForumsFromWeb()
        {
            List<SAForum> result = null;
            this._webGet = new WebGet();
            AutoResetEvent waitSignal = new AutoResetEvent(false);
            HtmlDocument doc = null;

            this._webGet.LoadAsync("http://forums.somethingawful.com/forumdisplay.php?forumid=1", (action, docArgs) =>
            {
                switch (action)
                {
                    case Awful.Core.Models.ActionResult.Success:
                        doc = docArgs.Document;
                        break;

                    case Awful.Core.Models.ActionResult.Failure:
                        worker.CancelAsync();
                        break;
                }

                if (!worker.CancellationPending)
                {
                    result = ParseData(doc);
                    
                    if (result == null) { worker.CancelAsync(); }
                    else
                    {
                        var args = new ForumsRefreshedEventArgs<SAForum>(result);
                        ForumsRefreshed.Fire(this, args);
                    }
                }

                waitSignal.Set();
            });

            waitSignal.WaitOne();
            return result;
        }

        private void UpdateFavorites(IEnumerable<SAForum> forums)
        {
            if (forums == null) return;

            var current = db.Profiles.SingleOrDefault(p => p.ID == App.Settings.CurrentProfileID);
            if (current != null)
            {
                
                var query = from f in db.ForumFavorites
                            where f.ProfileID == current.ID && f.IsFavorite
                            select f.ForumID;

                var favs = query.ToList();

                foreach (var forum in forums)
                {
                    if (favs.Contains(forum.ID))
                    {
                        forum.IsFavorite = true;
                    }
                }
            }
        }

        private List<SAForum> ParseData(HtmlDocument doc)
        {
            if(doc == null)
                return null;

            var parent = doc.DocumentNode;
            List<SAForum> forums = new List<SAForum>(100);

            var selectNode = parent.Descendants("select")
                .Where(node => node.GetAttributeValue("name", "").Equals("forumid"))
                .FirstOrDefault();
            
            if (selectNode != null)
            {
                var forumNodes = selectNode.Descendants("option").ToArray();

                foreach (var node in forumNodes)
                {
                    if (worker.CancellationPending)
                        return null;
                    
                    var value = node.Attributes["value"].Value;
                    int id = 0;
                    if (Int32.TryParse(value, out id) && id > 0)
                    {
                        string name = node.NextSibling.InnerText;
                        name = HttpUtility.HtmlDecode(name);
                        if (name != String.Empty)
                        {
                            name = name.Replace("-", "");
                            name = name.Trim();
                            
                            if (!forumBlackList.Contains(name))
                            {
                                var forum = new SAForum() { ID = id, ForumName = name };
                                Data.SAForumDB.SetDefaultMapping(forum);
                                forums.Add(forum);
                            }
                        }
                    }
                }
            }

            return forums;               
        }

        public void CancelAsync()
        {
            if (worker.IsBusy) 
            {
                if (this._webGet != null) { this._webGet.CancelAsync(); }
                worker.CancelAsync(); 
            }
        }

        public void GetForumList(bool ignoreCache, Action<Awful.Core.Models.ActionResult, IForumDataResponse> action)
        {
            try
            {
                if (worker.IsBusy)
                {
                    action(Awful.Core.Models.ActionResult.Busy, null);
                }

                else
                {
                    this._forceReload = ignoreCache;

                    RunWorkerCompletedEventHandler completed = null;
                    completed = (obj, args) =>
                        {
                            worker.RunWorkerCompleted -= new RunWorkerCompletedEventHandler(completed);
                            if (args.Cancelled)
                                action(Awful.Core.Models.ActionResult.Cancelled, null);
                            else
                                action(Awful.Core.Models.ActionResult.Success, args.Result as IForumDataResponse);
                        };

                    worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(completed);
                    worker.RunWorkerAsync();
                }
            }
            catch (Exception ex)
            {
                string error = string.Format("GetForumList: An error occured while trying to get the forum collection. [{0}] - {1}",
                    ex.Message, ex.StackTrace);
                Awful.Core.Event.Logger.AddEntry(error);
                action(Awful.Core.Models.ActionResult.Failure, null);
            }
        }
    }
}