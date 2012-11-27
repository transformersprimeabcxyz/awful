using System.Data.Linq;
using Awful.Core.Models;
using System;

namespace Awful.Core.Database
{
    public abstract class AbstractDataContext : DataContext
    {
        protected Table<AwfulForum> Forums;
        protected Table<AwfulSubforum> Subforums;
        protected Table<AwfulThread> Threads;
        protected Table<AwfulThreadPage> ThreadPages;
        protected Table<AwfulPost> Posts;
        protected Table<AwfulProfile> Profiles;
        protected Table<AwfulAuthenticationToken> Tokens;
        protected Table<AwfulForumFavorite> ForumFavorites;
        protected Table<AwfulSmiley> Smilies;

        private const string AWFUL_COREDB_CONNECTION = "Data Source=isostore:/coredb.sdf";
        private const string DB_CONNECTION_PREFIX = "Data Source=isostore:";

        public AbstractDataContext(string connectionString) : base(connectionString) { }
        public AbstractDataContext() : this(AWFUL_COREDB_CONNECTION) { }
        
        public static T CreateDataContext<T>(string name, Func<string, T> predicate) where T : AbstractDataContext
        {
            string connectionString = string.Format("{0}/{1}.sdf", DB_CONNECTION_PREFIX, name);
            T context = predicate(connectionString);
            return context;
        }
    }
}
