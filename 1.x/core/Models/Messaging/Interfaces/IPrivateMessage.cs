// -----------------------------------------------------------------------
// <copyright file="IPrivateMessage.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Awful.Core.Models.Messaging.Interfaces
{
    using System;
    using System.ComponentModel;
    using System.Collections.Generic;

    /// <summary> TODO: Add summary here.
    /// TODO: Update summary.
    /// </summary>
    public interface IPrivateMessage : INotifyPropertyChanged
    {
        /// <summary> TODO: Add summary here.
        /// 
        /// </summary>
        string Subject { get; set; }
        /// <summary> TODO: Add summary here.
        /// 
        /// </summary>
        string To { get; set; }
        /// <summary> TODO: Add summary here.
        /// 
        /// </summary>
        string From { get; set; }
        /// <summary> TODO: Add summary here.
        /// 
        /// </summary>
        string Body { get; set; }
        /// <summary> TODO: Add summary here.
        /// 
        /// </summary>
        int PrivateMessageID { get; }
        /// <summary> TODO: Add summary here.
        /// 
        /// </summary>
        PrivateMessageStatus Status { get; }
        /// <summary>
        /// 
        /// </summary>
        DateTime? Postmark { get; }
        /// <summary> TODO: Add summary here.
        /// 
        /// </summary>
        /// <returns></returns>
        IList<IPrivateMessage> GetConversation();
        /// <summary> TODO: Add summary here.
        /// 
        /// </summary>
        /// <returns></returns>
        IPrivateMessageRequest BeginForward();
        /// <summary> TODO: Add summary here.
        /// 
        /// </summary>
        /// <returns></returns>
        IPrivateMessageRequest BeginReply();
        /// <summary> TODO: Add summary here.
        /// 
        /// </summary>
        /// <returns></returns>
        bool Delete();
        /// <summary> TODO: Add summary here.
        /// 
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        bool MoveToFolder(IPrivateMessageFolder folder);
        /// <summary>
        /// TODO: Add summary here.
        /// </summary>
        /// <returns></returns>
        IPrivateMessage Refresh();
    }
}
