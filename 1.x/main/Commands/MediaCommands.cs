using System;
using System.Windows.Input;
using System.Windows;
using Microsoft.Phone.Tasks;

namespace Awful.Commands
{
    public class ImageCommand : CommandBase
    {
        public const string SAVE_COMMAND = "SAVE";
        public const string OPEN_COMMAND = "OPEN";
        private readonly Services.AwfulMediaService mediaSvc = new Services.AwfulMediaService(); 
        private WebBrowserTask _task;
        private string m_ImageUrl;

        private WebBrowserTask _Task
        {
            get
            {
                if (_task == null)
                    _task = new WebBrowserTask();
                return _task;
            }
        }

        public string ImageUrl
        {
            get { return m_ImageUrl; }
            set
            {
                if (m_ImageUrl == value) return;
                m_ImageUrl = value;
                NotifyPropertyChangedAsync("ImageUrl");
                NotifyCanExecuteChanged();
            }
        }

        public override void Execute(object parameter)
        {
           if(CanExecute(parameter))
           {
               switch (parameter.ToString())
               {
                   case SAVE_COMMAND:
                       HandleSave();
                       break;

                   case OPEN_COMMAND:
                       HandleOpen();
                       break;
               }
           }
        }

        private void HandleOpen()
        {
            Helpers.UrlEventArgs e = new Helpers.UrlEventArgs(ImageUrl);
            if (e.IsHttpOrHttps)
            {
                _Task.Uri = new Uri(e.Url);
                try { _Task.Show(); }
                catch (Exception) { }
            }
            else
                MessageBox.Show(
                    string.Format("I can't open this image due to the malforumed URL '{0}'.", ImageUrl),
                    ":(", 
                    MessageBoxButton.OK);
        }

        private void HandleSave()
        {
            var confirm = MessageBox.Show("Save image to pictures?", ":o", MessageBoxButton.OK);
            if (confirm == MessageBoxResult.Cancel)
                return;

            App.IsBusy = true;
            mediaSvc.SaveMediaToLibrary(ImageUrl, result =>
                {
                    if (result == Awful.Core.Models.ActionResult.Success)
                        MessageBox.Show("Image saved to Saved Pictures album.", ":)", MessageBoxButton.OK);
                    else
                        MessageBox.Show("Save failed.", ":(", MessageBoxButton.OK);
                    
                    App.IsBusy = false;
                });
        }

        public override bool CanExecute(object parameter)
        {
            if (parameter == null && ImageUrl == null) return false;
            return true; 
        }
    }

}
