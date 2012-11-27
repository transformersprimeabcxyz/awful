using System;
using Telerik.Windows.Controls;
using Awful.Models;
using KollaSoft;

namespace Awful.Menus
{
    public abstract class ThreadContextMenu : AwfulContextMenu
    {
        private readonly AwfulContextMenuItem _jump = new AwfulContextMenuItem();
        private readonly AwfulContextMenuItem _clear = new AwfulContextMenuItem();
        private ThreadData m_threadData;
        private readonly Commands.ClearMarkedPostsCommand _clearCommand = new Commands.ClearMarkedPostsCommand();

        public event EventHandler<ContextMenuItemSelectedEventArgs> JumpToPageRequest;

        public ThreadContextMenu()
        {
            this.Opening += new EventHandler<ContextMenuOpeningEventArgs>(OnMenuOpening);
            
            this._jump.Content = "jump to page...";
            this._jump.Tapped += new EventHandler<ContextMenuItemSelectedEventArgs>(OnJumpTapped);

            this._clear.Content = "clear marked posts...";
            this._clear.Command = this._clearCommand;

            this.Items.Add(this._jump);
            this.Items.Add(this._clear);
        }

        public ThreadData SelectedThread
        {
            get { return this.m_threadData; }
            protected set
            {
                this.m_threadData = value;
                this.UpdateMenuItems();
            }
        }

        protected virtual void UpdateMenuItems()
        {
            this._clear.CommandParameter = this.SelectedThread;
        }

        protected void OnMenuOpening(object sender, ContextMenuOpeningEventArgs e)
        {
            RadDataBoundListBoxItem focused = e.FocusedElement as RadDataBoundListBoxItem;
            if (focused == null)
            {
                e.Cancel = true;
                return;
            }

            else
            {
                ThreadData thread = focused.DataContext as ThreadData;
                this.SelectedThread = thread;
            }
        }

        void OnJumpTapped(object sender, ContextMenuItemSelectedEventArgs e)
        {
            this._jump.Tag = this.SelectedThread;
            this.JumpToPageRequest.Fire(this, e);
        }
    }

    public class BookmarksContextMenu : ThreadContextMenu
    {
        private readonly Commands.RemoveBookmarkCommand _removeCommand = new Commands.RemoveBookmarkCommand();
        private readonly AwfulContextMenuItem _remove = new AwfulContextMenuItem();

        public BookmarksContextMenu()
            : base()
        {
            this._remove.Content = "remove from bookmarks...";
            this._remove.Command = this._removeCommand;
            this.Items.Add(this._remove);
        }

        protected override void UpdateMenuItems()
        {
            base.UpdateMenuItems();
            this._remove.CommandParameter = this.SelectedThread;
        }
    }

    public class ThreadsContextMenu : ThreadContextMenu
    {
        private readonly Commands.AddBookmarkCommand _addCommand = new Commands.AddBookmarkCommand();
        private readonly AwfulContextMenuItem _add = new AwfulContextMenuItem();

        public ThreadsContextMenu()
            : base()
        {
            this._add.Content = "add to bookmarks";
            this._add.Command = this._addCommand;
            this.Items.Add(this._add);
        }

        protected override void UpdateMenuItems()
        {
            base.UpdateMenuItems();
            this._add.CommandParameter = this.SelectedThread;
        }
    }
}
