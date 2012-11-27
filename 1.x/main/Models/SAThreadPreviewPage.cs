using System;
using System.Linq;
using HtmlAgilityPack;
using Awful.Helpers;
using System.Text;
using System.Windows.Media;
using System.Collections.Generic;

namespace Awful.Models
{
    public class SAThreadPreviewPage : SAThreadPage
    {
        public SAThreadPreviewPage(string html)
            : base()
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            this.Thread = new SAThread() { ThreadTitle = "Thread Preview", MaxPages = 1, CurrentPage = 1, };
            this.ThreadTitle = this.Thread.ThreadTitle;
            this.PageNumber = 1;
            this.MaxPages = 1;

            this._Html = BuildPreviewHtml(doc.DocumentNode);
        }

        private string BuildPreviewHtml(HtmlNode node)
        {
            SAPost post = new SAPost();
            post.PostAuthor = "Post Preview";
            post.PostDate = DateTime.Now;
            post.ShowPostIcon = false;
            post.PostIndex = 1;

            var previewContentNode = node.Descendants("div")
                .Where(n => n.GetAttributeValue("class", "").Contains("postbody"))
                .FirstOrDefault();

            if (previewContentNode != null)
            {
                post.ContentNode = previewContentNode;
                this.Posts = new List<PostData>(1);
                this.Posts.Add(post);
            }

            string content = PostWebViewContentItemBuilder.MergePostsToHtml(this.Posts);
            return content;
        }
    }
}
