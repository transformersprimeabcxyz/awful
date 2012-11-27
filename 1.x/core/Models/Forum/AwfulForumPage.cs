using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using HtmlAgilityPack;
using System.Diagnostics;
using KollaSoft;
using Awful.Core.Event;
using Awful.Core.Models.Interfaces;
using Awful.Core.Models.Factory;

namespace Awful.Core.Models
{
    public class AwfulForumPage : PropertyChangedBase, ForumPageData, WebBrowseable
    {
        private int _forumID;
        private int _id;
        private int _pageNumber;
        private ForumData _parent;
        private IList<AwfulThread> _threads;
        private string _url;
        private string m_html;
        private DateTime? _LastUpdated;

        public static event EventHandler<CollectionUpdatedEventArgs<AwfulThread>> PageUpdated;

        public AwfulForumPage(ForumData forum, int pageNumber) : this(forum, pageNumber, null) { }

        protected AwfulForumPage(ForumData forum, int pageNumber, string url)
        {
            _pageNumber = pageNumber;
            this._forumID = default(int);

            if (forum != null)
            {
                this._forumID = forum.ID;
                this._parent = forum;
            }

            if (url == null) { this.Url = this.CreateUrl(forum, pageNumber); }
            else { this.Url = url; }
        }

        public int ForumID
        {
            get { return this._forumID; }
            set { this._forumID = value; }
        }

        public int PageNumber
        {
            get { return this._pageNumber; }
            set { this._pageNumber = value; }
        }

        public IList<AwfulThread> Threads
        {
            get { return this._threads; }
            set 
            { 
                this._threads = value;
            }
        }

        public string Url
        {
            get { return this._url; }
            set { this._url = value; }
        }

        public string Html
        {
            get { return this.m_html; }
            set { this.m_html = value; }
        }
        public ForumData Parent { get { return _parent; } }

        public DateTime? LastUpdated
        {
            get
            {
                if (this._LastUpdated.HasValue)
                {
                    DateTime value = this._LastUpdated.Value.ToLocalTime();
                    return value;
                }

                return null;
            }
            set
            {
                if (value.HasValue)
                {
                    DateTime toUTC = value.Value.ToUniversalTime();
                    this._LastUpdated = toUTC;
                    NotifyPropertyChangedAsync("LastUpdated");
                }
                else
                {
                    this._LastUpdated = null;
                    NotifyPropertyChangedAsync("LastUpdated");
                }

                PageUpdated.Fire(this, new CollectionUpdatedEventArgs<AwfulThread>(this.Threads));
            }
        }

        private string CreateUrl(ForumData forum, int pageNumber)
        {
            // example:
            // http://forums.somethingawful.com/forumdisplay.php?forumid={0}&daysprune=15&perpage=40&posticon=0sortorder=desc&sortfield=lastpost&pagenumber={1} //

            var url = string.Format("{0}/{1}?forumid={2}&{3}&pagenumber={4}",
                Constants.BASE_URL,
                Constants.FORUM_PAGE_URI,
                forum.ID,
                Constants.FORUM_PAGE_QUERY,
                pageNumber);

            Logger.AddEntry(string.Format("AwfulForumPage - Forum url: '{0}'", url));

            return url;
        }

        public void RefreshAsync(Action<AwfulForumPage> result)
        {
            AwfulForumPageFactory.BuildAsync(this, build =>
                {
                    PageUpdated(this, new CollectionUpdatedEventArgs<AwfulThread>(build.Threads));
                    result(build);
                });
        }

        IList<ThreadData> ForumPageData.Threads
        {
            get 
            {
                var threads = new List<ThreadData>();
                foreach (var thread in this.Threads) { threads.Add(thread); }
                return threads;
            }
        }
    }

    internal class AwfulBookmarkPage : AwfulForumPage
    {
        public AwfulBookmarkPage(AwfulControlPanel cp) : base(cp, 1, Constants.USERCP_URI) { }
    }
}
