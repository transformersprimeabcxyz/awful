using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using Awful.Models;
using System.Threading;

namespace Awful
{
	public partial class SmileyPanel : UserControl
	{
        public const int SMILIES_PER_PAGE = 20;
        private readonly SmileyPanelPageProvider _source = new SmileyPanelPageProvider();
		
        public SmileyPanel()
		{
			// Required to initialize variables
			InitializeComponent();
            BindEvents();

            this.smileyView.DataContext = this._source;
		}

        private void BindEvents()
        {
            this.smileyView.SelectionChanged += (sender, args) =>
                {
                    var page = this.smileyView.SelectedItem as SmileyPanelPage;
                    if (page == null) return;

                    page.Activate();
                };
        }

        private void SmileySelected(object sender, 
            System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var list = sender as ListBox;
            if (list.SelectedIndex == -1) return;

            var smiley = list.SelectedItem as AwfulSmiley;
            var text = smiley.Text;
            Clipboard.SetText(text);

            var message = string.Format("{0} copied to clipboard.", text);
            MessageBox.Show(message, ":)", MessageBoxButton.OK);
            list.SelectedIndex = -1;
        }
	}

    public class SmileyPanelPage : KollaSoft.PropertyChangedBase
    {
        private bool _isLoading;
        public bool IsLoading
        {
            get { return this._isLoading; }
            private set
            {
                this._isLoading = value;
                NotifyPropertyChangedAsync("IsLoading");
            }
        }

        public int Index { get; set; }

        private List<AwfulSmiley> _page;
        public List<AwfulSmiley> Page { get { return this._page; } }

        public void Activate()
        {
            if (this.Page != null && this.Page.Count > 0) return;
            if (this.IsLoading) return;

            this.IsLoading = true;
            ThreadPool.QueueUserWorkItem(state =>
                {
                    using (var db = new Data.SAForumDB())
                    {
                        int startIndex = this.Index * SmileyPanel.SMILIES_PER_PAGE;
                        var smilies = db.Smilies.Skip(startIndex).Take(SmileyPanel.SMILIES_PER_PAGE);
                        this._page = new List<AwfulSmiley>(smilies.Count());

                        foreach (var item in smilies)
                        {
                            this._page.Add(item);
                        }

                        this.IsLoading = false;
                        NotifyPropertyChangedAsync("Page");
                    }
                },
                null);
        }
    }

    public class SmileyPanelPageProvider : KollaSoft.KSVirtualizedList<SmileyPanelPage>
    {
        private readonly Dictionary<int, SmileyPanelPage> _cache = new Dictionary<int, SmileyPanelPage>();
        private int _count;

        public IList<SmileyPanelPage> Pages { get { return this; } }

        public SmileyPanelPageProvider()
        {
            using (var db = new Data.SAForumDB())
            {
                int count = db.Smilies.Count();
                this._count = (int)Math.Ceiling((double)count / (double)SmileyPanel.SMILIES_PER_PAGE);
            }
        }

        protected override SmileyPanelPage GetItem(int index)
        {
            if (this._cache.ContainsKey(index))
                return this._cache[index];
            else
            {
                SmileyPanelPage page = new SmileyPanelPage() { Index = index };
                return page;
            }
        }

        protected override void SetItem(SmileyPanelPage item, int index)
        {
            item.Index = index;
            if (this._cache.ContainsKey(index))
                this._cache[index] = item;
            else
                this._cache.Add(index, item);
        }

        public override bool Contains(SmileyPanelPage item)
        {
            return this._cache.ContainsKey(item.Index);
        }

        public override int Count
        {
            get
            {
                return this._count;
            }

            protected set
            {
                this._count = value;
                NotifyPropertyChangedAsync("Count");
            }
        }

        public override IEnumerator<SmileyPanelPage> GetEnumerator()
        {
            return new SmileyPanelPageProviderEnumerator(this);
        }

        public class SmileyPanelPageProviderEnumerator : IEnumerator<SmileyPanelPage>
        {
            private int _index = -1;
            private SmileyPanelPageProvider _target;

            public SmileyPanelPageProviderEnumerator(SmileyPanelPageProvider target) { this._target = target; }

            public SmileyPanelPage Current
            {
                get 
                {
                    if (_index == -1) return null;
                    else { return this._target[_index]; }
                }
            }

            public void Dispose()
            {
                this._index = -1;
                this._target = null;
            }

            object System.Collections.IEnumerator.Current
            {
                get { return this.Current; }
            }

            public bool MoveNext()
            {
                int newIndex = this._index + 1;
                if (newIndex < this._target.Count)
                {
                    this._index = newIndex;
                    return true;
                }

                return false;
            }

            public void Reset()
            {
                this._index = -1;
            }
        }

        public override int IndexOf(SmileyPanelPage item)
        {
            return item.Index;
        }
    }
}