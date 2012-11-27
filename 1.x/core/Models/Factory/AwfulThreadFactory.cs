using System;
using System.Linq;
using HtmlAgilityPack;
using Awful.Core.Models;
using System.Data.Linq;
using System.Net;
using KollaSoft;
using Awful.Core.Event;

namespace Awful.Core.Models.Factory
{
    public class AwfulThreadFactory
    {
        private static readonly AwfulThreadFactory Factory = new AwfulThreadFactory();

        private const int POSTS_PER_THREAD_PAGE = 40;
        private AwfulThreadFactory() { }

        public static AwfulThread Build(HtmlNode node, int forumID)
        {
            Logger.AddEntry("AwfulThread - Node Html Print:");
            Logger.AddEntry("AwfulThread ------------------");
            Logger.AddEntry(node.OuterHtml);
            Logger.AddEntry("AwfulThread ------------------");

            AwfulThread result = null;
            int id = Factory.GetThreadID(node);

            result = new AwfulThread();
            result.ID = id;
            Factory.ParseThreadSeen(result, node);
            Factory.ParseThreadTitleAndUrl(result, node);
            Factory.ParseThreadAuthor(result, node);
            Factory.ParseReplies(result, node);
            Factory.ParseRating(result, node);
            Factory.ParseSticky(result, node);
            result.LastUpdated = DateTime.Now;

            return result;
        }

        private void ParseSticky(AwfulThread thread, HtmlNode node)
        {
            var stickyNode = node.Descendants("td").Where(aNode => aNode.GetAttributeValue("class", "")
                .Contains("sticky")).FirstOrDefault();

            thread.IsSticky = stickyNode != null;
        }

        private void ParseRating(AwfulThread thread, HtmlNode node)
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
                    case Constants.THREAD_RATING_5:
                        thread.Rating = 5;
                        break;

                    case Constants.THREAD_RATING_4:
                        thread.Rating = 4;
                        break;

                    case Constants.THREAD_RATING_3:
                        thread.Rating = 3;
                        break;

                    case Constants.THREAD_RATING_2:
                        thread.Rating = 2;
                        break;

                    case Constants.THREAD_RATING_1:
                        thread.Rating = 1;
                        break;
                }
            }
        }

        private void ParseThreadCount(AwfulThread thread, HtmlNode node)
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
                    Logger.AddEntry(string.Format("AwfulThread - Thread has new unread posts: {0}", count));
                }

                else
                {
                    // no new posts, set to maximum int value for low score sorting
                    thread.NewPostCount = Int32.MaxValue;
                    thread.ShowPostCount = true;
                    Logger.AddEntry("AwfulThread - Thread has no new posts.");
                }

                if (count > 0)
                {
                    int readPostCount = thread.Replies - count;
                    int postsPerPage = Constants.POSTS_PER_THREAD_PAGE;
                    int readPage = (readPostCount / postsPerPage) + (readPostCount % postsPerPage > 0 ? 1 : 0);
                    Logger.AddEntry(string.Format("AwfulThread - posts read: {0}, last page: {1}", readPostCount, thread.TotalPages));
                }

                #endregion
            }
            else
            {
                Logger.AddEntry("AwfulThread - Couldn't find the threadCountNode. no new posts.");
                thread.NewPostCount = Int32.MaxValue;
            }
        }

        private void ParseReplies(AwfulThread thread, HtmlNode node)
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
                    Logger.AddEntry(string.Format("AwfulThread - # of replies: {0}", replies));
                }

                int postsPerPage = POSTS_PER_THREAD_PAGE;

                thread.TotalPages = (replies / postsPerPage) + (replies % postsPerPage > 0 ? 1 : 0);

                Logger.AddEntry(string.Format("AwfulThread - Max Pages: {0}", thread.TotalPages));
            }

            catch (Exception ex)
            {
                Logger.AddEntry(string.Format("AwfulThread - Exception thrown while parsing replies: {0}",
                    ex.Message));
            }
        }

        private void ParseThreadAuthor(AwfulThread thread, HtmlNode node)
        {
            var threadAuthorParentNode = node.Descendants("td")
              .Where(value => value.GetAttributeValue("class", "").Equals("author"))
              .FirstOrDefault();

            thread.Author = threadAuthorParentNode.FirstChild.InnerText;
            Logger.AddEntry(string.Format("AwfulThread - Author Name: {0}", thread.Author));
        }

        private void ParseThreadTitleAndUrl(AwfulThread thread, HtmlNode node)
        {
            var threadTitleNode = node.Descendants("a")
               .Where(value => value.GetAttributeValue("class", "").Equals("thread_title"))
               .FirstOrDefault();

            var title = threadTitleNode.InnerText.Sanitize();
            thread.Title = HttpUtility.HtmlDecode(title);

            Logger.AddEntry(string.Format("AwfulThread - Thread Title: {0}", thread.Title));

            //thread.PageUri = Constants.BASE_URL + "/" + threadTitleNode.GetAttributeValue("href", "");
            Logger.AddEntry(string.Format("AwfulThread - Thread Url: '{0}'", thread.Url));
        }

        private void ParseThreadSeen(AwfulThread thread, HtmlNode node)
        {
            // if the node is null, then we haven't seen this thread, otherwise it's been visited
            var threadSeenNode = node.DescendantsAndSelf()
               .Where(value => value.GetAttributeValue("class", "").Contains("thread seen"))
               .FirstOrDefault();

            bool seen = threadSeenNode == null ? false : true;
            thread.LastRead = seen;

            // if thread is new, all posts are new, so don't show post count
            if (!thread.LastRead)
            {
                thread.NewPostCount = -1;
                thread.ShowPostCount = false;
                Logger.AddEntry("AwfulThread - This thread is brand new! Hide the post count.");
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
            Logger.AddEntry(string.Format("AwfulThread - ThreadID: {0}", id));

            return parsedID;
        }
    }
}
