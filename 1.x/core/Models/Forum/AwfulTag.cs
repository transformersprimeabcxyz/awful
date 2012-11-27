using System;
using KollaSoft;
using System.Collections.Generic;
using Awful.Core.Models.Messaging.Interfaces;

namespace Awful.Core.Models
{
    public class AwfulTag : PropertyChangedBase, ThreadTag, IPrivateMessageIcon
    {
        public static readonly AwfulTag NoIcon;
        
        public string Title { get; private set; }
        public string Value { get; private set; }
        public string IconUri { get; private set; }

        static AwfulTag()
        {
            NoIcon = new AwfulTag("No Icon", "0", string.Empty);
        }

        public AwfulTag(string title, string value, string iconUri)
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

    public class SampleThreadIcons : List<AwfulTag>
    {
        public SampleThreadIcons()
            : base()
        {
            this.Add(new AwfulTag("sample one", "0", string.Empty));
            this.Add(new AwfulTag("sample two", "1", string.Empty));
        }
    }
}
