// -----------------------------------------------------------------------
// <copyright file="IPrivateMessagingService.cs" company="KollaSoft">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Awful.Core.Models.Messaging.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Windows;

    /// <summary> TODO: Add summary here.
    ///
    /// </summary>
    public interface IPrivateMessagingService : IApplicationService
    {
        /// <summary>
        /// TODO: Add summary here.
        /// </summary>
        /// <param name="result"></param>
        void FetchFoldersAsync(Action<ActionResult, ICollection<IPrivateMessageFolder>> result);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="privateMessageId"></param>
        /// <param name="result"></param>
        void FetchMessageAsync(int privateMessageId, Action<ActionResult, IPrivateMessage> result);
        /// <summary> TODO: Add summary here.
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="result"></param>
        void SendMessageAsync(IPrivateMessageRequest request, Action<ActionResult> result);
        /// <summary> TODO: Add summary here.
        /// 
        /// </summary>
        /// <param name="thisFolderID"></param>
        /// <param name="privateMessageID"></param>
        /// <param name="folderID"></param>
        /// <param name="result"></param>
        void MoveMessageAsync(int privateMessageID, int thisFolderID, int folderID, Action<ActionResult> result);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="privateMessageIDs"></param>
        /// <param name="thisFolderID"></param>
        /// <param name="folderID"></param>
        /// <param name="result"></param>
        void MoveMessagesAsync(List<int> privateMessageIDs, int thisFolderID, int folderID, Action<ActionResult> result);
        /// <summary> TODO: Add summary here.
        /// 
        /// </summary>
        /// <param name="folderID"></param>
        /// <param name="thisFolderID"></param>
        /// <param name="privateMessageID"></param>
        /// <param name="result"></param>
        void DeleteMessageAsync(int privateMessageID, int thisFolderID, int folderID, Action<ActionResult> result);
        /// <summary> TODO: Add summary here.
        /// 
        /// </summary>
        /// <param name="result"></param>
        void FetchInboxAsync(Action<ActionResult, IPrivateMessageFolder> result);
        /// <summary> TODO: Add summary here.
        /// 
        /// </summary>
        /// <param name="result"></param>
        void FetchSentItemsAsync(Action<ActionResult, IPrivateMessageFolder> result);
        /// <summary> TODO: Add summary here.
        /// 
        /// </summary>
        /// <param name="folderID"></param>
        /// <param name="result"></param>
        void FetchFolderAsync(int folderID, Action<ActionResult, IPrivateMessageFolder> result);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        void BeginEditFolderRequest(Action<ActionResult, IPrivateMessageFolderRequest> result);
        /// <summary>
        /// TODO: Add summary here.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="result"></param>
        void SendEditFolderRequest(IPrivateMessageFolderRequest request, Action<ActionResult> result);
        /// <summary> TODO: Add summary here.
        /// 
        /// </summary>
        /// <param name="result"></param>
        void BeginNewMessageRequestAsync(Action<ActionResult, IPrivateMessageRequest> result);
        /// <summary> TODO: Add summary here.
        /// 
        /// </summary>
        /// <param name="privateMessageID"></param>
        /// <param name="result"></param>
        void BeginReplyToMessageRequest(int privateMessageID, Action<ActionResult, IPrivateMessageRequest> result);
        /// <summary> TODO: Add summary here.
        /// 
        /// </summary>
        /// <param name="privateMessageID"></param>
        /// <param name="result"></param>
        void BeginForwardToMessageRequest(int privateMessageID, Action<ActionResult, IPrivateMessageRequest> result);
    }
}
