// -----------------------------------------------------------------------
// <copyright file="IPrivateMessageFolderRequest.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Awful.Core.Models.Messaging.Interfaces
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public interface IPrivateMessageFolderRequest
    {
        void RenameFolder(IPrivateMessageFolder folder, string newFolderName);
        void CreateFolder(string folderName);
        void DeleteFolder(IPrivateMessageFolder folder);
        void SendRequest(Action<ActionResult> result);
        string GenerateRequestData();
    }
}
