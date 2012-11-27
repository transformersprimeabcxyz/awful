// -----------------------------------------------------------------------
// <copyright file="IPrivateMessageRequest.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Awful.Core.Models.Messaging.Interfaces
{
    using System;

    /// <summary> TODO: Add summary here.
    /// 
    /// </summary>
    public interface IPrivateMessageRequest : IPrivateMessage
    {
        /// <summary> TODO: Add summary here.
        /// 
        /// </summary>
        IPrivateMessageIcon SelectedTag { get; set; }
        /// <summary> TODO: Add summary here.
        /// 
        /// </summary>
        string FormKey { get; }
        /// <summary> TODO: Add summary here.
        /// 
        /// </summary>
        string FormCookie { get; }
        /// <summary>
        /// 
        /// </summary>
        bool IsForward { get; }
        /// <summary> TODO: Add summary here.
        /// 
        /// </summary>
        /// <returns></returns>
        bool SendMessage();
    }
}
