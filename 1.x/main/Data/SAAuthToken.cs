using System;
using System.Linq;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using Microsoft.Phone.Data.Linq;
using Microsoft.Phone.Data.Linq.Mapping;
using System.Collections.ObjectModel;
using Awful.Models;
using System.Collections.Generic;
using System.Net;

namespace Awful.Data
{
    [Table]
    public class SAAuthToken
    {
        private EntityRef<Profile> _profileEntity = new EntityRef<Profile>();

        [Column(IsVersion = true)]
        private Binary _version;
        [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL Identity", CanBeNull = false, AutoSync = AutoSync.OnInsert)]
        public int ID { get; set; }
        [Column]
        public int ProfileID { get; set; }
        [Column(CanBeNull = false)]
        public string Name { get; set; }
        [Column(CanBeNull = false)]
        public string Value { get; set; }
        [Column(CanBeNull = false)]
        public string Path { get; set; }
        [Column(CanBeNull = false)]
        public string Domain { get; set; }
        [Association(Storage = "_profileEntity", ThisKey = "ProfileID", OtherKey = "ID", IsForeignKey = true, DeleteOnNull = true)]
        public Profile Profile
        {
            get { return this._profileEntity.Entity; }
            set
            {
                Profile previous = this._profileEntity.Entity;
                if ((previous != value) || (!this._profileEntity.HasLoadedOrAssignedValue))
                {
                    if (previous != null)
                    {
                        this._profileEntity.Entity = null;
                    }
                    this._profileEntity.Entity = value;
                    if (value != null)
                    {
                        this.ProfileID = value.ID;
                    }
                    else { this.ProfileID = -1; }
                }
            }
        }

        public SAAuthToken() { }

        public SAAuthToken(Cookie cookie) : this()
        {
            this.Name = cookie.Name;
            this.Path = cookie.Path;
            this.Domain = cookie.Domain;
            this.Value = cookie.Value;
        }

        internal Cookie AsCookie()
        {
            Cookie cookie = new Cookie(
                this.Name,
                this.Value,
                this.Path,
                this.Domain);

            return cookie;
        }
    }
}
