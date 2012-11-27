using System;
using System.Linq;
using Awful.Core.Models;
using HtmlAgilityPack;
using System.Collections.Generic;

namespace Awful.Core.Models.Factory
{
    public class AwfulSmileyFactory
    {
        private const string SMILEY_NODE_PARENT_ELEMENT = "li";
        private const string SMILEY_NODE_ATTRIBUTE_VALUE = "smilie";
        private const string SMILEY_TEXT_ATTRIBUTE_VALUE = "text";
        private const string SMILEY_URI_ATTRIBUTE = "src";
        private const string SMILEY_TITLE_ATTRIBUTE = "title";

        private static readonly AwfulSmileyFactory Factory = new AwfulSmileyFactory();
        private AwfulSmiley BuildFromNode(HtmlNode node)
        {
            AwfulSmiley smiley = new AwfulSmiley();
            smiley.Text = this.GetText(node);
            this.SetTitleAndUri(node, smiley);

            return smiley;
        }

        private IList<AwfulSmiley> BuildFromDocument(HtmlDocument doc)
        {
            List<AwfulSmiley> list = new List<AwfulSmiley>();
            var nodes = doc.DocumentNode.Descendants(SMILEY_NODE_PARENT_ELEMENT)
                .Where(node => node.GetAttributeValue("class", "").Equals(SMILEY_NODE_ATTRIBUTE_VALUE));

            foreach (var node in nodes)
            {
                list.Add(this.BuildFromNode(node));
            }

            return list;
        }

        private string GetText(HtmlNode parent)
        {
            var node = parent.Descendants("div")
                .Where(n => n.GetAttributeValue("class", "").Equals(SMILEY_TEXT_ATTRIBUTE_VALUE))
                .FirstOrDefault();

            string value = string.Empty;
            if (node != null) { value = node.InnerText; }
            return value;
        }

        private void SetTitleAndUri(HtmlNode parent, AwfulSmiley smiley)
        {
            var node = parent.Descendants("img")
                .FirstOrDefault();

            if (node != null)
            {
                string uri = node.GetAttributeValue(SMILEY_URI_ATTRIBUTE, "");
                string title = node.GetAttributeValue(SMILEY_TITLE_ATTRIBUTE, "");
                smiley.Uri = uri;
                smiley.Title = title;
            }
        }

        public static AwfulSmiley Build(HtmlNode node) { return Factory.BuildFromNode(node); }

        public static IList<AwfulSmiley> Build(HtmlDocument doc) { return Factory.BuildFromDocument(doc); }
    }
}
