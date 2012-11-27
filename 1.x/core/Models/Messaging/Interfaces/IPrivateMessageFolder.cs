// -----------------------------------------------------------------------
// <copyright file="IPrivateMessageFolder.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Awful.Core.Models.Messaging.Interfaces
{
    using System;
    using System.ComponentModel;
    using System.Collections.Generic;
    

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public interface IPrivateMessageFolder : INotifyPropertyChanged
    {
        /// <summary> TODO: Add summary here.
        /// 
        /// </summary>
        string Name { get; }
        /// <summary> TODO: Add summary here.
        /// 
        /// </summary>
        int FolderID { get; }
        /// <summary> TODO: Add summary here.
        /// 
        /// </summary>
        ICollection<IPrivateMessage> Messages { get; }
        /// <summary> TODO: Add summary here.
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        bool Rename(string name);
        /// <summary> TODO: Add summary here.
        /// 
        /// </summary>
        /// <returns></returns>
        bool Delete();
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IPrivateMessageFolder Refresh();
    }
}
