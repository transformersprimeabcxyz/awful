using System;
using System.Linq;
using HtmlAgilityPack;
using Awful.Core.Models.Messaging.Interfaces;
using Awful.Core.Models.Messaging;
using System.Collections.Generic;
using Awful.Core.Models;

namespace Awful
{
    public static class AwfulExtensions
    {
        internal static string ParseTitleFromBreadcrumbsNode(this HtmlNode node)
        {
            string value = string.Empty;
            try
            {
                var breadcrumbs = node.InnerText.Replace("&gt;", "|").Split('|');
                value = breadcrumbs.Last().Trim();
            }
            catch (Exception) { value = "Unknown Value"; }
            return value;
        }

        public static bool IsReadOnly(this IPrivateMessageFolder folder)
        {
            int id = folder.FolderID;
            return AwfulPrivateMessageFolder.READONLY_FOLDER_LIST.Contains(id);
        }

        public static void AddToBookmarks(this AwfulProfile profile, AwfulThread thread)
        {
            profile.ThreadBookmarks.Add(new AwfulThreadBookmark() { Profile = profile, Thread = thread });
        }
    }
}
