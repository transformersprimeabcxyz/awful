using System;
using System.Text;
using System.Collections.Generic;
using HtmlAgilityPack;
using Awful.Core.Models;
using System.Windows.Media;
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using System.Threading;
using System.Windows;
using Awful.Core.Web;
using Awful.Core.Models.Interfaces;

namespace Awful.Core.Models.Factory
{
    public class AwfulThreadPageHtmlFactory
    {
        public static PageOrientation CurrentOrientation;

        public static string Metrofy(AwfulThreadPage page)
        {
            return Metrofy(page.Posts);
        }

        public static string Metrofy(ICollection<AwfulPost> posts)
        {
            StringBuilder html = new StringBuilder();
            html.Append("<table>");

            int postCount = posts.Count;

            foreach (var item in posts)
            {
                try
                {
                    var post = item as AwfulPost;

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

        private static string AppendPostAuthor(AwfulPost post)
        {
            string style = string.Empty;
            switch (post.AccountType)
            {
                case AccountType.ADMIN:
                    style = "admin_post";
                    break;

                case AccountType.MODERATOR:
                    style = "mod_post";
                    break;

                case AccountType.NORMAL:
                    style = "user_post";
                    break;
            }

            return string.Format("<span class='text_title3style'><span class='{0}'>{1}</span></span><br/>", style, post.PostAuthor);
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

