using System;
using Awful.Core.Web;
using Awful.Core.Models;
using System.Collections.Generic;
using Awful.Core.Web.Parsers;
using System.Text;
using Awful.Core.Models.Factory;

namespace Awful.Services
{
    public class AwfulForumsService
    {
        public static readonly AwfulForumsService Service;

        static AwfulForumsService() { Service = new AwfulForumsService(); }

        private AwfulForumsService() { }

        public void FetchAllForums(Action<ActionResult, IEnumerable<ForumData>> action)
        {
            string url = string.Format("{0}/{1}?forumid=1", Constants.BASE_URL, Constants.FORUM_PAGE_URI);
            var web = new WebGet();
            web.LoadAsync(url, (ar, doc) =>
                {
                    IEnumerable<ForumData> result = null;
                    if (ar == ActionResult.Success)
                    {
                        result = AwfulForumParser.ParseForumList(doc.Document);
                    }
                    action(ar, result);
                });
        }

        public void FetchForumPage(ForumData forum, int pageNumber, Action<ForumPageData> action)
        {
            var url = new StringBuilder();
            // http://forums.somethingawful.com/forumdisplay.php
            url.AppendFormat("{0}/{1}", Constants.BASE_URL, Constants.FORUM_PAGE_URI);
            // ?forumid=<FORUMID>
            url.AppendFormat("?forumid={0}", forum.ID);
            // &daysprune=30&perpage=30&posticon=0sortorder=desc&sortfield=lastpost
            url.Append("&daysprune=30&perpage=30&posticon=0sortorder=desc&sortfield=lastpost");
            // &pagenumber=<PAGENUMBER>
            url.AppendFormat("&pagenumber={0}", pageNumber);

            var web = new WebGet();
            web.LoadAsync(url.ToString(), (ar, doc) =>
            {
                ForumPageData result = null;
                if (ar == ActionResult.Success)
                {
                    result = AwfulForumParser.ParseForumPage(doc.Document);
                }
                action(result);
            });
        }

        public void FetchThreadPage(ThreadData thread, int pageNumber, Action<ThreadPageData> action)
        {
            // http://forums.somethingawful.com/showthread.php?noseen=0&threadid=3439182&pagenumber=69
            var url = new StringBuilder();
            // http://forums.somethingawful.com/showthread.php
            url.AppendFormat("{0}/{1}", Constants.BASE_URL, Constants.THREAD_PAGE_URI);
            // noseen=0&threadid=<THREADID>&pagenumber=<PAGENUMBER>
            url.AppendFormat("noseen=0&threadid={0}&pagenumber={1}", thread.ID, pageNumber);

            var web = new WebGet();
            web.LoadAsync(url.ToString(), (ar, doc) =>
           { 
                ThreadPageData result = null;
                if (ar == ActionResult.Success)
                {
                    result = AwfulThreadParser.ParseFromThreadPage(doc.Document);
                }
                action(result);
            });
        }

        public void RefreshThreadPage(ThreadPageData page, Action<ThreadPageData> action)
        {
            string url = page.Url;
            var web = new WebGet();
            web.LoadAsync(url, (a, e) =>
                {
                    ThreadPageData result = null;
                    if (a == ActionResult.Success) 
                    { 
                        result = AwfulThreadParser.ParseFromThreadPage(e.Document); 
                    }

                    action(result);
                });
        }
    }
}
