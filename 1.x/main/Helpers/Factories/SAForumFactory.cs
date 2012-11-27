using System;
using System.Linq;
using Awful.Models;
using HtmlAgilityPack;

namespace Awful.Helpers
{
    public static class SAForumFactory
    {
        public static SAForum Build(HtmlNode node)
        {
            var url = node.Attributes["href"].Value;
            var tokens = url.Split('=');

            SAForum forum = new SAForum();

            forum.ID = Int32.Parse(tokens.Last());
            forum.ForumName = node.InnerText.Trim();
            forum.ForumName = ContentFilter.Censor(forum.ForumName);
            return forum;
        }
    }
}
