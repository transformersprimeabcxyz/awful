using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.LoopingList;
using System.Collections.Generic;
using KollaSoft;
using Awful.Helpers;

namespace Awful
{
	public partial class ReplyBoxControl : UserControl
	{
        private bool _listEnabled;
        public bool IsListEnabled
        {
            get { return this._listEnabled; }
            set
            {
                this._listEnabled = value;
                if (value)
                    VisualStateManager.GoToState(this, "ShowList", true);
                else
                    VisualStateManager.GoToState(this, "HideList", true);
            }
        }

        public Awful.ViewModels.ReplyBoxControlViewModel Context
        {
            get { return this.Resources["dataModel"] as Awful.ViewModels.ReplyBoxControlViewModel; }
        }

        private bool _block;
        private AwfulTextBoxTagOptionAdapter _tagSource;
        private LoopingPanelScrollState _lastState = LoopingPanelScrollState.NotScrolling;
        private readonly Dictionary<string, int> _loopTable = new Dictionary<string, int>(10);

        public event EventHandler SmileyWindowRequested;

		public ReplyBoxControl()
		{
			// Required to initialize variables
			InitializeComponent();

            this.replyText.Text = string.Empty;
            this.replyTitle.Text = string.Empty;

            BindEvents();
            InteractionEffectManager.AllowedTypes.Add(typeof(LoopingListItem));
            InteractionEffectManager.AllowedTypes.Add(typeof(Button));

            this.IsListEnabled = false;
            this._tagSource = new AwfulTextBoxTagOptionAdapter(this.replyText, this.tagOptions,
                this.Context.Tags);
		}

        private void BindEvents()
        {
            this.SizeChanged += (sender, args) =>
                {
                    this.tagOptions.ItemWidth = this.controlsLeftBorder.ActualWidth;
                };

            
        }

		private void OnReplyBoxTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{
			// TODO: Add event handler implementation here.
		}

		private void OnTagsButtonClick(object sender, System.Windows.RoutedEventArgs e)
		{
            this.IsListEnabled = !this.IsListEnabled;
		}

		private void OnTagOptionSelected(object sender, System.EventArgs e)
		{
			// TODO: Add event handler implementation here.
		}

        private void OnOptionsButtonClick(object sender, RoutedEventArgs e)
        {
            var content = (sender as Button).Content as LoopingListDataItem;
            var title = content.Text;
            var index = this._tagSource.TitleTable[title];

            if (this.tagOptions.SelectedIndex == index)
            {
                this.tagOptions.SelectedIndex = -1;
            }

            this.tagOptions.SelectedIndex = index;
        }

        private void OnButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
        	var button = sender as Button;
			if (button == null) return;
			switch(button.Tag.ToString())
			{
				case "TAGS":
					this.IsListEnabled = !this.IsListEnabled;
					break;
					
				case "SMILEY":
                    this.SmileyWindowRequested.Fire(this);
					break;
			}
        }
	}
}