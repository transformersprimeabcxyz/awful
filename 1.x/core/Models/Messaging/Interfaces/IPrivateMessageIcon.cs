// -----------------------------------------------------------------------
// <copyright file="IPrivateMessageIcon.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Awful.Core.Models.Messaging.Interfaces
{
    using System;
    using System.ComponentModel;

    /// <summary> TODO: Add summary here.
    /// 
    /// </summary>
    public interface IPrivateMessageIcon : INotifyPropertyChanged
    {
        /// <summary> TODO: Add summary here.
        /// 
        /// </summary>
        string Title { get; }
        /// <summary> TODO: Add summary here.
        /// 
        /// </summary>
        string Value { get; }
        /// <summary> TODO: Add summary here.
        /// 
        /// </summary>
        string IconUri { get; }
    }
}
