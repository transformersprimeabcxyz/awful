using System;
using System.Linq;
using Awful.Models;
using Awful.Helpers;
using System.Collections.Generic;
using System.Windows.Data;
using System.Collections.ObjectModel;
using Telerik.Windows.Data;
using Telerik.Windows.Controls;
using System.ComponentModel;
using System.Windows;
using System.Threading;
using System.Windows.Threading;
using KollaSoft;

namespace Awful.ViewModels
{
    public class HomePageViewModel : KollaSoft.ViewModelBase
    {
        private ICollection<SAForum> _forums;
        private ICollection<ThreadData> _bookmarks;
        private IEnumerable<SAForum> _favorites;
        private ObservableCollection<DataDescriptor> _forumGroupData = new ObservableCollection<DataDescriptor>();
        
        private RadJumpList _jumpList;
        private int _currentSectionIndex;
        private bool _isForumsLoading;
        private bool _isBookmarksLoading;
        private bool _isFavoritesLoading;

        private readonly Services.ForumDataRequest _forumService = new Services.ForumDataRequest();

        private const int FORUMS_INDEX = 0;
        private const int FAVORITES_INDEX = 1;
        private const int BOOKMARKS_INDEX = 2;

        private const string ALPHA_GROUP = "#abcdefghijklmnopqrstuvwxyz";
        private const string ALPHABET = "abcdefghijklmnopqrstuvwxyz";

        public int CurrentSectionIndex
        {
            get { return this._currentSectionIndex; }
            set
            {
                this._currentSectionIndex = value;
                NotifyPropertyChangedAsync("CurrentSectionIndex");
                switch (value)
                {
                    case FORUMS_INDEX:
                        if (this.Forums == null)
                            this.RefreshForums(false);
                        break;

                    case FAVORITES_INDEX:
                        if (this.Forums == null)
                            this.RefreshForums(false);
                        break;

                    case BOOKMARKS_INDEX:
                        if (this.Bookmarks == null)
                            this.RefreshBookmarks();
                        break;
                }
            }
        }

        public ICollection<ThreadData> Bookmarks
        {
            get { return this._bookmarks; }
            set
            {
                this._bookmarks = value;
                NotifyPropertyChanged("Bookmarks");
            }
        }

        public IEnumerable<SAForum> Favorites
        {
            get { return this._favorites; }
            set 
            { 
                this._favorites = value; 
                NotifyPropertyChanged("Favorites"); 
            }
        }

        public ObservableCollection<DataDescriptor> ForumGroups
        {
            get { return this._forumGroupData; }
        }

        public ICollection<SAForum> Forums
        {
            get { return this._forums; }
            set
            {
                this._forums = value;
                NotifyPropertyChanged("Forums");
                this.RefreshFavorites();
            }
        }

        public bool IsFavoritesLoading
        {
            get { return this._isFavoritesLoading; }
            private set
            {
                this._isFavoritesLoading = value;
                NotifyPropertyChanged("IsFavoritesLoading");
            }
        }

        public bool IsForumsLoading
        {
            get { return this._isForumsLoading; }
            private set
            {
                this._isForumsLoading = value;
                NotifyPropertyChanged("IsForumsLoading");

                if (value)
                    Loading.Fire(this);
                else
                    Loaded.Fire(this);
            }
        }

        public bool IsBookmarksLoading
        {
            get { return this._isBookmarksLoading; }
            private set
            {
                this._isBookmarksLoading = value;
                NotifyPropertyChanged("IsBookmarksLoading");

                if (value)
                    Loading.Fire(this);
                else
                    Loaded.Fire(this);
            }
        }

        public void RefreshFavorites()
        {
            if (this.Forums == null)
            {
                this.Favorites = null;
                return;
            }

            this.Favorites = null;
            this.IsFavoritesLoading = true;

            Action action = () =>
                {
                    var query = from f in this.Forums
                                where f.IsFavorite
                                select f;

                    var favs = query.ToList();
                    favs.Sort(new SortForumByName());
                    this.Favorites = favs;
                    this.IsFavoritesLoading = false;
                };

            action.DelayThenInvoke(new TimeSpan(0, 0, 0, 0, 500));
        }

        public void InitializeForumGroups(AwfulSettings.ForumGroup group)
        {
            if (this._jumpList == null) return;

            this._jumpList.GroupDescriptorsSource = null;
            this._jumpList.GroupPickerItemsSource = null;
            
            this._forumGroupData = null;
            this._forumGroupData = new ObservableCollection<DataDescriptor>();

            if (group == AwfulSettings.ForumGroup.Alphanumeric)
            {
                this._forumGroupData.Add(new GenericGroupDescriptor<ForumData, char>(GroupByAlpha));
                this._jumpList.GroupPickerItemTap += new EventHandler<GroupPickerItemTapEventArgs>(OnJumpListGroupPickerItemTap);
                this._jumpList.GroupPickerItemsSource = ALPHA_GROUP;
            }

            else
            {
                this._jumpList.GroupPickerItemTap -= new EventHandler<GroupPickerItemTapEventArgs>(OnJumpListGroupPickerItemTap);
                this._forumGroupData.Add(new GenericGroupDescriptor<ForumData, string>(GroupBySubforum));
            }

            this._jumpList.GroupDescriptorsSource = this.ForumGroups;
        }

        private void OnJumpListGroupPickerItemTap(object sender, GroupPickerItemTapEventArgs e)
        {
            foreach (DataGroup group in this._jumpList.Groups)
            {
                if (object.Equals(e.DataItem, group.Key))
                {
                    e.DataItemToNavigate = group;
                    return;
                }
            }
        }

        private char GroupByAlpha(ForumData forum)
        {
            string name = forum.ForumName.ToLower();
            char c;

            if (name.Substring(0, 4).Equals("the "))
                c = name[4];
            else
                c = name[0];

            if (ALPHABET.Contains(c))
                return c;

            return '#';
        }

        private string GroupBySubforum(ForumData forum)
        {
            var mapping = (forum as SAForum).Subforum;

            if (mapping != null)
                return mapping.Name;

            return "Other";
        }

        private void RefreshBookmarks()
        {
            if (this.IsInDesignMode) return;
            if (this.IsBookmarksLoading) return;

            else
            {
                ICollection<ThreadData> old = this.Bookmarks;
                this.IsBookmarksLoading = true;
                this.Bookmarks = null;

                AwfulControlPanel cp = new AwfulControlPanel();
                SAForumPageFactory.BuildAsync(result =>
                    {
                        switch (result)
                        {
                            case Awful.Core.Models.ActionResult.Success:
                                this.Bookmarks = cp.Threads;
                                break;

                            case Awful.Core.Models.ActionResult.Failure:
                                MessageBox.Show("Could not retrieve bookmarks. Check your internet connection and try again.", ":(", MessageBoxButton.OK);
                                this.Bookmarks = old;
                                break;

                            case Awful.Core.Models.ActionResult.Cancelled:
                                this.Bookmarks = old;
                                break;
                        }

                        this.IsBookmarksLoading = false;
                    }, cp);
            }
        }

        private void RefreshForums(bool ignoreCache)
        {
            if (this.IsInDesignMode) return;
            if (this.IsForumsLoading) return;

            this.IsForumsLoading = true;
            this.IsFavoritesLoading = true;
            
            ICollection<SAForum> old = null;
            if (this.Forums != null)
            {
                old = this.Forums;
            }

            this.Forums = null;
            this.InitializeForumGroups(App.Settings.ForumGrouping);

            this._forumService.GetForumList(ignoreCache, (result, response) =>
                {
                    switch (result)
                    {
                        case Awful.Core.Models.ActionResult.Success:
                            this.Forums = response.Forums;
                            break;

                        case Awful.Core.Models.ActionResult.Failure:
                            MessageBox.Show(
                                "Could not retrieve the forum collection. Check your internet connection and try again.", ":(", MessageBoxButton.OK);
                            this.Forums = old;
                            this.IsFavoritesLoading = false;
                            break;
                    }

                    this.IsForumsLoading = false;

                    if (result == Awful.Core.Models.ActionResult.Success) { this.RefreshFavorites(); }
                });
        }

        private void FilterForumFavorites(object sender, FilterEventArgs e)
        {
            var item = e.Item as ForumData;
            if (item == null) e.Accepted = false;
            else if (!item.IsFavorite) 
            { 
                e.Accepted = false; 
            } 
        }

        public void RefreshAsync()
        {
            switch (this.CurrentSectionIndex)
            {
                case FORUMS_INDEX:
                    if (this.IsForumsLoading) { this.CancelAsync(); }
                    else
                        this.RefreshForums(true);
                    break;

                case FAVORITES_INDEX:
                    this.RefreshFavorites(); 
                    break;

                case BOOKMARKS_INDEX:
                    if (this.IsBookmarksLoading) { this.CancelAsync(); }
                    else
                        this.RefreshBookmarks();
                    break;
            }
        }

        public void CancelAsync()
        {
            switch (this.CurrentSectionIndex)
            {
                case FORUMS_INDEX:
                    this._forumService.CancelAsync();
                    break;

                case BOOKMARKS_INDEX:
                    SAForumPageFactory.CancelAsync();
                    break;
            }
        }

        public event EventHandler Loading;
        public event EventHandler Loaded;

        public HomePageViewModel(RadJumpList forumsJumpList)
            : base()
        {
            this.SetJumpList(forumsJumpList);
        }

        void OnTimerTick(object sender, EventArgs e)
        {
            NotifyPropertyChanged("Favorites");
        }

        void OnFavoriteUpdate(object sender, EventArgs e)
        {
            //this.NotifyPropertyChanged("Favorites");
        }

        public HomePageViewModel()
        {
            Commands.ToggleFavoritesCommand.FavoriteAdded += new EventHandler(OnFavoriteUpdate);
            Commands.ToggleFavoritesCommand.FavoriteRemoved += new EventHandler(OnFavoriteUpdate);

            if (this.IsInDesignMode == false)
            {
                this.Initialize();
            }

            else
            {
                this.InitializeForDesignMode();
            }
        }

        public void SetJumpList(RadJumpList jump)
        {
            this._jumpList = jump;
        }

        protected void Initialize()
        {
            this.CurrentSectionIndex = App.Settings.MainMenuStartingIndex;
        }

        protected override void InitializeForDesignMode()
        {
            this.InitializeForumGroups(AwfulSettings.ForumGroup.Alphanumeric);

            List<SAForum> forums = new List<SAForum>();
            forums.Add(new SAForum() { ForumName = "A Forum", IsFavorite = true });
            forums.Add(new SAForum() { ForumName = "B Forum", IsFavorite = true });

            List<ThreadData> bookmarks = new List<ThreadData>();
            bookmarks.Add(new SAThread(null) { ThreadTitle = "Thread One", AuthorName = "author" });
            bookmarks.Add(new SAThread(null) { ThreadTitle = "Thread Two", AuthorName = "author" });

            this.Forums = forums;
            this.Bookmarks = bookmarks;
        }
    }

    public sealed class ForumGroupTemplateSelector : DataTemplateSelector
    {
        private DataTemplate m_LinkedItemTemplate;
        private DataTemplate m_EmptyItemTemplate;

        private const string alphabet = "abcdefghijklmnopqrstuvwxyz";

        public IEnumerable<SAForum> Forums
        {
            get { return GetValue(ForumsProperty) as IEnumerable<SAForum>; }
            set { SetValue(ForumsProperty, value); }
        }

        public static DependencyProperty ForumsProperty = DependencyProperty.Register
            ("Forums", typeof(IEnumerable<SAForum>), typeof(ForumGroupTemplateSelector), new PropertyMetadata(null));

        public DataTemplate LinkedItemTemplate
        {
            get { return m_LinkedItemTemplate; }
            set { m_LinkedItemTemplate = value; }
        }

        public DataTemplate EmptyItemTemplate
        {
            get { return m_EmptyItemTemplate; }
            set { m_EmptyItemTemplate = value; }
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (Forums == null)
                return LinkedItemTemplate;

            if (!(item is char))
                return LinkedItemTemplate;

            char group = (char)item;
            bool linked = false;
            switch (group)
            {
                case '#':
                    linked = Forums.Count(forum => ContainsOther(forum.ForumName)) > 0;
                    break;

                default:
                    linked = Forums.Count(forum => ContainsAlpha(forum.ForumName, group)) > 0;
                    break;
            }

            if (!linked)
                return EmptyItemTemplate;
            else
                return LinkedItemTemplate;

            //return base.SelectTemplate(item, container);
        }

        private bool ContainsOther(string value)
        {
            char c = value[0];
            return !alphabet.Contains(c);
        }

        private bool ContainsAlpha(string value, char group)
        {
            char c = value.ToLower()[0];
            return c == group;
        }
    }
}
