using System;
using KollaSoft;
using System.Collections.Generic;

namespace Awful.Models
{
    public class SAThreadIcon : PropertyChangedBase
    {
        public static readonly SAThreadIcon Empty;
        
        public string Title { get; private set; }
        public string Value { get; private set; }
        public string IconUri { get; private set; }

        static SAThreadIcon()
        {
            Empty = new SAThreadIcon("No Icon", "0", string.Empty);
        }

        public SAThreadIcon(string title, string value, string iconUri)
        {
            this.Title = title;
            this.Value = value;
            this.IconUri = iconUri;
        }

        public override string ToString()
        {
            return this.Title;
        }
    }

    public class SampleThreadIcons : List<SAThreadIcon>
    {
        public SampleThreadIcons()
            : base()
        {
            this.Add(new SAThreadIcon("sample one", "0", string.Empty));
            this.Add(new SAThreadIcon("sample two", "1", string.Empty));
        }
    }
}
