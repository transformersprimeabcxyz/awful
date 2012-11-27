using System;


namespace Awful.Core.Models
{
    public class AwfulUserFilteredThreadPage : AwfulThreadPage
    {
        private int _userId;

        public int UserID { get { return this._userId; } }

        public AwfulUserFilteredThreadPage(int userId, AwfulThread parent, int pageNumber)
            : base(parent, pageNumber)
        {
            this._userId = userId;
        }

        public AwfulUserFilteredThreadPage(int userId, AwfulThread parent) : this(userId, parent, 0) { }

        protected override string GenerateUrl(int pageNumber)
        {
            if (pageNumber != 0)
            {
                return String.Format("http://forums.somethingawful.com/showthread.php?threadid={0}&userid={1}&perpage=40&pagenumber={2}",
                    this.ThreadID,
                    this.UserID,
                    pageNumber);
            }

            return String.Format("http://forums.somethingawful.com/showthread.php?threadid={0}&userid={1}", this.ThreadID, this.UserID);
        }
    }
}
