using System;
using Awful.Models;
using Telerik.Windows.Controls;
using KollaSoft;

namespace Awful.Menus
{
    public class FavoritesContextMenu : AwfulContextMenu
    {
        private readonly Commands.ToggleFavoritesCommand _favorites = new Commands.ToggleFavoritesCommand();
        private readonly AwfulContextMenuItem _toggle = new AwfulContextMenuItem();

        public event EventHandler FavoritesChanged;

        public FavoritesContextMenu()
            : base()
        {
            this.Opening += new EventHandler<Telerik.Windows.Controls.ContextMenuOpeningEventArgs>(OnMenuOpening);
            this.Closed += new EventHandler(OnMenuClosed);
            this._toggle.Command = this._favorites;
            this._toggle.Tapped += new EventHandler<ContextMenuItemSelectedEventArgs>(OnToggleTapped);
            this.Items.Add(this._toggle);
        }

        void OnMenuClosed(object sender, EventArgs e)
        {
            this._toggle.CommandParameter = null;
        }

        void OnToggleTapped(object sender, ContextMenuItemSelectedEventArgs e)
        {
            this.FavoritesChanged.Fire(this);
        }

        private void OnMenuOpening(object sender, Telerik.Windows.Controls.ContextMenuOpeningEventArgs e)
        {
            var item = e.FocusedElement as RadDataBoundListBoxItem;
            if (item == null)
            {
                e.Cancel = true;
                return;
            }

            ForumData forum = item.DataContext as ForumData;
            if (forum == null)
            {
                e.Cancel = true;
                return;
            }

            this._toggle.CommandParameter = forum;
            this._toggle.Content = this._favorites.Header;
        }
    }
}
