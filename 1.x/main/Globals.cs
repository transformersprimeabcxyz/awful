using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Tasks;

namespace Awful
{
    public static class Globals
    {
        public static class States
        {
            public const string IS_LOGGED_IN = "IsLoggedIn";
            public const string CURRENT_THREAD = "CurrentThread";
            public const string CURRENT_FORUM = "CurrentForum";
            public const string LAST_POST_INDEX = "LastPostIndex";
        }

        public static class Resources
        {
            public const string AddUri = "/icons/appbar.add.rest.png";
            public const string BackUri = "/icons/appbar.back.rest.png";
            public const string CheckUri = "/icons/appbar.check.rest.png";
            public const string CloseUri = "/icons/appbar.close.rest.png";
            public const string NextUri = "/icons/appbar.next.rest.png";
            public const string DownloadUri = "/icons/appbar.download.rest.png";
            public const string RefreshUri = "/icons/appbar.refresh.rest.png";
            public const string UploadUri = "/icons/appbar.upload.rest.png";
            public static Color Foreground { get; set; }
            public static Color PostForeground { get; set; }
            public static Color Background { get; set; }
            public static Color HasSeen { get; set; }
        }

        public static class Constants
        {
            public const string STATE_DIRECTORY = "states";
            public const string THREADPAGE_DIRECTORY = "www\\pages";
            public const string COOKIE_DOMAIN = "http://forums.somethingawful.com";

            public const string AWFUL_THREAD_ID = "3460814";
            public const string AUTHOR_EMAIL = "kollasoftware@gmail.com";
            public const string DEMARC = "##$$##";

            public const string WEB_PAGE_FAILURE = "Could not load the requested page. Hit refresh and try again.";

            public const string SA_BASE = "http://forums.somethingawful.com";
            public const string FORUM_BASE_URL = SA_BASE + "/forumdisplay.php?forumid=";
            public const string FORUM_PAGE_QUERY = "&daysprune=15&perpage=40&posticon=0sortorder=desc&sortfield=lastpost&pagenumber=";
            public const string THREAD_PAGE_QUERY = "&userid=0&perpage=40&pagenumber=";
            public const string THREAD_LAST_POST =  "&gotolastpost";
            public const string BOOKMARK_THREAD = "bookmarkthreads.php";
            public const string ADD_BOOKMARK = "json=1&action=cat_toggle";
            public const string REMOVE_BOOKMARK = "json=1&action=remove";
            public const string THREAD_ID = "threadid";

            public const int POSTS_PER_THREAD_PAGE = 40;
            public const int POST_SMALL_TEXT = 16;
            public const int POST_MEDIUM_TEXT = 20;
            public const int POST_LARGE_TEXT = 32;

            public const string THREAD_RATING_5 = "5stars.gif";
            public const string THREAD_RATING_4 = "4stars.gif";
            public const string THREAD_RATING_3 = "3stars.gif";
            public const string THREAD_RATING_2 = "2stars.gif";
            public const string THREAD_RATING_1 = "1star.gif";

            public const int THREAD_TIMEOUT = 10000;
        }

        public static class Events
        {
            public static void MediaTapped(object sender, GestureEventArgs args)
            {
                FrameworkElement element = sender as FrameworkElement;
                WebBrowserTask task = new WebBrowserTask();
                task.Uri = new Uri(element.Tag as string);
                try { task.Show(); }
                catch (Exception) { }
            }
        }
    }
}
