using System;

namespace Awful.Core.Models.Interfaces
{
    public interface Refreshable<T>
    {
        void RefreshAsync(Action<T> result);
    }
}
