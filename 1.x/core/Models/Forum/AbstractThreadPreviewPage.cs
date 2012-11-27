using System;
using System.Linq;
using HtmlAgilityPack;
using System.Text;
using System.Windows.Media;
using System.Collections.Generic;

namespace Awful.Core.Models
{
    public abstract class AbstractThreadPreviewPage : AwfulThreadPage
    {
        public AbstractThreadPreviewPage(string html)
            : base()
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            var thread = new AwfulThread() { Title = "Thread Preview", TotalPages = 1 };
            this._Parent = thread;
            this.PageNumber = 1;
            this.Html = BuildPreviewHtml(doc.DocumentNode);
        }

        protected abstract string BuildPreviewHtml(HtmlNode node);
    }
}
