using System;
using Telerik.Windows.Controls;
using System.Collections.Generic;
using Telerik.Windows.Controls.LoopingList;

namespace Awful.Helpers
{
    public class AwfulLoopingListDataSource<T> : LoopingListDataSource
    {
        private readonly RadLoopingList _list;
        private readonly IList<T> _data;
        private LoopingPanelScrollState _lastState = LoopingPanelScrollState.NotScrolling;
        private bool _block;

        public Dictionary<string, int> TitleTable { get; private set; }
        protected IList<T> Data { get { return this._data; } }
        protected RadLoopingList List { get { return this._list; } }

        protected ItemSelector SelectItem { get; set; }
        protected TitleGenerator GenerateTitle { get; set; }

        public delegate void ItemSelector(int index);
        public delegate string TitleGenerator(T item);

        public AwfulLoopingListDataSource(RadLoopingList list, IList<T> data)
            : base(data.Count)
        {
            this._list = list;
            this._data = data;
            this.TitleTable = new Dictionary<string, int>();
            InitializeBeforeAttaching();
            this._list.DataSource = this;
        }

        protected virtual void InitializeBeforeAttaching()
        {
            BindEvents();
        }

        private void BindEvents()
        {
            if (this._list == null) return;

            this.ItemNeeded += new EventHandler<LoopingListDataItemEventArgs>(OnItemNeeded);
            this.ItemUpdated += new EventHandler<LoopingListDataItemEventArgs>(OnItemUpdated);
            this._list.SelectedIndexChanged += new EventHandler(OnListSelectedIndexChanged);
            this._list.ScrollCompleted += 
                new EventHandler<Telerik.Windows.Controls.LoopingList.LoopingListScrollEventArgs>(OnScrollCompleted);
        }

        void OnScrollCompleted(object sender, LoopingListScrollEventArgs e)
        {
            switch (e.ScrollState)
            {
                case LoopingPanelScrollState.SnapScrolling:
                    if (this._lastState != LoopingPanelScrollState.NotScrolling)
                    {
                        this._block = false;
                    }
                    break;

                default:
                    this._block = true;
                    break;
            }

            this._list.SelectedIndex = -1;
            this._lastState = e.ScrollState;
        }

        void OnItemUpdated(object sender, LoopingListDataItemEventArgs e)
        {
            int index = e.Index;
            var tag = this._data[index];
            if (this.GenerateTitle == null) { e.Item.Text = tag.ToString(); }
            else { e.Item.Text = this.GenerateTitle(tag); }

            if (this.TitleTable.ContainsKey(e.Item.Text))
                this.TitleTable[e.Item.Text] = index;
            else
                this.TitleTable.Add(e.Item.Text, index);
        }

        void OnItemNeeded(object sender, LoopingListDataItemEventArgs e)
        {
            int index = e.Index;
            var tag = this._data[index];
            string title = null;
            if (this.GenerateTitle == null) { title = tag.ToString(); }
            else { title = this.GenerateTitle(tag); }
            e.Item = new LoopingListDataItem(title);

            if (this.TitleTable.ContainsKey(e.Item.Text))
                this.TitleTable[e.Item.Text] = index;
            else
                this.TitleTable.Add(e.Item.Text, index);
        }

        void OnListSelectedIndexChanged(object sender, EventArgs e)
        {
            if (this._list.SelectedIndex == -1) return;
            if (this._block)
            {
                this._list.SelectedIndex = -1;
                return;
            }

            if (this.SelectItem != null)
                this.SelectItem(this._list.SelectedIndex);
        }
    }
}
