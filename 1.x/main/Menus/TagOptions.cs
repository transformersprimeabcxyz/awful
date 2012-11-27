using System;
using KollaSoft;

namespace Awful.Menus
{
    public interface TagOption 
    {
        string Title { get; }
        string Tag { get; }
        int EntryIndex { get; }
        string ApplyToText(string text, int startIndex);
    }

    public abstract class AbstractTagOption : PropertyChangedBase, TagOption
    {
        public virtual int EntryIndex
        {
            get { return this.GetEntryPosition(); }
        }

        public abstract string Title { get; }

        public abstract string Tag { get; }

        public virtual string ApplyToText(string text, int startIndex)
        {
            string result = text.Insert(startIndex, this.Tag);
            return result;
        }

        protected abstract int GetEntryPosition();
    }

    public sealed class BoldTag : AbstractTagOption
    {
        private const string TAG = "[b]TAPHERE[/b]";
        private const string TITLE = "bold";

        protected override int GetEntryPosition()
        {
            return "[b]".Length;
        }

        public override string Tag
        {
            get
            {
                return TAG;
            }
        }

        public override string Title
        {
            get { return TITLE; }
        }
    }

    public sealed class ItalicTag : AbstractTagOption
    {
        private const string TAG = "[i]TAPHERE[/i]";
        private const string TITLE = "italic";

        public override string Tag
        {
            get { return TAG; }
        }

        public override string Title
        {
            get { return TITLE; }
        }

        protected override int GetEntryPosition()
        {
            return "[i]".Length;
        }
    }

    public sealed class UnderlineTag : AbstractTagOption
    {
        private const string TAG = "[u]TAPHERE[/u]";
        private const string TITLE = "underline";

        public override string Tag
        {
            get { return TAG; }
        }

        public override string Title
        {
            get { return TITLE; }
        }

        protected override int GetEntryPosition()
        {
            return "[u]".Length;
        }
    }

    public sealed class PreTag : AbstractTagOption
    {
        private const string TAG = "[pre]TAPHERE[/pre]";
        private const string TITLE = "pre";

        public override string Tag
        {
            get { return TAG; }
        }

        public override string Title
        {
            get { return TITLE; }
        }

        protected override int GetEntryPosition()
        {
            return "[pre]".Length;
        }
    }

    public sealed class CodeTag : AbstractTagOption
    {
        private const string TAG = "[code]TAPHERE[/code]";
        private const string TITLE = "code";

        public override string Tag
        {
            get { return TAG; }
        }

        public override string Title
        {
            get { return TITLE; }
        }

        protected override int GetEntryPosition()
        {
            return "[code]".Length;
        }
    }

    public sealed class QuoteTag : AbstractTagOption
    {
        private const string TAG = "[quote=\"TAPHERE\"]TAPHERE[/quote]";
        private const string TITLE = "quote";

        public override string Tag
        {
            get { return TAG; }
        }

        public override string Title
        {
            get { return TITLE; }
        }

        protected override int GetEntryPosition()
        {
            return "[quote=\"\"]".Length;
        }
    }

    public sealed class SpoilerTag : AbstractTagOption
    {
        private const string TAG = "[spoiler]TAPHERE[/spoiler]";
        private const string TITLE = "spoiler";

        public override string Tag
        {
            get { return TAG; }
        }

        public override string Title
        {
            get { return TITLE; }
        }

        protected override int GetEntryPosition()
        {
            return "[spoiler]".Length;
        }
    }

    public sealed class URLTag : AbstractTagOption
    {
        private const string TAG = "[url=\"TAPHERE\"]TAPHERE[/url]";
        private const string TITLE = "url";

        public override string Tag
        {
            get { return TAG; }
        }

        public override string Title
        {
            get { return TITLE; }
        }

        protected override int GetEntryPosition()
        {
            return "[url=\"\"]".Length;
        }
    }

    public sealed class EmailTag : AbstractTagOption
    {
        private const string TAG = "[email]TAPHERE[/email]";
        private const string TITLE = "email";

        public override string Tag
        {
            get { return TAG; }
        }

        public override string Title
        {
            get { return TITLE; }
        }

        protected override int GetEntryPosition()
        {
            return "[email=\"\"]".Length;
        }
    }

    public sealed class ImageTag : AbstractTagOption
    {
        private const string TAG = "[img]TAPHERE[/img]";
        private const string TITLE = "image";

        public override string Tag
        {
            get { return TAG; }
        }

        public override string Title
        {
            get { return TITLE; }
        }

        protected override int GetEntryPosition()
        {
            return "[img]".Length;
        }
    }

    public sealed class VideoTag : AbstractTagOption
    {
        private const string TAG = "[video]TAPHERE[/video]";
        private const string TITLE = "video";

        public override string Tag
        {
            get { return TAG; }
        }

        public override string Title
        {
            get { return TITLE; }
        }

        protected override int GetEntryPosition()
        {
            return "[video]".Length;
        }
    }

    public sealed class YouTubeTag : AbstractTagOption
    {
        private const string TAG = "[video type=\"youtube\"]TAPHERE[/video]";
        private const string TITLE = "YouTube";

        public override string Tag
        {
            get { return TAG; }
        }

        public override string Title
        {
            get { return TITLE; }
        }

        protected override int GetEntryPosition()
        {
            return "[video type=\"youtube\"]".Length;
        }
    }

}
