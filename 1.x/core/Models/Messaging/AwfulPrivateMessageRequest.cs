using System;
using KollaSoft;
using Awful.Core.Models.Messaging.Interfaces;
using System.Collections.Generic;
using System.Threading;

namespace Awful.Core.Models.Messaging
{
    public class AwfulPrivateMessageRequest : ViewModelBase, IPrivateMessageRequest
    {
        private IPrivateMessagingService _pmService;

        private IPrivateMessageIcon _selectedTag;
        public IPrivateMessageIcon SelectedTag
        {
            get { return this._selectedTag; }
            set
            {
                this._selectedTag = value;
                this.NotifyPropertyChangedAsync("SelectedIcon");
            }
        }

        private IList<IPrivateMessageIcon> _tags;
        public IList<IPrivateMessageIcon> Tags
        {
            get { return this._tags; }
            set
            {
                this._tags = value;
                this.NotifyPropertyChangedAsync("Tags");
            }
        }

        public DateTime? Postmark
        {
            get { return DateTime.Now; }
        }

        public bool IsForward
        {
            get;
            set;
        }

        public bool SendMessage()
        {
            var signal = new AutoResetEvent(false);
            bool value = false;
            this._pmService.SendMessageAsync(this, result => 
            { 
                value = result == ActionResult.Success; 
                signal.Set(); 
            });
            signal.WaitOne();
            return value;
        }

        private string _subject;
        public string Subject
        {
            get { return this._subject; }
            set
            {
                this._subject = value;
                this.NotifyPropertyChangedAsync("Subject");
            }
        }

        private string _to;
        public string To
        {
            get { return this._to; }
            set
            {
                this._to = value;
                this.NotifyPropertyChangedAsync("To");
            }
        }

        public string From { get; set; }

        private string _body;
        public string Body
        {
            get { return this._body; }
            set
            {
                this._body = value;
                this.NotifyPropertyChangedAsync("Body");
            }
        }

        public int PrivateMessageID { get; set; }
        
        public AwfulPrivateMessageRequest()
        {
            if (this.IsInDesignMode == false)
            {
                this.SelectedTag = AwfulTag.NoIcon;
                this.Tags = new List<IPrivateMessageIcon>();
                this._pmService = Services.AwfulServiceManager.PrivateMessageService;
            }
        }

        protected override void InitializeForDesignMode()
        {
            this.SelectedTag = AwfulTag.NoIcon;
            this.From  = "Sperglord";
            this.To = "SA Goon";
            this.Subject = "When is Awful ever gonna be ready?";
            this.Body = "So, when is this app ever gonna be ready, huh???";
            this.Tags = new List<IPrivateMessageIcon>();
        }

        #region Not in Use

        public PrivateMessageStatus Status { get; set; }

        public System.Collections.Generic.IList<Interfaces.IPrivateMessage> GetConversation()
        {
            throw new NotImplementedException("This method is not implemented for requests.");
        }

        public IPrivateMessageRequest BeginForward()
        {
            throw new NotImplementedException("This method is not implemented for requests.");
        }

        public IPrivateMessageRequest BeginReply()
        {
            throw new NotImplementedException("This method is not implemented for requests.");
        }

        public bool Delete() { throw new NotImplementedException("This method is not implemented for requests."); }

        public bool MoveToFolder(Interfaces.IPrivateMessageFolder folder)
        {
            throw new NotImplementedException("This method is not implemented for requests.");
        }

        IPrivateMessage IPrivateMessage.Refresh()
        {
            throw new NotImplementedException("This method is not implemented for requests.");
        }

        public string FormKey
        {
            get { return string.Empty; }
        }

        public string FormCookie
        {
            get { return string.Empty; }
        }

        #endregion
    }
}
