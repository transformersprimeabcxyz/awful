using System;
using System.Linq;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using Microsoft.Phone.Data.Linq;
using Microsoft.Phone.Data.Linq.Mapping;
using Awful.Models;
using KollaSoft;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;

namespace Awful.Data
{
    public class SAForumDB : DataContext
    {
        public static string DBConnectionString = "Data Source=isostore:/SADB.sdf";

        public static EventHandler Disposed;

        #region Default Subforum Mappings 

        private static readonly Subforum[] subforums = new Subforum[]
        {
            new Subforum() { Name = "Other", ForumIDs = new List<int>()
            {

            }},
            new Subforum() { Name = "Main", ForumIDs = new List<int>()
            {
                1, 26, 155, 214, 154
            }},
            new Subforum() { Name = "Discussion", ForumIDs = new List<int>()
            {
                192, 190, 158, 200, 46, 162, 22, 170, 202, 219, 167, 91, 236,
                124, 132, 218, 211
            }},
            new Subforum() { Name = "Games", ForumIDs = new List<int>()
            {
                44, 145, 93, 234, 191, 256, 146, 250, 103,
            }},
            new Subforum() { Name = "Sports & Auto", ForumIDs = new List<int>()
            {
                122, 181, 175, 177, 139, 91, 248, 
            }},
            new Subforum() { Name = "Fitness & Living" , ForumIDs = new List<int>()
            {
                179, 183, 244, 161
            }},
            new Subforum() { Name = "The Finer Arts",  ForumIDs = new List<int>()
            {
                31, 210, 247, 151, 133, 182, 150, 104, 130, 144, 27, 215, 255
            }},
            new Subforum() { Name = "The Community",ForumIDs = new List<int>()
            {
                61, 77, 85, 43, 241, 188, 251, 186, 201
            }},
            new Subforum() { Name = "Archives",ForumIDs = new List<int>()
            {
                21, 115, 222, 229, 25, 204
            }},
        };

        #endregion

        // Feel free to add whatever SA account you want here for the !!DEFAULTUSER!! login to work.
        private static readonly Profile _defaultProfile = new Profile() { Username = "", Password = "" };

        public static IEnumerable<Subforum> DefaultSubforums { get { return subforums; } }

        // Pass the connection string to the base class.
        public SAForumDB(string connectionString): base(connectionString) { }

        public SAForumDB() : this(DBConnectionString) { }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Disposed.Fire(null);
        }
        
        public Table<Profile> Profiles;

        // Specify a single table for the to-do items.
        public Table<Subforum> Subforums;

        // Specify a single table for forums
        public Table<SAForum> Forums;

        public Table<ForumFavorite> ForumFavorites;

        public Table<SAThread> Threads;

        public Table<SAThreadPage> ThreadPages;

        public Table<SAPost> Posts;

        public Table<AwfulSmiley> Smilies;

        public Table<SAAuthToken> Tokens;

        // Default profile will always be the first profile in the table
        public static Profile DefaultProfile { get { return _defaultProfile; } }

        public static void SetDefaultMapping(SAForum forum)
        {
            int id = forum.ID;
            foreach (var subforum in DefaultSubforums)
            {
                if (subforum.ForumIDs.Contains(id))
                {
                    subforum.Forums.Add(forum);
                    return;
                }
            }

            if (forum.Subforum == null) { forum.Subforum = subforums[0]; }
        }
    }
}