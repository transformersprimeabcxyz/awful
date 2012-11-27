using System;
using System.Collections.Generic;
using Awful.Core.Models.Messaging.Interfaces;
using System.Text;
using System.Net;

namespace Awful.Core.Models.Messaging
{
    public class AwfulEditFolderRequest : IPrivateMessageFolderRequest
    {
        private readonly Dictionary<int, string> _folderTable = new Dictionary<int, string>();
        private IPrivateMessagingService pmService;
        public IDictionary<int, string> FolderTable { get { return this._folderTable; } }
        public int NewFolderFieldIndex { get; set; }
        public int HighestIndex { get; set; }

        internal AwfulEditFolderRequest() { pmService = Services.AwfulServiceManager.PrivateMessageService;  }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="newFolderName"></param>
        public void RenameFolder(IPrivateMessageFolder folder, string newFolderName)
        {
            // folder index = folder id + 1
            int index = folder.FolderID + 1;
            // rename field
            this.FolderTable[index] = newFolderName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newFolderName"></param>
        public void CreateFolder(string newFolderName)
        {
            this.FolderTable[this.NewFolderFieldIndex] = newFolderName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folder"></param>
        public void DeleteFolder(IPrivateMessageFolder folder)
        {
            // folder index = folder id + 1
            int index = folder.FolderID + 1;
            // set folder field to empty
            this.FolderTable[index] = string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GenerateRequestData()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("action=doeditfolders&");
            ICollection<int> keys = this.FolderTable.Keys;
            foreach (var key in keys)
            {
                string folderName = this.FolderTable[key];
                folderName = HttpUtility.UrlEncode(folderName);
                folderName = folderName.Replace("%2b", "+");
                string queryString = string.Format("folderlist[{0}]", key);
                sb.AppendFormat("{0}={1}&", HttpUtility.UrlEncode(queryString), folderName);
            }

            sb.AppendFormat("highest={0}&", this.NewFolderFieldIndex);
            sb.Append("submit=Edit+Folders");
            return sb.ToString();
        }

        public void SendRequest(Action<ActionResult> result)
        {
            this.pmService.SendEditFolderRequest(this, result);
        }
    }
}
