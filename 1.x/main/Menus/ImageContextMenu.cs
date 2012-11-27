using System;
using Telerik.Windows.Controls;

namespace Awful.Menus
{
    public class ImageContextMenu : AwfulContextMenu
    {
        private readonly RadContextMenuItem _save = new AwfulContextMenuItem();
        private readonly RadContextMenuItem _ie = new AwfulContextMenuItem();
        private readonly Commands.ImageCommand _imgCommand = new Commands.ImageCommand();

        private string _url;

        public string SelectedUrl
        {
            get { return this._url; }
            set
            {
                this._url = value;
                this.UpdateCommands(this._url);
            }
        }

        public ImageContextMenu()
            : base()
        {
            this.InitializeMenuItems();
            this.Items.Add(this._save);
            this.Items.Add(this._ie);
        }

        private void UpdateCommands(string url)
        {
            this._imgCommand.ImageUrl = url;
        }

        private void InitializeMenuItems()
        {
            this._save.Content = "save to pictures...";
            this._save.Command = this._imgCommand;
            this._save.CommandParameter = Commands.ImageCommand.SAVE_COMMAND;

            this._ie.Content = "open in IE";
            this._ie.Command = this._imgCommand;
            this._ie.CommandParameter = Commands.ImageCommand.OPEN_COMMAND;
        }
    }
}
