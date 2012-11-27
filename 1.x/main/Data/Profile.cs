using System;
using System.Runtime.Serialization;
using KollaSoft;
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
    [Table(Name = "Profiles")]
    [Index(Name = "Index_Username", IsUnique = true, Columns = "Username ASC")]
    [DataContract]
    public class Profile : PropertyChangedBase
    {
        private string _username;
        private string _password;
        private EntitySet<SAAuthToken> _tokensEntity;
        //private EntitySet<ForumFavorite> _favoritesSet;
       
        [Column(IsVersion = true)]
        private Binary _version;

        [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL Identity",
            CanBeNull = false, AutoSync = AutoSync.OnInsert)]
        public int ID { get; set; }

        [Column(CanBeNull = false)]
        public string Username
        {
            get { return this._username; }
            set
            {
                if (this._username == value) return;
                NotifyPropertyChangingAsync("Username");
                this._username = value;
                NotifyPropertyChangedAsync("Username");
            }
        }

        [Column(CanBeNull = false)]
        public string Password
        {
            get { return this._password; }
            set
            {
                if (this._password == value) return;
                NotifyPropertyChangingAsync("Password");
                this._password = value;
                NotifyPropertyChangedAsync("Password");
            }
        }

        [Association(Storage="_tokensEntity", IsForeignKey = false, ThisKey = "ID", OtherKey = "ProfileID")]
        public EntitySet<SAAuthToken> Tokens
        {
            get { return this._tokensEntity; }
            set { this._tokensEntity.Assign(value); }
        }

        /*
        [Association(Name = "FK_Profile_ForumFavorites", Storage = "_favoritesSet", 
            OtherKey = "ProfileID", ThisKey = "ID")]
        private EntitySet<ForumFavorite> Favorites
        {
            get { return this._favoritesSet; }
            set { this._favoritesSet.Assign(value); }
        }
        */

        public Profile()
        {
            this._tokensEntity = new EntitySet<SAAuthToken>(this.OnTokenAdded, this.OnTokenRemoved);
            //this._favoritesSet = new EntitySet<ForumFavorite>(this.OnFavoritesAdded, this.OnFavoritesRemoved);
        }

        private void OnTokenAdded(SAAuthToken token) { token.Profile = this; }
        private void OnTokenRemoved(SAAuthToken token) { token.Profile = null; }

        private void OnFavoritesAdded(ForumFavorite favorite)
        {
            //favorite.Profile = this;
        }

        private void OnFavoritesRemoved(ForumFavorite favorite)
        {
            //favorite.Profile = null;
        }

        internal void AssignTokens(System.Collections.Generic.IList<System.Net.Cookie> iList)
        {
            this.Tokens.Clear();
            foreach (var cookie in iList)
            {
                SAAuthToken token = new SAAuthToken(cookie);
                this.Tokens.Add(token);
            }
        }

        internal IList<Cookie> GetTokens()
        {
            List<Cookie> result = new List<Cookie>();
            foreach (var token in this.Tokens)
            {
                result.Add(token.AsCookie());
            }
            return result;
        }
    }
}
