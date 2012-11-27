using System;
using KollaSoft;
using System.Linq;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using Microsoft.Phone.Data.Linq;
using Microsoft.Phone.Data.Linq.Mapping;
using System.Collections.ObjectModel;
using Awful.Models;

namespace Awful.Data
{

    [Table(Name = "ForumFavorites")]
    [Index(Name = "Index_ProfileID", Columns = "ProfileID ASC", IsUnique = false)]
    public class ForumFavorite : PropertyChangedBase
    {
        //private EntityRef<SAForum> _forumEntity;
        //private EntityRef<Profile> _profileEntity;

        private bool _isFavorite;

        [Column(IsPrimaryKey = true, AutoSync = AutoSync.Default)]
        public int ProfileID { get; set; }

        [Column(IsPrimaryKey = true, AutoSync = AutoSync.Default)]
        public int ForumID { get; set; }

        [Column]
        public bool IsFavorite
        {
            get { return this._isFavorite; }
            set
            {
                NotifyPropertyChangingAsync("IsFavorite");
                this._isFavorite = value;
                NotifyPropertyChangedAsync("IsFavorite");
            }
        }

        [Column(IsVersion = true)]
        private Binary _version;

        /*
        [Association(Name = "FK_ForumFavorites_Profiles", Storage = "_profileEntity", ThisKey = "ProfileID", OtherKey = "ID", IsForeignKey = true, DeleteOnNull = true)]
        private Profile Profile
        {
            get { return this._profileEntity.Entity; }
            set
            {
                Profile prev = this._profileEntity.Entity;
                if ((prev != value) || (this._profileEntity.HasLoadedOrAssignedValue == false))
                {
                    NotifyPropertyChanging("Profile");

                    if (prev != null)
                    {
                        this._profileEntity.Entity = null;
                        //prev.Favorites.Remove(this);
                    }

                    this._profileEntity.Entity = value;

                    if (value != null)
                    {
                        //value.Favorites.Add(this);
                        this.ProfileID = value.ID;
                    }

                    else
                    {
                        this.ProfileID = default(int);
                    }

                    NotifyPropertyChanged("Profile");
                }
            }
        }

        [Association(Name = "FK_ForumFavorites_Forums", Storage = "_forumEntity", ThisKey = "ForumID", OtherKey = "ID", IsForeignKey = true, DeleteOnNull = true)]
        private SAForum Forum
        {
            get { return this._forumEntity.Entity; }
            set 
            { 
                SAForum prev = this._forumEntity.Entity;
                if ((prev != value) || (this._profileEntity.HasLoadedOrAssignedValue == false))
                {
                    NotifyPropertyChanging("Forum");

                    if (prev != null)
                    {
                        this._forumEntity.Entity = null;
                    }

                    this._forumEntity.Entity = value;

                    if (value != null)
                    {
                        this.ForumID = value.ID;
                    }

                    else
                    {
                        this.ForumID = default(int);
                    }

                    NotifyPropertyChanged("Forum");
                }
            }   
        }
        */
    }
}
