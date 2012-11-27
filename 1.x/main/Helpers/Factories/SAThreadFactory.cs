using System;
using System.Linq;
using HtmlAgilityPack;
using Awful.Models;
using System.Data.Linq;
using System.Net;
using KollaSoft;

namespace Awful.Helpers
{
    public class SAThreadFactory
    {
        private static readonly SAThreadFactory Factory = new SAThreadFactory();

        private SAThreadFactory() { }

        public static SAThread Build(HtmlNode node, int forumID)
        {
            Awful.Core.Event.Logger.AddEntry("SAThread - Node Html Print:");
            Awful.Core.Event.Logger.AddEntry("SAThread ------------------");
            Awful.Core.Event.Logger.AddEntry(node.OuterHtml);
            Awful.Core.Event.Logger.AddEntry("SAThread ------------------");

            SAThread result = null;
            int id = Factory.GetThreadID(node);

            result = new SAThread();
            result.ID = id;
            result.ForumID = forumID;

            Factory.ParseThreadSeen(result, node);
            Factory.ParseThreadTitleAndUrl(result, node);
            Factory.ParseThreadAuthor(result, node);
            Factory.ParseReplies(result, node);
            Factory.ParseRating(result, node);
            Factory.ParseSticky(result, node);
            result.LastUpdated = DateTime.Now;

            return result;
        }

        private void ParseSticky(SAThread thread, HtmlNode node)
        {
            var stickyNode = node.Descendants("td").Where(aNode => aNode.GetAttributeValue("class", "")
                .Contains("sticky")).FirstOrDefault();

            thread.IsSticky = stickyNode != null;
        }

        private void ParseRating(SAThread thread, HtmlNode node)
        {
            var ratingNode = node.Descendants("img")
                .Where(imgNode =>
                {
                    string src = imgNode.GetAttributeValue("src", "");
                    return src.Contains("rate");
                })
                .FirstOrDefault();

            if (ratingNode == null)
                thread.Rating = 0;

            else
            {
                string src = ratingNode.GetAttributeValue("src", "");
                var tokens = src.Split('/');
                var ratingToken = tokens[tokens.Length - 1];
                switch (ratingToken)
                {
                    case Globals.Constants.THREAD_RATING_5:
                        thread.Rating = 5;
                        break;

                    case Globals.Constants.THREAD_RATING_4:
                        thread.Rating = 4;
                        break;

                    case Globals.Constants.THREAD_RATING_3:
                        thread.Rating = 3;
                        break;

                    case Globals.Constants.THREAD_RATING_2:
                        thread.Rating = 2;
                        break;

                    case Globals.Constants.THREAD_RATING_1:
                        thread.Rating = 1;
                        break;
                }
            }
        }

        private void ParseThreadCount(SAThread thread, HtmlNode node)
        {
            // locate the thread count
            var threadCountNode = node.Descendants("a")
                .Where(value => value.GetAttributeValue("class", "").Equals("count"))
                .FirstOrDefault();

            // if we found the new post count, get and set the value
            if (threadCountNode != null)
            {
                #region if we found the thread count...

                int count = -1;
                if (Int32.TryParse(threadCountNode.InnerText.Sanitize(), out count))
                {
                    thread.NewPostCount = count;
                    thread.ShowPostCount = true;
                    Awful.Core.Event.Logger.AddEntry(string.Format("SAThread - Thread has new unread posts: {0}", count));
                }

                else
                {
                    // no new posts, set to maximum int value for low score sorting
                    thread.NewPostCount = Int32.MaxValue;
                    thread.ShowPostCount = true;
                    Awful.Core.Event.Logger.AddEntry("SAThread - Thread has no new posts.");
                }

                if (count > 0)
                {
                    int readPostCount = thread.Replies - count;
                    int postsPerPage = Globals.Constants.POSTS_PER_THREAD_PAGE;
                    int readPage = (readPostCount / postsPerPage) + (readPostCount % postsPerPage > 0 ? 1 : 0);
                    Awful.Core.Event.Logger.AddEntry(string.Format("SAThread - posts read: {0}, last page: {1}", readPostCount, thread.MaxPages));
                }

                #endregion
            }
            else
            {
                Awful.Core.Event.Logger.AddEntry("SAThread - Couldn't find the threadCountNode. no new posts.");
                thread.NewPostCount = Int32.MaxValue;
            }
        }

        private void ParseReplies(SAThread thread, HtmlNode node)
        {
            var threadRepliesNode = node.Descendants("td")
                .Where(value => value.GetAttributeValue("class", "").Equals("replies"))
                .FirstOrDefault();

            try
            {
                string repliesValue = threadRepliesNode.InnerText.Sanitize();
                int replies = 0;
                if (Int32.TryParse(repliesValue, out replies))
                {
                    thread.Replies = replies;
                    Awful.Core.Event.Logger.AddEntry(string.Format("SAThread - # of replies: {0}", replies));
                }

                int postsPerPage = Globals.Constants.POSTS_PER_THREAD_PAGE;

                thread.MaxPages = (replies / postsPerPage) + (replies % postsPerPage > 0 ? 1 : 0);

                Awful.Core.Event.Logger.AddEntry(string.Format("SAThread - Max Pages: {0}", thread.MaxPages));
            }

            catch (Exception ex)
            {
                Awful.Core.Event.Logger.AddEntry(string.Format("SAThread - Exception thrown while parsing replies: {0}",
                    ex.Message));
            }
        }

        private void ParseThreadAuthor(SAThread thread, HtmlNode node)
        {
            var threadAuthorParentNode = node.Descendants("td")
              .Where(value => value.GetAttributeValue("class", "").Equals("author"))
              .FirstOrDefault();

            thread.AuthorName = threadAuthorParentNode.FirstChild.InnerText;
            Awful.Core.Event.Logger.AddEntry(string.Format("SAThread - Author Name: {0}", thread.AuthorName));
        }

        private void ParseThreadTitleAndUrl(SAThread thread, HtmlNode node)
        {
            var threadTitleNode = node.Descendants("a")
               .Where(value => value.GetAttributeValue("class", "").Equals("thread_title"))
               .FirstOrDefault();

            var title = threadTitleNode.InnerText.Sanitize();
            thread.ThreadTitle = HttpUtility.HtmlDecode(title);
            thread.ThreadTitle = ContentFilter.Censor(thread.ThreadTitle);

            Awful.Core.Event.Logger.AddEntry(string.Format("SAThread - Thread Title: {0}", thread.ThreadTitle));

            thread.ThreadURL = Globals.Constants.SA_BASE + "/" + threadTitleNode.GetAttributeValue("href", "");
            Awful.Core.Event.Logger.AddEntry(string.Format("SAThread - Thread Url: '{0}'", thread.ThreadURL));
        }

        private void ParseThreadSeen(SAThread thread, HtmlNode node)
        {
            // if the node is null, then we haven't seen this thread, otherwise it's been visited
            var threadSeenNode = node.DescendantsAndSelf()
               .Where(value => value.GetAttributeValue("class", "").Contains("thread seen"))
               .FirstOrDefault();

            bool seen = threadSeenNode == null ? false : true;
            thread.ThreadSeen = seen;

            // if thread is new, all posts are new, so don't show post count
            if (!thread.ThreadSeen)
            {
                thread.NewPostCount = -1;
                thread.ShowPostCount = false;
                Awful.Core.Event.Logger.AddEntry("SAThread - This thread is brand new! Hide the post count.");
            }

            // else parse thread count
            else { this.ParseThreadCount(thread, node); }
        }

        private int GetThreadID(HtmlNode node)
        {
            var threadIDNode = node.DescendantsAndSelf()
                 .Where(value => value.GetAttributeValue("id", "") != null)
                 .FirstOrDefault();

            string id = threadIDNode.GetAttributeValue("id", "").Trim();
            id = id.Replace("thread", "");

            int parsedID = -1;
            int.TryParse(id, out parsedID);
            Awful.Core.Event.Logger.AddEntry(string.Format("SAThread - ThreadID: {0}", id));

            return parsedID;
        }
    }
}
