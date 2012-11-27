using System;
using System.Linq;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using Microsoft.Phone.Data.Linq;
using Microsoft.Phone.Data.Linq.Mapping;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Net;

namespace Awful.Core.Models
{
    [Table]
    public class AwfulAuthenticationToken
    {
        public const string AWFUL_PROFILE_FOREIGN_KEY = "AwfulProfileID";

        private EntityRef<AwfulProfile> _profileEntity = new EntityRef<AwfulProfile>();

        [Column(IsVersion = true)]
        private Binary _version;
        [Column(IsPrimaryKey = true)]
        public int AwfulProfileID { get; set; }
        [Column(IsPrimaryKey = true, CanBeNull = false)]
        public string Name { get; set; }
        [Column(CanBeNull = false)]
        public string Value { get; set; }
        [Column(CanBeNull = false)]
        public string Path { get; set; }
        [Column(CanBeNull = false)]
        public string Domain { get; set; }
        [Association(
            Storage = "_profileEntity", 
            ThisKey = AWFUL_PROFILE_FOREIGN_KEY, 
            OtherKey = AwfulProfile.AWFUL_PROFILE_PRIMARY_KEY, 
            IsForeignKey = true, 
            DeleteOnNull = true)]
        public AwfulProfile AwfulProfile
        {
            get { return this._profileEntity.Entity; }
            set
            {
                AwfulProfile previous = this._profileEntity.Entity;
                if ((previous != value) || (!this._profileEntity.HasLoadedOrAssignedValue))
                {
                    if (previous != null)
                    {
                        this._profileEntity.Entity = null;
                    }
                    this._profileEntity.Entity = value;
                    if (value != null)
                    {
                        this.AwfulProfileID = value.ID;
                    }
                    else { this.AwfulProfileID = -1; }
                }
            }
        }

        public AwfulAuthenticationToken() { }

        internal AwfulAuthenticationToken(Cookie cookie) : this()
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
