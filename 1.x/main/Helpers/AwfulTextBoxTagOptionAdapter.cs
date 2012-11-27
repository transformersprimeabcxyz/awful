using System;
using Awful.Menus;
using System.Windows.Controls;
using System.Collections.Generic;
using Telerik.Windows.Controls;
using System.Windows;

namespace Awful.Helpers
{
    public class AwfulTextBoxTagOptionAdapter : AwfulLoopingListDataSource<TagOption>
    {
        private readonly TextBox _textBox;

        private void OnTagSelected(int index)
        {
            this.CopyTagToClipboard(index);
        }

        private void CopyTagToClipboard(int index)
        {

            var tag = this.Data[index];
            Clipboard.SetText(tag.Tag);
            MessageBox.Show(string.Format("The {0} tag has been copied to the clipboard.", tag.Title), ":)", MessageBoxButton.OK);
        }

        private void ApplyTagToText(int index)
        {
            var tag = this.Data[index];
            int cursorIndex = this._textBox.SelectionStart;

            EventHandler handler = null;
            handler = (obj, textArgs) =>
            {
                this._textBox.TextChanged -= new TextChangedEventHandler(handler);
                this._textBox.Focus();
                this._textBox.SelectionStart = cursorIndex + tag.EntryIndex;
            };

            this._textBox.TextChanged += new TextChangedEventHandler(handler);
            this._textBox.Text = tag.ApplyToText(this._textBox.Text, cursorIndex);
            this.List.SelectedIndex = -1;
        }

        private string GetTagTitle(TagOption tag)
        {
            return tag.Title;
        }

        public AwfulTextBoxTagOptionAdapter(TextBox textBox, RadLoopingList list, IList<TagOption> tags)
            : base(list, tags) { this._textBox = textBox; }

        protected override void InitializeBeforeAttaching()
        {
            base.InitializeBeforeAttaching();
            this.SelectItem = this.OnTagSelected;
            this.GenerateTitle = this.GetTagTitle;
        }
    }
}
