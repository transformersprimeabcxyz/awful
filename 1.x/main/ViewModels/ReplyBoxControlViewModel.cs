using System;
using KollaSoft;
using System.Collections.Generic;
using Awful.Menus;

namespace Awful.ViewModels
{
    public class ReplyBoxControlViewModel : ViewModelBase
    {
        private const int MAX_REPLY_CHARS = 1977;

        private readonly List<TagOption> _tags = new List<TagOption>()
        {
            new BoldTag(),
            new ItalicTag(),
            new QuoteTag(),
            new UnderlineTag(),
            new PreTag(),
            new SpoilerTag(),
            new CodeTag(),
            new URLTag(),
            new EmailTag(),
            new ImageTag(),
            new VideoTag(),
            new YouTubeTag()
        };

        private int _maxChars;
        private string _status;
        private string _text;
        private string _title;

        public IList<TagOption> Tags
        {
            get { return this._tags; }
        }
       
        public string Title
        {
            get { return this._title; }
            set { this._title = value; NotifyPropertyChanged("Title"); }
        }

        public string Status
        {
            get { return this._status; }
            private set
            {
                this._status = value;
                NotifyPropertyChanged("Status");
            }
        }

        public string Text
        {
            get { return this._text; }
            set
            {
                this._text = value;
                NotifyPropertyChanged("Text");
                this.UpdateStatus(value);
            }
        }

        public ReplyBoxControlViewModel()
        {
            if (this.IsInDesignMode)
                this.InitializeForDesignMode();
            else
                this.Initialize();
        }

        private void UpdateStatus(string value)
        {
            this.Status = string.Format("Characters: {0}/{1}",
                string.IsNullOrEmpty(value) ? 0 : value.Length,
                this._maxChars);
        }

        protected void Initialize()
        {
            this._maxChars = MAX_REPLY_CHARS;
            this.Title = "reply";
            this.Text = string.Empty;
        }

        protected override void InitializeForDesignMode()
        {
            this._maxChars = MAX_REPLY_CHARS;
            this.Text = "Mary had a little lamb, whose fleece was white as snow.";
            this.Title = "reply";
        }
    }
}
