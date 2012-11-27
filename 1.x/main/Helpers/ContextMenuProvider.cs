using System;
using Telerik.Windows.Controls;
using Awful.Models;
using Awful.Helpers;

namespace Awful
{
    public class ContextMenuProvider : KollaSoft.PropertyChangedBase
    {
        private RadContextMenu _menu;
        public RadContextMenu Menu
        {
            get { return this._menu; }
            set
            {
                if (this._menu != null && this._menu.Equals(value)) return;

                this._menu = value;
                NotifyPropertyChanged("Menu");
            }
        }
    }
}
