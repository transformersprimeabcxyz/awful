using System;
using System.Linq;
using Awful.Models;
using Awful.Data;
using HtmlAgilityPack;
using System.Data.Linq;
using System.Net;
using KollaSoft;

namespace Awful.Helpers
{
    public class SAPostFactory
    {
        private SAPostFactory() { }
        private static readonly SAPostFactory Factory = new SAPostFactory();

        private const string HAS_SEEN_FLAG = "seen1";
        private const string HAS_NOT_SEEN_URL = "http://fi.somethingawful.com/style/posticon-new.gif";

        public static SAPost Build(HtmlNode node, SAThreadPage page)
        {
            SAPost result = null;
            Awful.Core.Event.Logger.AddEntry("SAPost - Parsing postNode for html...");

            int id = Factory.ParsePostID(node);
            try
            {
                result = new SAPost();
                result.ID = id;
                result.ThreadPageID = page.ID;
                Factory.ParseSeenUrl(result, node);
                Factory.ParseAuthor(result, node);
                Factory.ParsePostDate(result, node);
                Factory.ParseUserID(result, node);
                result.ContentNode = Factory.ParseContent(node);
                Factory.ParseHasSeen(result, node);
                Factory.ParseIcon(result, node);
            }

            catch (Exception ex)
            {
                string error = string.Format("An error occured while processing the post to the database. [{0}] {1}",
                    ex.Message, ex.StackTrace);

                Awful.Core.Event.Logger.AddEntry(error);
            }

            return result;
        }

        private HtmlNode ParseContent(HtmlNode postNode)
        {
            var content = postNode.Descendants("td")
                .Where(node => node.GetAttributeValue("class", "")
                    .Equals("postbody"))
                .FirstOrDefault();

            if (content == null)
                throw new ArgumentException("Content should not be null");

            return content;
        }

        private void ParseIcon(SAPost post, HtmlNode postNode)
        {
            try
            {
                 post.PostIconUri = postNode.Descendants()
                    .Where(node => node.GetAttributeValue("class", "").Equals("title"))
                    .First()
                    .Descendants("img")
                    .First()
                    .GetAttributeValue("src", "");

                post.ShowPostIcon = true;
            }

            catch (Exception)
            {
                post.PostIconUri = null;
                post.ShowPostIcon = false;
            }
        }

        private void ParseUserID(SAPost post, HtmlNode postNode)
        {
            var userIDNode = postNode.Descendants()
                .Where(node => node.GetAttributeValue("class", "").Contains("userid"))
                .FirstOrDefault();

            if (userIDNode != null)
            {
                string value = userIDNode.GetAttributeValue("class", "");
                value = value.Replace("userinfo userid-", "");
                int userid = 0;
                int.TryParse(value, out userid);
                post.UserID = userid;
            }
        }

        private void ParsePostDate(SAPost post, HtmlNode postNode)
        {
            var postDateNode = postNode.Descendants()
              .Where(node => node.GetAttributeValue("class", "").Equals("postdate"))
              .FirstOrDefault();

            var postDateString = postDateNode == null ? string.Empty : postDateNode.InnerText;

            post.PostDate = postDateNode == null ? default(DateTime) :
                Convert.ToDateTime(postDateString.SanitizeDateTimeHTML());
        }

        private void ParseHasSeen(SAPost post, HtmlNode postNode)
        {
            var hasSeenMarker = postNode.Descendants("tr")
                .Where(node => node.GetAttributeValue("class", "").Contains(HAS_SEEN_FLAG))
                .FirstOrDefault();

            var hasNotSeenMarker = postNode.Descendants("img")
            .Where(node => node.GetAttributeValue("src", "")
                .Equals(HAS_NOT_SEEN_URL)).FirstOrDefault();

            bool firstGuess = hasSeenMarker != null;
            bool secondGuess = hasNotSeenMarker == null;

            post.HasSeen = firstGuess || secondGuess;
        }

        private void ParseAuthor(SAPost post, HtmlNode postNode)
        {
            var authorNode = postNode.Descendants()
              .Where(node =>
                  (node.GetAttributeValue("class", "").Equals("author")) ||
                  (node.GetAttributeValue("title", "").Equals("Administrator")) ||
                  (node.GetAttributeValue("title", "").Equals("Moderator")))
              .FirstOrDefault();

            if (authorNode != null)
            {
                var type = authorNode.GetAttributeValue("title", "");
                switch (type)
                {
                    case "Administrator":
                        post.AccountType = Models.AccountType.Admin;
                        break;

                    case "Moderator":
                        post.AccountType = Models.AccountType.Moderator;
                        break;

                    default:
                        post.AccountType = Models.AccountType.Normal;
                        break;
                }

                post.PostAuthor = authorNode.InnerText;
            }

            else
            {
                post.PostAuthor = "SAPoster";
                post.AccountType = Models.AccountType.Normal;
            }

            post.PostAuthor = ContentFilter.Censor(post.PostAuthor);
        }

        private void ParseSeenUrl(SAPost post, HtmlNode postNode)
        {
            var seenUrlNode = postNode.Descendants("a")
                .Where(node => node.GetAttributeValue("title", "").Equals("Mark thread seen up to this post"))
                .FirstOrDefault();

            if (seenUrlNode == null)
            {
                post.MarkSeenUrl = String.Empty;
            }

            else
            {
                // make sure the string is in the right format so the uri class can parse correctly.
                var nodeValue = seenUrlNode.GetAttributeValue("href", "");
                post.MarkSeenUrl = string.Format("http://forums.somethingawful.com{0}", HttpUtility.HtmlDecode(nodeValue));
            }
        }

        private int ParsePostID(HtmlNode postNode)
        {
            var id = postNode.DescendantsAndSelf()
              .Where(node => node.GetAttributeValue("class", "").Equals("post"))
              .FirstOrDefault();

            int result = -1;

            if (id != null)
            {
                string postID = id.GetAttributeValue("id", "").Replace("post", "");
                int.TryParse(postID, out result);
            }

            return result;
        }
    }
}
