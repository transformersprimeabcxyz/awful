using System;
using System.Data.Linq;
using Awful.Core.Models;
using System.Collections.Generic;
using Microsoft.Phone.Data.Linq;
using Awful.Core.Event;

namespace Awful.Core.Database
{
    public class AwfulDataContext : DataContext
    {
        public Table<AwfulForum> Forums;
        public Table<AwfulSubforum> Subforums;
        public Table<AwfulThread> Threads;
        public Table<AwfulThreadPage> ThreadPages;
        public Table<AwfulPost> Posts;
        public Table<AwfulProfile> Profiles;
        public Table<AwfulAuthenticationToken> Tokens;
        public Table<AwfulForumFavorite> ForumFavorites;
        public Table<AwfulThreadBookmark> ThreadBookmarks;
        public Table<AwfulSmiley> Smilies;

        private const string AWFUL_COREDB_CONNECTION = "Data Source=isostore:/coredb.sdf";
        private const string DB_CONNECTION_PREFIX = "Data Source=isostore:";

        public AwfulDataContext(string connectionString) : base(connectionString) { }
        public AwfulDataContext() : this(AWFUL_COREDB_CONNECTION) { }
        public static AwfulDataContext CreateDataContext(string name)
        {
            string connectionString = string.Format("{0}/{1}.sdf", DB_CONNECTION_PREFIX, name);
            AwfulDataContext context = null;
            context = new AwfulDataContext(connectionString);
            return context;
        }

        public void ManageDatabase()
        {
            if (!this.DatabaseExists()) { this.CreateDatabase(); }

            // manage versions
            var schemaUpdater = this.CreateDatabaseSchemaUpdater();
            int version = schemaUpdater.DatabaseSchemaVersion;
            try
            {
                switch (version)
                {
                    case 0:
                        this.UpdateToVersionOne(schemaUpdater);
                        break;
                }
            }
            catch (Exception ex) { Logger.AddEntry("An error occurred while updating schema:", ex); }
        }

        private void UpdateToVersionOne(DatabaseSchemaUpdater updater)
        {
            updater.AddTable<AwfulThreadBookmark>();
            updater.AddAssociation<AwfulProfile>("ThreadBookmarks");
            updater.AddColumn<AwfulProfile>("LastBookmarkRefresh");
            updater.AddAssociation<AwfulThread>("ThreadBookmarks");
            updater.DatabaseSchemaVersion = 1;
            updater.Execute();
        }
    }
}
