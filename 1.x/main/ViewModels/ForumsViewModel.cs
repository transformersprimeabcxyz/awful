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
    public sealed class ForumsViewModel : PropertyChangedBase
    {
        public static ForumsViewModel Instance { get; private set; }
        static ForumsViewModel() { Instance = new ForumsViewModel(); }

        private const string alphabet = "abcdefghijklmnopqrstuvwxyz";
        private const string numbers = "1234567890";
        private const string m_groups = "#abcdefghijklmnopqrstuvwxyz";
        
        public event EventHandler ForumsLoading;
        public event EventHandler ForumsLoaded;

		// property variables
		private IList<ForumData> m_Forums;
		private readonly DataTemplateSelector m_EmptySelector = new DataTemplateSelector();
		private readonly ObservableCollection<DataDescriptor> m_sortData = new ObservableCollection<DataDescriptor>();
		private readonly ObservableCollection<ForumData> m_Favorites = new ObservableCollection<ForumData>();
		private ObservableCollection<DataDescriptor> m_groupData;
		private int m_FavoriteIndex = -1;
        private bool m_IsLoading;
		
		private IList<ForumData> _forumsCache;
        private readonly Services.ForumListService service = new Services.ForumListService();
        private RadJumpList _jumpList;
        private readonly CollectionViewSource favorites = new CollectionViewSource();
        private readonly DispatcherTimer _forumsTimer = new DispatcherTimer();
        private readonly DispatcherTimer _favoritesTimer = new DispatcherTimer();
        private readonly GenericSortDescriptor<ForumData, char> sortByName = new GenericSortDescriptor<ForumData, char>();
       
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
        
        private ForumsViewModel() 
        {
            InitializeMembers();
            BindEvents();
        }

        private void InitializeMembers()
        {
            this.sortByName.KeySelector = (forum => forum.ForumName[0]);
            this.m_sortData.Add(sortByName);
            TimeSpan delay = new TimeSpan(0, 0, 0, 0, 350);
            this._favoritesTimer.Interval = delay;
            this._forumsTimer.Interval = delay;
        }

        private void BindEvents()
        {
            Commands.ToggleFavoritesCommand.FavoritesChanged += new EventHandler(OnFavoritesChanged);
            this.favorites.Filter += new FilterEventHandler(FilterFavorites);
            this._favoritesTimer.Tick += new EventHandler(OnFavoritesTimerTick);
            this._forumsTimer.Tick += new EventHandler(OnForumsTimerTick);
        }

        private void OnForumsTimerTick(object sender, EventArgs e)
        {
            this._forumsTimer.Stop();
            this.Forums = _forumsCache;
            this.IsLoading = false;
            this.ForumsLoaded.Fire(this);
        }

        public void SetJumpList(RadJumpList list)
        {
            this._jumpList = list;
            this._jumpList.SortDescriptorsSource = this.SortData;
            this.RefreshGroups(Settings.Current.ForumGrouping);
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

        public void CancelAsync() { this.service.CancelAsync(); }
        
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

        private void OnFavoritesTimerTick(object sender, EventArgs e)
        {
            this._favoritesTimer.Stop();

            if (m_Forums.IsNullOrEmpty()) return;

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

        private void OnFavoritesChanged(object sender, EventArgs e)
        {
            // a bit hackish, but whatever, I don't care at this point. UIs...
            this._favoritesTimer.Start();
        }

        private void RefreshFavorites()
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
			if (_forumsCache != null)
			{
				ForumsLoading.Fire(this);
                IsLoading = true;
                _forumsTimer.Start();
			}
			else
			{
				Refresh();
			}
        }
		
		public void Refresh()
		{
			ForumsLoading.Fire(this);
			this.IsLoading = true;
			
            this.service.GetForumList((result, list) =>
            {
                switch (result)
                {
                    case ActionResult.Success:
                        this.favorites.Source = list;
					    this._forumsCache = list;
                        this.Forums = list;
                        this.RefreshFavorites();
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

                this.IsLoading = false;
            });
		}
    }

  
}
