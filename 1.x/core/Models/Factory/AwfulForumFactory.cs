using System;
using System.Linq;
using Awful.Core.Models;
using HtmlAgilityPack;

namespace Awful.Core.Helpers
{
    public static class AwfulForumFactory
    {
        public static AwfulForum Build(HtmlNode node)
        {
            var url = node.Attributes["href"].Value;
            var tokens = url.Split('=');

            AwfulForum forum = new AwfulForum();

            forum.ID = Int32.Parse(tokens.Last());
            forum.ForumName = node.InnerText.Trim();

            return forum;
        }
    }
}
