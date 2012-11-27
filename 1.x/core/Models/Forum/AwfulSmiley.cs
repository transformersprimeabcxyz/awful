using System;
using System.Linq;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using Microsoft.Phone.Data.Linq;
using Microsoft.Phone.Data.Linq.Mapping;

namespace Awful.Core.Models
{
    [Table]
    public class AwfulSmiley : KollaSoft.PropertyChangedBase
    {
        private string _text;
        private string _title;
        private string _uri;

        private const string UNKNOWN = "Unknown";
        private const string NULL = ":NULL:";

        [Column(IsVersion = true)]
        private Binary _version;

        [Column(
            IsPrimaryKey = true, 
            IsDbGenerated = true, 
            DbType = "INT NOT NULL Identity",
            CanBeNull = false, 
            AutoSync = AutoSync.OnInsert)]
        public int ID { get; set; }

        [Column(CanBeNull = true)]
        public string Title
        {
            get
            {
                string value = this._title;
                if (value == null) return UNKNOWN;
                return this._title;
            }

            set
            {
                this.NotifyPropertyChangingAsync("Title");
                this._title = value;
                this.NotifyPropertyChangedAsync("Title");
            }
        }

        [Column(CanBeNull = true)]
        public string Text
        {
            get { return this._text; }
            set
            {
                this.NotifyPropertyChangingAsync("Text");
                this._text = value;
                this.NotifyPropertyChangedAsync("Text");
            }
        }

        [Column(CanBeNull = true)]
        public string Uri
        {
            get { return this._uri; }
            set
            {
                this.NotifyPropertyChangingAsync("Uri");
                this._uri = value;
                this.NotifyPropertyChangedAsync("Uri");
            }
        }

        public override string ToString()
        {
            string value = this.Text;
            if (value == null) return NULL;
            return this.Text;
        }
    }
}
