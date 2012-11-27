using System;
using KollaSoft;
using System.Linq;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using Microsoft.Phone.Data.Linq;
using Microsoft.Phone.Data.Linq.Mapping;
using System.Collections.Generic;

namespace Awful.Core.Models
{
    [Table]
    public class AwfulSubforum : PropertyChangedBase, SubforumData<AwfulForum>
    {
        public const string AWFUL_SUBFORUM_PRIMARY_KEY = "ID";

        private enum DefaultSubforums
        {
            OTHER=0,
            MAIN,
            DISCUSSION,
            GAMES,
            SPORTS,
            FITNESS,
            ARTS,
            COMMUNITY,
            ARCHIVES
        };

        private readonly static Dictionary<DefaultSubforums, string> ForumNames = new Dictionary<DefaultSubforums,string>();
        private readonly static Dictionary<DefaultSubforums, List<int>> ForumsInSubforum = new Dictionary<DefaultSubforums, List<int>>();

        static AwfulSubforum()
        {
            ForumNames.Add(DefaultSubforums.OTHER, "Other");
            ForumNames.Add(DefaultSubforums.MAIN, "Main");
            ForumNames.Add(DefaultSubforums.DISCUSSION, "Discussion");
            ForumNames.Add(DefaultSubforums.GAMES, "Games");
            ForumNames.Add(DefaultSubforums.SPORTS, "Sports & Auto");
            ForumNames.Add(DefaultSubforums.FITNESS, "Fitness & Living");
            ForumNames.Add(DefaultSubforums.ARTS, "The Finer Arts");
            ForumNames.Add(DefaultSubforums.COMMUNITY, "The Community");
            ForumNames.Add(DefaultSubforums.ARCHIVES, "Archives");

            ForumsInSubforum.Add(DefaultSubforums.OTHER, new List<int>() { AwfulControlPanel.AWFUL_CONTROL_PANEL_ID });
            ForumsInSubforum.Add(DefaultSubforums.MAIN, new List<int>(){ 1, 26, 155, 214, 154 });
            ForumsInSubforum.Add(DefaultSubforums.DISCUSSION, new List<int>(){ 192, 190, 158, 200, 46, 162, 22, 170, 202, 219, 167, 91, 236, 124, 132, 218, 211 });
            ForumsInSubforum.Add(DefaultSubforums.GAMES, new List<int>(){ 44, 145, 93, 234, 191, 256, 146, 250, 103 });
            ForumsInSubforum.Add(DefaultSubforums.SPORTS, new List<int>(){122, 181, 175, 177, 139, 91, 248 });
            ForumsInSubforum.Add(DefaultSubforums.FITNESS, new List<int>(){179, 183, 244, 161});
            ForumsInSubforum.Add(DefaultSubforums.ARTS, new List<int>(){ 31, 210, 247, 151, 133, 182, 150, 104, 130, 144, 27, 215, 255 });
            ForumsInSubforum.Add(DefaultSubforums.COMMUNITY, new List<int>(){61, 77, 85, 43, 241, 188, 251, 186, 201});
            ForumsInSubforum.Add(DefaultSubforums.ARCHIVES, new List<int>(){21, 115, 222, 229, 25, 204});
        }

        public static AwfulSubforum SetDefaultSubforum(AwfulForum forum)
        {
            foreach (var key in ForumsInSubforum.Keys)
            {
                var list = ForumsInSubforum[key];
                if (list.Contains(forum.ID))
                {
                    return new AwfulSubforum()
                    {
                        ID = (int)key,
                        Name = ForumNames[key]
                    };
                }
            }

            return new AwfulSubforum() { ID = (int)DefaultSubforums.OTHER, Name = ForumNames[DefaultSubforums.OTHER] };
        }

        public static IEnumerable<AwfulSubforum> GenerateDefaultSubforums()
        {
            List<AwfulSubforum> subforums = new List<AwfulSubforum>();
            var keys = ForumNames.Keys;
            foreach (var key in keys)
            {
                string name = ForumNames[key];
                subforums.Add(new AwfulSubforum() { ID = (int)key, Name = name });
            }
            return subforums;
        }

        private EntitySet<AwfulForum> _forums;
        [Association(
            Storage = "_forums",
            ThisKey = AWFUL_SUBFORUM_PRIMARY_KEY,
            OtherKey = AwfulForum.AWFUL_SUBFORUM_FOREIGN_KEY)]
        private EntitySet<AwfulForum> _Forums
        {
            get { return this._forums; }
            set { this._forums.Assign(value); }
        }

        [Column(IsVersion = true)]
        private Binary _version;

        public IList<AwfulForum> Forums
        {
            get { return this._Forums; }
        }

        [Column(IsPrimaryKey = true)]
        public int ID { get; set; }

        [Column(CanBeNull = false)]
        public string Name { get; set; }

        private List<int> _forumIDs;
        public List<int> ForumIDs 
        {
            get { return this._forumIDs; }
            set { this._forumIDs = value; }
        }

        public AwfulSubforum() { this._forums = new EntitySet<AwfulForum>(this.OnForumAdd, this.OnForumRemove); }

        private void OnForumAdd(AwfulForum forum) { forum.Subforum = this; }
        private void OnForumRemove(AwfulForum forum) { forum.Subforum = null; }
    }
}
