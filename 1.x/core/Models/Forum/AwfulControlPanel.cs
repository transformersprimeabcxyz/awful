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

namespace Awful.Core.Models
{
    public class AwfulControlPanel : AwfulForum
    {
        public const int AWFUL_CONTROL_PANEL_ID = -2;

        public AwfulControlPanel()
            : base()
        {
            this.ForumName = "Control Panel";
            this.ID = AWFUL_CONTROL_PANEL_ID;
            this.Subforum = AwfulSubforum.SetDefaultSubforum(this);
        }

        public override ForumPageData GetForumPage(int pageNumber)
        {
            return GetBookmarks();
        }

        public AwfulForumPage GetBookmarks() { return new AwfulBookmarkPage(this); }
    }
}
