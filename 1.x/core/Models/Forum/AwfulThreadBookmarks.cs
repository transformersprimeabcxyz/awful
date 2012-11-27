using System;
using System.Data.Linq.Mapping;
using System.Data.Linq;

namespace Awful.Core.Models
{
    [Table]
    public class AwfulThreadBookmark
    {
        public const string AWFUL_THREAD_BOOKMARK_PROFILE_FOREIGN_KEY = "ProfileID";
        public const string AWFUL_THREAD_BOOKMARK_THREAD_FOREIGN_KEY = "ThreadID";

        [Column(IsVersion = true)]
        private Binary _version;

        [Column(IsPrimaryKey = true)]
        private int ProfileID { get; set; }

        [Column(IsPrimaryKey = true)]
        private int ThreadID { get; set; }
        
        private EntityRef<AwfulProfile> _profile;
        [Association(
            IsForeignKey = true,
            DeleteOnNull = true,
            ThisKey = AWFUL_THREAD_BOOKMARK_PROFILE_FOREIGN_KEY, 
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

        private EntityRef<AwfulThread> _thread;
        [Association(
            IsForeignKey = true,
            DeleteOnNull = true,
            ThisKey = AWFUL_THREAD_BOOKMARK_THREAD_FOREIGN_KEY, 
            OtherKey = AwfulThread.AWFUL_THREAD_PRIMARY_KEY,
            Storage = "_thread")]
        public AwfulThread Thread
        {
            get { return this._thread.Entity; }
            set 
            {
                AwfulThread previous = this._thread.Entity;
                if ((previous != value) || (!this._thread.HasLoadedOrAssignedValue))
                {
                    if (previous != null)
                    {
                        this._thread.Entity = null;
                    }
                    this._thread.Entity = value;
                    if (value != null)
                    {
                        this.ThreadID = value.ID;
                    }
                    else { this.ThreadID = -1; }
                }
            }
        }

        public AwfulThreadBookmark()
        {
            this._profile = new EntityRef<AwfulProfile>();
            this._thread = new EntityRef<AwfulThread>();
        }

        public AwfulThreadBookmark(int profileID, int threadID)
            : base()
        {
            this.ProfileID = profileID;
            this.ThreadID = threadID;
        }
    }
}
