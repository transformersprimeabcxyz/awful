// -----------------------------------------------------------------------
// <copyright file="PageFactory.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Awful.Core.Models.Factory.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HtmlAgilityPack;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public interface Factory<T>
    {
        T Build();
    }
}
