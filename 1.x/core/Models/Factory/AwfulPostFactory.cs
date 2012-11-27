using System;
using System.Linq;
using Awful.Core.Models;
using HtmlAgilityPack;
using System.Data.Linq;
using System.Net;
using KollaSoft;
using Awful.Core.Event;

namespace Awful.Core.Models.Factory
{
    public class AwfulPostFactory
    {
        private AwfulPostFactory() { }
        private static readonly AwfulPostFactory Factory = new AwfulPostFactory();

        public static AwfulPost Build(HtmlNode node, AwfulThreadPage page)
        {
            AwfulPost result = null;
            Logger.AddEntry("AwfulPost - Parsing postNode for html...");

            int id = Factory.ParsePostID(node);
            try
            {
                result = new AwfulPost();
                result.ID = id;
                result.ThreadPageID = page.ID;
                Factory.ParsePostThreadIndex(result, node);
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

                Logger.AddEntry(error);
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

        private void ParseIcon(AwfulPost post, HtmlNode postNode)
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

        private void ParseUserID(AwfulPost post, HtmlNode postNode)
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

        private void ParsePostDate(AwfulPost post, HtmlNode postNode)
        {
            var postDateNode = postNode.Descendants()
              .Where(node => node.GetAttributeValue("class", "").Equals("postdate"))
              .FirstOrDefault();

            var postDateString = postDateNode == null ? string.Empty : postDateNode.InnerText;

            post.PostDate = postDateNode == null ? default(DateTime) :
                Convert.ToDateTime(postDateString.SanitizeDateTimeHTML());
        }

        private void ParseHasSeen(AwfulPost post, HtmlNode postNode)
        {
            var hasSeenMarker = postNode.Descendants("tr")
                .Where(node => node.GetAttributeValue("class", "").Contains(Constants.LASTREAD_FLAG))
                .FirstOrDefault();

            var hasNotSeenMarker = postNode.Descendants("img")
            .Where(node => node.GetAttributeValue("src", "")
                .Equals(Constants.NEWPOST_GIF_URL)).FirstOrDefault();

            bool firstGuess = hasSeenMarker != null;
            bool secondGuess = hasNotSeenMarker == null;

            post.HasSeen = firstGuess || secondGuess;
        }

        private void ParseAuthor(AwfulPost post, HtmlNode postNode)
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
                        post.AccountType = Models.AccountType.ADMIN;
                        break;

                    case "Moderator":
                        post.AccountType = Models.AccountType.MODERATOR;
                        break;

                    default:
                        post.AccountType = Models.AccountType.NORMAL;
                        break;
                }

                post.PostAuthor = authorNode.InnerText;
            }

            else
            {
                post.PostAuthor = "AwfulPoster";
                post.AccountType = Models.AccountType.NORMAL;
            }
        }

        private void ParsePostThreadIndex(AwfulPost post, HtmlNode postNode)
        {
            var seenUrlNode = postNode.Descendants("a")
                .Where(node => node.GetAttributeValue("title", "").Contains("Mark thread"))
                .FirstOrDefault();

            if (seenUrlNode == null)
            {
                post.PostIndex = AwfulPost.UNKNOWN_POST_INDEX;
            }

            else
            {
                // make sure the string is in the right format so the uri class can parse correctly.
                var nodeValue = seenUrlNode.GetAttributeValue("href", "");
                int index = -1;
                string indexValue = nodeValue.Split('&').LastOrDefault();
                if (indexValue != null)
                {
                    indexValue = indexValue.Split('=').Last();
                    post.PostIndex = int.TryParse(indexValue, out index)
                        ? index
                        : AwfulPost.UNKNOWN_POST_INDEX;
                }
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
