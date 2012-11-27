using System;
using System.Linq;
using System.Data.Linq;
using System.Collections.Generic;
using System.Net;
using Awful.Models;
using HtmlAgilityPack;
using System.IO;
using System.Runtime.Serialization;
using System.IO.IsolatedStorage;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media.Animation;
using Telerik.Windows.Controls;
using Microsoft.Phone.Shell;
using Awful.Helpers;
using Awful.Data;
using System.Windows.Threading;

namespace Awful
{
    public static class DatabaseExtensions
    {
        public static IEnumerable<SAThreadPage> Pages(this SAThread thread)
        {
            IQueryable<SAThreadPage> query = null;
            IEnumerable<SAThreadPage> result = null;
            using (var db = new SAForumDB())
            {
                query = from page in db.ThreadPages
                        where page.ThreadID == thread.ID
                        select page;

                result = query.ToArray();
            }

            return result;
        }
    }

    // extensions custom to awful will go here
    public static class AwfulExtentions
    {
        private static string BuildPreviewHtml(this SAThreadPreviewPage page, HtmlNode node)
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
                page.Posts = new List<PostData>(1);
                page.Posts.Add(post);
            }

            string content = PostWebViewContentItemBuilder.MergePostsToHtml(page.Posts);
            return content;
        }

        public static bool IsEditable(this SAPost post)
        {
            var username = App.CurrentUser;
            return username.Equals(post.PostAuthor);
        }

        public static void IsOpenThenInvoke(this RadWindow window, bool isOpen, Action invoke)
        {
            // don't do anything if it's already open
            if (window.IsOpen == isOpen) return;

            EventHandler handler = null;
            if (isOpen)
            {
                handler = (obj, args) => { window.WindowOpened -= new EventHandler<EventArgs>(handler); invoke(); };
                window.WindowOpened += new EventHandler<EventArgs>(handler);
                window.IsOpen = true;
            }
            else
            {
                handler = (obj, args) => { window.WindowClosed -= new EventHandler<WindowClosedEventArgs>(handler); invoke(); };
                window.WindowClosed += new EventHandler<WindowClosedEventArgs>(handler);
                window.IsOpen = false;
            }
        }
    }
}

