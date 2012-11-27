using System;
using KollaSoft;
using System.Linq;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using Microsoft.Phone.Data.Linq;
using Microsoft.Phone.Data.Linq.Mapping;
using System.Collections.Generic;

namespace Awful.Data
{
    [Table(Name = "Subforums")]
    public class Subforum : PropertyChangedBase
    {
        private EntitySet<Models.SAForum> _Forums;

        [Column(IsVersion = true)]
        private Binary _version;

        [Association(Storage = "_Forums", ThisKey = "ID", OtherKey = "SubforumID")]
        public EntitySet<Models.SAForum> Forums
        {
            get 
            {
                return this._Forums;
            }
        }

        [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL Identity", 
            CanBeNull = false, AutoSync = AutoSync.OnInsert)]
        public int ID { get; set; }

        [Column(CanBeNull = false)]
        public string Name { get; set; }

        public List<int> ForumIDs { get; set; }

        public Subforum() { this._Forums = new EntitySet<Models.SAForum>(this.OnForumAdd, this.OnForumRemove); }

        private void OnForumAdd(Models.SAForum forum) { forum.Subforum = this; }
        private void OnForumRemove(Models.SAForum forum) { forum.Subforum = null; }
    }
}
