// -----------------------------------------------------------------------
// <copyright file="AsynchronousPageFactory.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Awful.Core.Models.Factory.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public interface AsynchronousFactory<T>
    {
        void BuildAsync(Action<T> result);
    }
}
