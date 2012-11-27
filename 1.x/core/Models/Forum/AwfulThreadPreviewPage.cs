using System;
using System.Linq;
using HtmlAgilityPack;
using System.Collections.Generic;
using Awful.Core.Models.Factory;

namespace Awful.Core.Models
{
    public class AwfulThreadPreviewPage : AwfulThreadPage
    {
        public AwfulThreadPreviewPage(string html)
            : base()
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            this._Parent = new AwfulThread() 
            { 
                Title = "Thread Preview", 
                TotalPages = 1,  
            };

            this.PageNumber = 1;

            this.Html = BuildPreviewHtml(doc.DocumentNode);
        }

        private string BuildPreviewHtml(HtmlNode node)
        {
            AwfulPost post = new AwfulPost();
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
                this.Posts.Add(post);
            }

            string content = AwfulThreadPageHtmlFactory.Metrofy(this);
            return content;
        }
    }
}
