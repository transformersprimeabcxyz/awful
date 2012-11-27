// -----------------------------------------------------------------------
// <copyright file="AwfulApplicationSettings.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Awful.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public interface AwfulApplicationSettings
    {
        int CurrentProfileID { get; set; }
        string CurrentThemeName { get; set; }
        int Fontsize { get; set; }
    }
}
