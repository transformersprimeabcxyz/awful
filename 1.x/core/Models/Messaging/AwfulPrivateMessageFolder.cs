using System;
using KollaSoft;
using Awful.Core.Models.Messaging.Interfaces;
using System.Collections.Generic;
using System.Threading;

namespace Awful.Core.Models.Messaging
{
    public class AwfulPrivateMessageFolder : ViewModelBase, IPrivateMessageFolder
    {
        public static readonly AwfulPrivateMessageFolder Inbox;
        public static readonly AwfulPrivateMessageFolder SentItems;
        private static readonly IPrivateMessagingService Service;

        public static readonly int[] READONLY_FOLDER_LIST = new int[] { 0, -1 };

        static AwfulPrivateMessageFolder()
        {
            Service = Services.AwfulServiceManager.PrivateMessageService;
            Inbox = new AwfulPrivateMessageFolder() { FolderID = 0 };
            SentItems = new AwfulPrivateMessageFolder() { FolderID = 1 };
        }

        public const int UNKNOWN_FOLDER_ID = -2;
       
        private string _name;
        public string Name 
        {
            get { return this._name; }
            set
            {
                this._name = value;
                this.NotifyPropertyChangedAsync("Name");
            }
        }

        public int FolderID { get; set; }

        private List<IPrivateMessage> _message;
        public ICollection<Interfaces.IPrivateMessage> Messages
        {
            get
            {
                if (this._message == null) { _message = new List<IPrivateMessage>(); }
                return this._message;
            }
        }

        public IPrivateMessageFolder Refresh()
        {
            return RefreshFolder(this);
        }

        public bool Rename(string name)
        {
            bool value = RenameFolder(this, name);
            return value;
        }

        public bool Delete()
        {
            bool value = DeleteFolder(this);
            return value;
        }

        public AwfulPrivateMessageFolder() { this.FolderID = UNKNOWN_FOLDER_ID; }

        protected override void InitializeForDesignMode()
        {
            this.Name = "Sample Folder";
            this.FolderID = -1;
            this.Messages.Add(new AwfulPrivateMessage());
            this.Messages.Add(new AwfulPrivateMessage());
        }

        public static IPrivateMessageFolder CreateNewFolder(string name)
        {
            IPrivateMessageFolder result = null;
            IPrivateMessageFolderRequest request = null;
            var signal = new AutoResetEvent(false);
            Service.BeginEditFolderRequest((a, r) =>
                {
                    request = r;
                    signal.Set();
                });
            
            signal.WaitOne();
            
            if (request != null)
            {
                request.CreateFolder(name);
                bool success = false;
                request.SendRequest(a =>
                    {
                        success = a == ActionResult.Success;
                        signal.Set();
                    });
                signal.WaitOne();
                if (success) { result = new AwfulPrivateMessageFolder() { Name = name }; }
            }

            return result;
        }

        private static IPrivateMessageFolder RefreshFolder(AwfulPrivateMessageFolder folder)
        {
            var signal = new AutoResetEvent(false);
            IPrivateMessageFolder result = null;
            Service.FetchFolderAsync(folder.FolderID, (s, newFolder) =>
            {
                result = newFolder;
                signal.Set();
            });
            signal.WaitOne();
            return result;
        }

        private static bool RenameFolder(AwfulPrivateMessageFolder folder, string name)
        {
            bool value = false;
            if (folder.FolderID != UNKNOWN_FOLDER_ID)
            {
                var signal = new AutoResetEvent(false);
                IPrivateMessageFolderRequest request = null;
                Service.BeginEditFolderRequest((a, result) => { request = result; signal.Set(); });
                signal.WaitOne();
                if (request != null)
                {
                    request.RenameFolder(folder, name);
                    Service.SendEditFolderRequest(request, result => 
                    {
                        value = result == Models.ActionResult.Success;
                        signal.Set(); 
                    });

                    signal.WaitOne();
                    if (value) { folder.Name = name; }
                }
            }
            return value;
        }

        private static bool DeleteFolder(IPrivateMessageFolder folder)
        {
            bool value = false;
            if (folder.FolderID != UNKNOWN_FOLDER_ID)
            {
                var signal = new AutoResetEvent(false);
                IPrivateMessageFolderRequest request = null;
                Service.BeginEditFolderRequest((a, result) => { request = result; signal.Set(); });
                signal.WaitOne();
                if (request != null)
                {
                    request.DeleteFolder(folder);
                    Service.SendEditFolderRequest(request, result => { value = result == ActionResult.Success; signal.Set(); });
                    signal.WaitOne();
                }
            }
            return value;
        }
    }
}
