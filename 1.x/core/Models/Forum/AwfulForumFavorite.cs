using System;
using System.Data.Linq.Mapping;
using System.Data.Linq;

namespace Awful.Core.Models
{
    [Table]
    public class AwfulForumFavorite
    {
        public const string AWFUL_FORUMFAVORITE_PROFILE_FOREIGN_KEY = "ProfileID";
        public const string AWFUL_FORUMFAVORITE_FORUM_FOREIGNKEY = "ForumID";

        [Column(IsVersion = true)]
        private Binary _version;

        [Column(IsPrimaryKey = true)]
        private int ProfileID { get; set; }

        [Column(IsPrimaryKey = true)]
        private int ForumID { get; set; }
        
        private EntityRef<AwfulProfile> _profile;
        [Association(
            IsForeignKey = true,
            DeleteOnNull = true,
            ThisKey = AWFUL_FORUMFAVORITE_PROFILE_FOREIGN_KEY, 
            OtherKey = AwfulProfile.AWFUL_PROFILE_PRIMARY_KEY, 
            Storage = "_profile")]
        public AwfulProfile Profile
        {
            get { return this._profile.Entity; }
            set 
            {
                AwfulProfile previous = this._profile.Entity;
                if ((previous != value) || (!this._profile.HasLoadedOrAssignedValue))
                {
                    if (previous != null)
                    {
                        this._profile.Entity = null;
                    }
                    this._profile.Entity = value;
                    if (value != null)
                    {
                        this.ProfileID = value.ID;
                    }
                    else { this.ProfileID = -1; }
                }
            }
        }

        private EntityRef<AwfulForum> _forum;
        [Association(
            IsForeignKey = true,
            DeleteOnNull = true,
            ThisKey = AWFUL_FORUMFAVORITE_FORUM_FOREIGNKEY, 
            OtherKey = AwfulForum.AWFUL_FORUM_PRIMARY_KEY,
            Storage = "_forum")]
        public AwfulForum Forum
        {
            get { return this._forum.Entity; }
            set 
            {
                AwfulForum previous = this._forum.Entity;
                if ((previous != value) || (!this._forum.HasLoadedOrAssignedValue))
                {
                    if (previous != null)
                    {
                        this._forum.Entity = null;
                    }
                    this._forum.Entity = value;
                    if (value != null)
                    {
                        this.ForumID = value.ID;
                    }
                    else { this.ForumID = -1; }
                }
            }
        }

        public AwfulForumFavorite()
        {
            this._profile = new EntityRef<AwfulProfile>();
            this._forum = new EntityRef<AwfulForum>();
        }
    }
}
