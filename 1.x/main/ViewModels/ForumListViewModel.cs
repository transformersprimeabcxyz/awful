using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows.Data;
using System.Linq;
using Awful.Models;
using System.Windows;
using System.Windows.Threading;
using Telerik.Windows.Data;
using Telerik.Windows.Controls;
using System.Collections;
using KollaSoft;

namespace Awful.ViewModels
{
    public sealed class ForumListViewModel : PropertyChangedBase
    {
        public static ForumListViewModel Instance { get; private set; }
        static ForumListViewModel() { Instance = new ForumListViewModel(); }

        private const string alphabet = "abcdefghijklmnopqrstuvwxyz";
        private const string numbers = "1234567890";
        private const string m_groups = "#abcdefghijklmnopqrstuvwxyz";
       
        public event EventHandler ForumsLoading;
        public event EventHandler ForumsLoaded;

        private Services.ForumListService service;
        private int m_FavoriteIndex = -1;
        private IList<ForumData> m_Forums;
      
        private bool m_IsLoading;
        private RadJumpList _jumpList;

        private readonly CollectionViewSource favorites = new CollectionViewSource();
        private readonly DispatcherTimer timer = new DispatcherTimer();
        private ObservableCollection<DataDescriptor> m_groupData;
        private readonly GenericSortDescriptor<ForumData, char> sortByName = new GenericSortDescriptor<ForumData, char>();
        private readonly ObservableCollection<DataDescriptor> m_sortData = new ObservableCollection<DataDescriptor>();
        private readonly ObservableCollection<ForumData> m_Favorites = new ObservableCollection<ForumData>();
        private readonly DataTemplateSelector m_EmptySelector = new DataTemplateSelector();

        public bool IsLoading
        {
            get { return m_IsLoading; }
            set
            {
                m_IsLoading = value;
                NotifyPropertyChanged("IsLoading");
            }
        }

        public IEnumerable Groups
        {
            get { return m_groups; }
        }

        public IEnumerable<DataDescriptor> GroupData
        {
            get { return m_groupData; }
        }

        public IEnumerable<DataDescriptor> SortData
        {
            get { return m_sortData; }
        }

        public IList<ForumData> Favorites
        {
            get { return m_Favorites; }
        }

        public int FavoriteIndex
        {
            get { return m_FavoriteIndex; }
            set
            {
                if (m_FavoriteIndex == value) return;
                m_FavoriteIndex = value;
                NotifyPropertyChanged("FavoriteIndex");
            }
        }

        public IList<ForumData> Forums
        {
            get 
            {
                return m_Forums; 
            }
            set
            {
                if (m_Forums == value) return;
                m_Forums = value;
                NotifyPropertyChanged("Forums");
            }
        }

        private ForumListViewModel() 
        {
            InitializeMembers();
            BindEvents();
        }

        private void InitializeMembers()
        {
            sortByName.KeySelector = (forum => forum.ForumName[0]);
            m_sortData.Add(sortByName);
            TimeSpan delay = new TimeSpan(0, 0, 0, 0, 350);
            timer.Interval = delay;
        }

        private void BindEvents()
        {
            ToggleFavoritesCommand.FavoritesChanged += new EventHandler(OnFavoritesChanged);
            favorites.Filter += new FilterEventHandler(FilterFavorites);
            timer.Tick += new EventHandler(OnTimerTick);
        }

        public void SetJumpList(RadJumpList list)
        {
            this._jumpList = list;
            this._jumpList.SortDescriptorsSource = this.SortData;
            RefreshGroups(Settings.Current.ForumGrouping);
        }

        public void RefreshGroups(AwfulSettings.ForumGroup grouping)
        {
            this._jumpList.GroupDescriptorsSource = null;
            this._jumpList.GroupPickerItemsSource = null;
            this.m_groupData = new ObservableCollection<DataDescriptor>();

            if (grouping == AwfulSettings.ForumGroup.Alphanumeric)
            {
                this._jumpList.GroupPickerItemTap += new EventHandler<GroupPickerItemTapEventArgs>(OnGroupItemTap);
                this._jumpList.GroupPickerItemsSource = this.Groups;
                this.m_groupData.Add(new GenericGroupDescriptor<ForumData, char>(GetForumAlphaGroup));           
            }
            else
            {
                this._jumpList.GroupPickerItemTap -= new EventHandler<GroupPickerItemTapEventArgs>(OnGroupItemTap);
                this.m_groupData.Add(new GenericGroupDescriptor<ForumData, string>(GetSubforumName));
            }

            NotifyPropertyChanged("Forums");
            this._jumpList.GroupDescriptorsSource = this.GroupData;
           
        }
        
        private void OnGroupItemTap(object sender, GroupPickerItemTapEventArgs e)
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

        private string GetSubforumName(ForumData forum)
        {
            var mapping = App.Database.SubForumMappings
                .Where(map => map.ForumID == forum.ForumID)
                .FirstOrDefault();

            if (mapping != null)
                return mapping.Subforum.Name;

            return "Other";
        }

        private char GetForumAlphaGroup(ForumData forum)
        {
            string name = forum.ForumName.ToLower();
            char c;

            if (name.Substring(0, 4).Equals("the "))
                c = name[4];
            else
                c = name[0];

            if (alphabet.Contains(c))
                return c;

            return '#';
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            var added = m_Forums.Where(forum => forum.IsFavorite);
            var removed = m_Forums.Where(forum => !forum.IsFavorite);
            
            foreach (var item in added)
            {
                if (m_Favorites.Contains(item) == false)
                    m_Favorites.Add(item);
            }

            foreach (var item in removed)
                if (m_Favorites.Contains(item))
                    m_Favorites.Remove(item);

            NotifyPropertyChanged("Favorites");
        }

        void OnFavoritesChanged(object sender, EventArgs e)
        {
            // a bit hackish, but whatever, I don't care at this point. UIs...
            timer.Start();
        }

        public void RefreshFavorites()
        {
            m_Favorites.Clear();
            var favs = m_Forums.Where(forum => forum.IsFavorite);
            foreach (var item in favs)
                m_Favorites.Add(item);
        }

        public void FilterFavorites(object sender, FilterEventArgs e)
        {
            var forum = e.Item as ForumData;
            if (forum == null)
            {
                e.Accepted = false;
                return;
            }

            if (forum.IsFavorite)
                e.Accepted = e.Accepted && true;
            else
                e.Accepted = false;
        }

        public void Load()
        {
            ForumsLoading.Fire(this);
            IsLoading = true;
            service = new Services.ForumListService();
            service.GetForumList((result, list) =>
            {
                switch (result)
                {
                    case ActionResult.Success:
                    favorites.Source = list;
                    Forums = list;
                    RefreshFavorites();
                    ForumsLoaded.Fire(this);
                    break;

                    case ActionResult.Failure:
                    MessageBox.Show("Could not retrive the forum list. Check your connection settings and tap refresh to try again.", ":(", MessageBoxButton.OK);
                    ForumsLoaded.Fire(this);
                    break;

                    case ActionResult.Cancelled:
                    ForumsLoaded.Fire(this);
                    break;
                }
                IsLoading = false;
            });
        }

        public void CancelAsync() { service.CancelAsync(); }
    }

    public sealed class ForumGroupTemplateSelector : DataTemplateSelector
    {
        private DataTemplate m_LinkedItemTemplate;
        private DataTemplate m_EmptyItemTemplate;

        private const string alphabet = "abcdefghijklmnopqrstuvwxyz";

        public IEnumerable<ForumData> Forums
        {
            get { return GetValue(ForumsProperty) as IEnumerable<ForumData>; }
            set { SetValue(ForumsProperty, value); }
        }

        public static DependencyProperty ForumsProperty = DependencyProperty.Register
            ("Forums", typeof(IEnumerable<ForumData>), typeof(ForumGroupTemplateSelector), new PropertyMetadata(null));

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

    public sealed class ForumsByGroup : List<ForumsInGroup>
    {
        private const string groups = "a b c d e f g h i j k l m n o p q r s t u v w x y z";

        public ForumsByGroup(List<ForumData> forums)
            : base()
        {
            var tokens = new List<string>(groups.Split(' '));
            foreach (var token in tokens)
            {
                ForumsInGroup group = new ForumsInGroup(token);
                var forumsInGroup = forums
                    .Where(forum => GetCategory(forum).Equals(group.Key))
                    .OrderBy((forum) => { return forum.ForumName; }, StringComparer.OrdinalIgnoreCase);

                group.AddRange(forumsInGroup);
                Add(group);
            }

            ForumsInGroup rest = new ForumsInGroup(".");
            var restOfForums = forums
                .Where(forum => tokens.IndexOf(GetCategory(forum)) < 0)
                .OrderBy((forum) => { return forum.ForumName; }, StringComparer.OrdinalIgnoreCase);

            rest.AddRange(restOfForums);
            Add(rest);
        }

        private string GetCategory(ForumData forum)
        {
            string name = forum.ForumName.Replace("The", "").Trim();
            return (name[0].ToString()).ToLowerInvariant();
        }
    }

    public sealed class ForumsInGroup : List<ForumData>
    {
        public string Key { get; set; }
        public bool HasItems { get { return this.Count != 0; } }

        public ForumsInGroup(string category) : base() { Key = category; } 
    }

    public abstract class ForumGroupItem
    {
        public string GroupName { get; protected set; }

        public delegate void ForumFilterDelegate(object sender,
            FilterEventArgs args);

        public ForumFilterDelegate FilterDelegate { get; private set; }

        public ForumGroupItem(string name,
            Func<Models.SAForum, bool> filterCheck)
        {
            this.GroupName = name;
            this.FilterDelegate = (obj, args) =>
                {
                    var forum = args.Item as Models.SAForum;
                    if (forum == null)
                        args.Accepted = false;

                    else if (filterCheck(forum))
                    {
                        args.Accepted = args.Accepted && true;
                    }

                    else
                        args.Accepted = false;
                };
        }
    }
}
