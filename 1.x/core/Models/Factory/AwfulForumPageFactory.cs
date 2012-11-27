using System;
using System.Linq;
using Awful.Core.Models;
using Awful.Core.Web;
using HtmlAgilityPack;
using System.Threading;
using System.ComponentModel;
using System.Collections.Generic;
using KollaSoft;
using Awful.Core.Event;
using Awful.Core.Models.Sort;

namespace Awful.Core.Models.Factory
{
    public class AwfulForumPageFactory : AbstractPageFactory<AwfulForumPage>
    {
        private static AwfulForumPageFactory Factory;

        static AwfulForumPageFactory() { Factory = new AwfulForumPageFactory(); }

        private AwfulForumPageFactory() : base() { }

        public static void BuildAsync(AwfulForumPage page, Action<AwfulForumPage> result)
        {
            Factory.BuildPage(page, result);
        }

        private void HandleMaxPages(ForumPageData page, HtmlNode node)
        {
            var maxPagesNode = node.Descendants("div")
                .Where(n => n.GetAttributeValue("class", "").Equals("pages"))
                .FirstOrDefault();

            if (maxPagesNode == null)
            {
                Logger.AddEntry("AwfulForumPage - Could not parse maxPagesNode.");
                page.Parent.TotalPages = 1;
            }
            else
            {
                page.Parent.TotalPages = this.ExtractMaxForumPages(maxPagesNode);
                Logger.AddEntry(string.Format("AwfulForumPage - maxPagesNode parsed. Value: {0}", page.Parent.TotalPages));
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

        private void HandleThreads(AwfulForumPage page, HtmlNode node)
        {
            var forumThreadsTable = node.Descendants("table")
                   .Where(n => n.Id.Equals("forum"))
                   .First();

            var threadList = forumThreadsTable.Descendants("tbody").First();
            var threadsInfo = threadList.Descendants("tr");

            page.Threads = this.GenerateThreadData(page, threadsInfo);
        }

        private IList<AwfulThread> GenerateThreadData(AwfulForumPage page, IEnumerable<HtmlNode> threadsInfo)
        {
            Logger.AddEntry("AwfulForumPage - Generating thread data...");

            List<AwfulThread> data = new List<AwfulThread>();
            foreach (var node in threadsInfo)
            {
                AwfulThread thread = AwfulThreadFactory.Build(node, page.ForumID);
                data.Add(thread);
            }

            data.Sort(SortThreadsByNewPostCount.Comparer);
            return data;
        }

        protected override AwfulForumPage BuildItem(HtmlNode node, AwfulForumPage page)
        {
            try
            {
                this.HandleMaxPages(page, node);
                this.HandleThreads(page, node);
                return page;
            }
            catch (Exception) { return null; }
        }
    }
}
