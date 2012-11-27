using System;
using Awful.Helpers;
using Awful.Models;
using Telerik.Windows.Controls;
using KollaSoft;

namespace Awful.Menus
{
    public class PostContextMenu : AwfulContextMenu
    {
        private readonly RadContextMenuItem _edit = new AwfulContextMenuItem();
        private readonly RadContextMenuItem _quote = new AwfulContextMenuItem();
        private readonly RadContextMenuItem _mark = new AwfulContextMenuItem();
        private readonly RadContextMenuItem _byAuthor = new AwfulContextMenuItem();
        private readonly Commands.PostCommand _postCommand = new Commands.PostCommand();

        private const string SHOW_ALL_POSTS = "show all posts";
        private const string FILTER_POSTS = "show posts from this author";

        private PostData _post;

        public PostData SelectedPost
        {
            get { return this._post; }
            set
            {
                this._post = value;
                this.UpdateCommands(this._post);
            }
        }

        public event EventHandler EditTapped;
        public event EventHandler FilterByAuthorTapped;
        public event EventHandler<WebContentLoadedEventArgs> ContentLoaded;
        public event EventHandler ContentLoading;

        public PostContextMenu()
            : base()
        {
            this._postCommand.ContentLoading += new EventHandler(OnPostCommandContentLoading);
            this._postCommand.ContentLoaded += new EventHandler<WebContentLoadedEventArgs>(OnPostCommandContentLoaded);

            this.InitializeMenuItems();
            this.Items.Add(this._edit);
            this.Items.Add(this._quote);
            this.Items.Add(this._mark);
            this.Items.Add(this._byAuthor);
        }

        private void UpdateCommands(PostData postData)
        {
            this._postCommand.CurrentPost = postData as SAPost;
        }

        private void OnPostCommandContentLoaded(object sender, WebContentLoadedEventArgs e)
        {
            this.ContentLoaded.Fire(this, e);
        }

        private void OnPostCommandContentLoading(object sender, EventArgs e)
        {
            this.ContentLoading.Fire(this);
        }

        private bool _AuthorFilterEnabled;

        public bool AuthorFilterEnabled
        {
            get { return this._AuthorFilterEnabled; }
            set 
            { 
                this._AuthorFilterEnabled = value;
                if (value)
                {
                    this._byAuthor.Content = SHOW_ALL_POSTS;
                }
                else { this._byAuthor.Content = FILTER_POSTS; }
            }
        }

        private void InitializeMenuItems()
        {
            //this.Opening += new EventHandler<ContextMenuOpeningEventArgs>(OnMenuOpening);
            this._edit.Content = "edit...";
            this._edit.Command = this._postCommand;
            this._edit.CommandParameter = Commands.PostCommand.EDIT;
            this._edit.Tapped += new EventHandler<ContextMenuItemSelectedEventArgs>(OnEditCommandTapped);

            this._quote.Content = "quote...";
            this._quote.Command = this._postCommand;
            this._quote.CommandParameter = Commands.PostCommand.QUOTE;

            this._mark.Content = "mark post as read...";
            this._mark.Command = this._postCommand;
            this._mark.CommandParameter = Commands.PostCommand.MARK;

            this._byAuthor.Content = this.AuthorFilterEnabled ? SHOW_ALL_POSTS : FILTER_POSTS;
            this._byAuthor.Tapped += new EventHandler<ContextMenuItemSelectedEventArgs>(OnByAuthorTapped);
        }

        void OnMenuOpening(object sender, ContextMenuOpeningEventArgs e)
        {
            if (this._AuthorFilterEnabled) { this._byAuthor.Content = "show all posts"; }
            else { this._byAuthor.Content = "show posts by author"; }
        }

        void OnByAuthorTapped(object sender, ContextMenuItemSelectedEventArgs e)
        {
            this.FilterByAuthorTapped.Fire(this);
            this.AuthorFilterEnabled = !this.AuthorFilterEnabled;
        }

        void OnEditCommandTapped(object sender, ContextMenuItemSelectedEventArgs e)
        {
            this.EditTapped.Fire(this);
        }
    }
}
