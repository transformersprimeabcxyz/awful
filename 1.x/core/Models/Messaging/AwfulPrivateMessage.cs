using System;
using KollaSoft;
using System.Threading;
using Awful.Core.Services;
using Awful.Core.Models.Messaging.Interfaces;

namespace Awful.Core.Models.Messaging
{
    /// <summary>
    /// TODO: Add summary for AwfulPrivateMessage.
    /// </summary>
    public class AwfulPrivateMessage : ViewModelBase, IPrivateMessage
    {
        #region fields
        private const int UNKNOWN_PRIVATE_MESSAGE_ID = -1;
        private string _subject;
        private string _to;
        private string _from;
        private string _body;
        private int _privateMessageID;
        private PrivateMessageStatus _status;
        private DateTime? _postMark;
        private static readonly Interfaces.IPrivateMessagingService Service;
        #endregion

        static AwfulPrivateMessage() { Service = Services.AwfulServiceManager.PrivateMessageService; }

        public AwfulPrivateMessage() : base() { this.PrivateMessageID = UNKNOWN_PRIVATE_MESSAGE_ID; }

        #region properties
        public int FolderID { get; set; }
        
        public string Subject
        {
            get
            {
                return this._subject;
            }
            set
            {
                this._subject = value;
                this.NotifyPropertyChangedAsync("Subject");
            }
        }

        public string To
        {
            get
            {
                return this._to;
            }
            set
            {
                this._to = value;
                this.NotifyPropertyChangedAsync("To");
            }
        }

        public string From
        {
            get
            {
                return this._from;
            }
            set
            {
                this._from = value;
                this.NotifyPropertyChangedAsync("From");
            }
        }

        public string Body
        {
            get
            {
                return this._body;
            }
            set
            {
                this._body = value;
                this.NotifyPropertyChangedAsync("Body");
            }
        }

        public DateTime? Postmark
        {
            get { return this._postMark; }
            set { this._postMark = value; }
        }

        public int PrivateMessageID
        {
            get { return this._privateMessageID; }
            set { this._privateMessageID = value; this.NotifyPropertyChangedAsync("PrivateMessageID"); }
        }

        public PrivateMessageStatus Status
        {
            get { return this._status; }
            set
            {
                this._status = value;
                this.NotifyPropertyChangedAsync("Status");
            }
        }
        #endregion

        #region methods

        protected override void InitializeForDesignMode()
        {
            this.Subject = "Sample Private Message";
            this.To = "Something Awful Goon";
            this.From = "bootleg robot";
            this.Body = "Here is some sample body text.";
            this.Status = PrivateMessageStatus.NEW;
            this.PrivateMessageID = 1;
        }

        public System.Collections.Generic.IList<Interfaces.IPrivateMessage> GetConversation()
        {
            // TODO : Add implementation for GetConversation().
            throw new NotImplementedException();
        }

        public Interfaces.IPrivateMessageRequest BeginForward()
        {
            Interfaces.IPrivateMessageRequest request = null;
            var signal = new AutoResetEvent(false);

            Service.BeginForwardToMessageRequest(this.PrivateMessageID,
                (result, resultRequest) =>
                {
                    if (result == ActionResult.Success) { request = resultRequest; }
                    signal.Set();
                });

            signal.WaitOne();
            return request;
        }

        public Interfaces.IPrivateMessageRequest BeginReply()
        {
            Interfaces.IPrivateMessageRequest request = null;
            var signal = new AutoResetEvent(false);

            Service.BeginReplyToMessageRequest(this.PrivateMessageID,
                (result, resultRequest) =>
                {
                    if (result == ActionResult.Success) { request = resultRequest; }
                    signal.Set();
                });

            signal.WaitOne();
            return request;
        }

        public bool Delete()
        {
            bool deleted = false;
            var signal = new AutoResetEvent(false);

            Service.DeleteMessageAsync(this.PrivateMessageID, this.FolderID, this.FolderID, result =>
                {
                    deleted = result == ActionResult.Success;
                    signal.Set();
                });

            signal.WaitOne();
            return deleted;
        }

        public bool MoveToFolder(Interfaces.IPrivateMessageFolder folder)
        {
            bool success = false;
            var signal = new AutoResetEvent(false);

            Service.MoveMessageAsync(this.PrivateMessageID, this.FolderID, folder.FolderID, result =>
                {
                    success = result == ActionResult.Success;
                    signal.Set();
                });

            signal.WaitOne();
            return success;
        }

        public IPrivateMessage Refresh()
        {
            IPrivateMessage result = null;
            var signal = new AutoResetEvent(false);
            Service.FetchMessageAsync(this.PrivateMessageID, (action, message) =>
                {
                    message = result;
                    signal.Set();
                });
            signal.WaitOne();
            return result;
        }

        public static IPrivateMessageRequest CreateNewMessage()
        {
            IPrivateMessageRequest result = null;
            var signal = new AutoResetEvent(false);
            Service.BeginNewMessageRequestAsync((action, request) =>
                {
                    result = request;
                    signal.Set();
                });
            signal.WaitOne();
            return result;
        }

        #endregion
    }
}
