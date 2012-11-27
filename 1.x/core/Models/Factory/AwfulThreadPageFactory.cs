using System;
using System.Linq;
using HtmlAgilityPack;
using Awful.Core.Models;
using System.Threading;
using System.ComponentModel;
using System.Collections.Generic;
using System.Data.Linq;
using System.Windows;
using KollaSoft;
using Awful.Core.Web;
using Awful.Core.Event;
using System.Net;

namespace Awful.Core.Models.Factory
{
    public class AwfulThreadPageFactory : Interfaces.Factory<AwfulThreadPage>
    {
        private static readonly AwfulThreadPageFactory _factory = new AwfulThreadPageFactory();

        private AwfulThreadPageFactory() { }

        private AwfulThreadPage ProcessData(HtmlDocument args)
        {
            AwfulThreadPage page = null;
            try
            {
                page = this.ProcessThreadPage(args.DocumentNode);     
            }

            catch (Exception ex)
            {
                Logger.AddEntry("An error occured while processing thread page data:", ex);
                page = null;
            }
            return page;
        }

        private AwfulThread ProcessParent(HtmlNode top)
        {
            AwfulThread thread = new AwfulThread();
            var threadNode = top.Descendants()
                .Where(node => node.GetAttributeValue("class", "").Equals("bclast"))
                .FirstOrDefault();

            if (threadNode != null)
            {
                int id = -1;
                string idString = threadNode.GetAttributeValue("href", "");
                idString = idString.Split('=')[1];
                string title = HttpUtility.HtmlDecode(threadNode.InnerText);
                
                thread.Title = title;
                if (int.TryParse(idString, out id)) { thread.ID = id; }
            }

            return thread;
        }

        private AwfulThreadPage ProcessThreadPage(HtmlNode top)
        {
            Logger.AddEntry("AwfulThreadPage - Parsing HTML for posts...");
            
            // first, let's generate data about the thread
            AwfulThread parent = this.ProcessParent(top);

            // now, let's parse page number
            int pageNumber = -1;
            var currentPageNode = top.Descendants("span")
                .Where(node => node.GetAttributeValue("class", "").Equals("curpage"))
                .FirstOrDefault();

            if (currentPageNode != null) { int.TryParse(currentPageNode.InnerText, out pageNumber); }
            
            // create page instance
            AwfulThreadPage page = new AwfulThreadPage(parent, pageNumber);
            
            // parse other thread page data
            var maxPagesNode = top.Descendants("div")
               .Where(node => node.GetAttributeValue("class", "").Equals("pages top"))
               .FirstOrDefault();

            if (maxPagesNode != null)
            {
                int totalPages = ParseMaxPagesNode(maxPagesNode);
                page.Parent.TotalPages = totalPages;
                Logger.AddEntry(string.Format("AwfulThreadPage - maxPagesNode found: {0}", totalPages));
            }

            if (page.Posts == null) { page.Posts = new EntitySet<AwfulPost>(); }

            var postArray = top.Descendants("table")
                .Where(tables => tables.GetAttributeValue("class", "").Equals("post"))
                .ToArray();

            int index = 1;

            foreach (var postNode in postArray)
            {
                AwfulPost post = AwfulPostFactory.Build(postNode, page);
                post.PostIndex = index;
                page.Posts.Add(post);
                index++;
            }

            return page;
        }

        private int ProcessPageNumber(string url)
        {
            Logger.AddEntry(string.Format("AwfulThreadPage - Parsing Page Number from '{0}'...", url));

            string pageNumberQuery = url.Split('#').First();
            pageNumberQuery = pageNumberQuery.Split('&').Last();
            pageNumberQuery = pageNumberQuery.Replace("pagenumber=", "");

            int pageNumber = 0;
            if (!int.TryParse(pageNumberQuery, out pageNumber))
                pageNumber = 1;

            Logger.AddEntry(string.Format("AwfulThreadPage - Page number is {0}.", pageNumber));
            return pageNumber;
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

        public static AwfulThreadPage Build(HtmlDocument doc)
        {
            return _factory.ProcessData(doc);
        }

        public AwfulThreadPage Build() { throw new NotImplementedException(); }
    }
}
