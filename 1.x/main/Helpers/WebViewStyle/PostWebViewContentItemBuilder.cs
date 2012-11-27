using System;
using System.Text;
using System.Collections.Generic;
using HtmlAgilityPack;
using Awful.Models;
using System.Windows.Media;
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using Telerik.Windows.Controls;
using System.Threading;
using System.Windows;

namespace Awful.Helpers
{
    public class PostWebViewContentItemBuilder
    {
        private Color foreground;
        private Color background;

        public static PageOrientation CurrentOrientation;

        public static string MergePostsToHtml(IEnumerable<PostData> posts)
        {
            StringBuilder html = new StringBuilder();
           
            Color bg = Globals.Resources.Background;

            html.Append("<!DOCTYPE html>");
            html.Append("<html>");
            
            html.Append("<head><meta charset='utf-8' /><meta content='user-scalable=no' name='viewport' /><title>SomethingAwfulPost</title>" + 
            "<script src='jquery.min.js'></script><script src='awful.js'></script><link rel='stylesheet' type='text/css' href='awful.css' /></head>");

            html.AppendFormat("<body style='background:#{0}'>", bg.ToString().Substring(3));
            html.Append("<table>");

            int postCount = posts.Count();

            foreach (var item in posts)
            {
                try
                {
                    var post = item as SAPost;

                    html.Append("<tr><td>");
                    html.AppendFormat("<a href='#post_{0}' id='postlink{0}' style='display:none'>This should be hidden.</a>", post.PostIndex);
                    html.AppendFormat("<div class='post_header' id='post_{0}' onclick=\"javascript:openPostMenu('{0}')\">", post.PostIndex);

                    if (post.ShowPostIcon)
                    {
                        Uri iconUri = new Uri(post.PostIconUri, UriKind.RelativeOrAbsolute);
                        html.AppendFormat("<img class='post_icon' alt='' src='{0}' />", iconUri.AbsoluteUri);
                        html.AppendFormat("<div class='post_header_text' >", post.PostIndex);
                        html.Append(AppendPostAuthor(post));
                        html.AppendFormat("<span class='text_subtlestyle'>#{0}/{1}, {2}</span>",
                            post.PostIndex,
                            postCount,
                            post.PostDate);
                    }
                    else
                    {
                        html.Append("<div class='post_header_text_noicon'>");
                        html.Append(AppendPostAuthor(post));
                        html.AppendFormat("<span class='text_subtlestyle'>#{0}/{1}, {2}</span>",
                            post.PostIndex,
                            postCount,
                            post.PostDate.ToString("MM/dd/yyyy, h:mm tt"));
                    }

                    var node = new HtmlNode(HtmlNodeType.Element, post.ContentNode.OwnerDocument, -1)
                    {
                        InnerHtml = post.ContentNode.InnerHtml,
                        Name = post.ContentNode.Name
                    };

                    var content = new MainWebParser(node.FirstChild).Body;

                    html.AppendFormat("</div></div><div class='{0}'><pre>{1}</pre></div></pre></td></tr>",
                        post.HasSeen ? "post_content_seen" : "post_content",
                        content);
                }

                catch (Exception ex)
                {
                    string error = string.Format("There was an error while merging posts. [{0}] {1}", ex.Message, ex.StackTrace);
                }
            }

            html.Append("</table>");
            html.Append("</body>");
            string result = html.ToString();
            return result;
        }

        private static string AppendPostAuthor(SAPost post)
        {
            var color = string.Empty;
            switch (post.AccountType)
            {
                case AccountType.Admin:
                    color = "style='color:red; font-size: large'";
                    break;

                case AccountType.Moderator:
                    color = "style='color:yellow; font-size: large'";
                    break;

                case AccountType.Normal:
                    color = string.Format("style='color:#{0}; font-size: large'", Globals.Resources.Foreground.ToString().Substring(3));
                    break;
            }
              return string.Format("<span class='text_title3style' {1}>{0}</span><br/>", post.PostAuthor, color);
        }

        public PostWebViewContentItemBuilder(Color foreground, Color background)
        {
            this.foreground = foreground;
            this.background = background;
        }

        public string FormatHtml(HtmlNode node)
        {
            string fg = foreground.ToString().Substring(3);
            string bg = background.ToString().Substring(3);

            StringBuilder html = new StringBuilder();
            html.Append("<!DOCTYPE html>");
            html.Append("<html>");
            
            html.Append("<head><meta charset='utf-8' /><meta content='user-scalable=no' name='viewport' /><title>SomethingAwfulPost</title>" + 
            "<script src='jquery.min.js'></script><script src='awful.js'></script><link rel='stylesheet' type='text/css' href='awful.css' /></head>");

            string body = new MainWebParser(node).Body;
          
            html.AppendFormat(@"<body style='background:#{0}; color:#{1}; font-size:{2}px'>{3}</body>", 
                bg, 
                fg,
                App.Settings.PostTextSize,
                body);
            
            html.AppendLine("</html>");

            string result = html.ToString();
            
            return result;
        }

        #region Html constants

        private const string postHeader =
          @"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" 
            ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">
            <html xmlns=""http://www.w3.org/1999/xhtml"">";

        private const string metaContent =
            @"<meta content=""en-us"" http-equiv=""Content-Language"" />
            <meta content=""text/html; charset=iso-8859-1"" http-equiv=""Content-Type"" />
            <meta name=""viewport"" content=""user-scalable=no"" />
            <title>SomethingAwfulPost</title>";

        #endregion
    }
}
