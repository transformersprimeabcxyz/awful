using System;
using System.Windows.Media;

namespace Awful.Core.Models.Interfaces
{
    public interface ColorProfile
    {
        string ProfileName { get; }
        Color MainBackground { get; }
        Color MainForeground { get; }
        Color Accent { get; }
        Color LastReadTextForeground { get; }
        Color AdminAuthorForeground { get; }
        Color ModAuthorForeground { get; }
        Color AuthorForeground { get; }
    }
}
