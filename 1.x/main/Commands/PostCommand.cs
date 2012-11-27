using System.Windows.Input;
using Awful.Models;
using Awful.Helpers;
using Awful;
using System;
using System.Windows;
using KollaSoft;

namespace Awful.Commands
{
    public class PostCommand : CommandBase
    {
        public const string MARK = "MARK";
        public const string EDIT = "EDIT";
        public const string QUOTE = "QUOTE";

        private SAPost m_CurrentPost;
        private readonly Services.ThreadReplyService replySvc = new Services.ThreadReplyService();
        private readonly Services.SomethingAwfulThreadService threadSvc = Services.SomethingAwfulThreadService.Current;

        public event EventHandler ContentLoading;
        public event EventHandler<WebContentLoadedEventArgs> ContentLoaded;
   
        public SAPost CurrentPost
        {
            get { return m_CurrentPost; }
            set
            {
                if (m_CurrentPost == value) return;
                m_CurrentPost = value;
                NotifyPropertyChangedAsync("CurrentPost");
                NotifyCanExecuteChanged();
            }
        }

        public override bool CanExecute(object parameter)
        {
            if (CurrentPost == null) return false;
            if (parameter == null) return false;

            var command = parameter.ToString();
            bool result = false;

            switch (command)
            {
                case EDIT:
                    result = CurrentPost.IsEditable;
                    break;

                case QUOTE:
                    result = true;
                    break;

                case MARK:
                    result = true;
                    break;
            }

            return result;
        }

        public override void Execute(object parameter)
        {
            if (CanExecute(parameter))
            {
                switch (parameter.ToString())
                {
                    case EDIT:
                        ContentLoading.Fire(this);
                        HandleEdit(CurrentPost);
                        break;

                    case QUOTE:
                        ContentLoading.Fire(this);
                        HandleQuote(CurrentPost);
                        break;

                    case MARK:
                        HandleMark(CurrentPost);
                        break;
                }
            }
        }

        private void HandleMark(SAPost post)
        {
            var confirm = MessageBox.Show("Mark this post as last read?", ":o", MessageBoxButton.OKCancel);
            if (confirm == MessageBoxResult.Cancel)
                return;

            App.IsBusy = true;
            threadSvc.MarkThreadToPostAsync(post, result =>
            {
                if (result == Awful.Core.Models.ActionResult.Success)
                    MessageBox.Show("Mark successful.", ":)", MessageBoxButton.OK);
                else
                    MessageBox.Show("Mark failed.", ":(", MessageBoxButton.OK);

                App.IsBusy = false;
            });
        }

        private void HandleQuote(SAPost post)
        {
            App.IsBusy = true;
            string id = post.ID.ToString();
            replySvc.GetQuote(id, (result, text) =>
            {
                ContentLoaded.Fire<WebContentLoadedEventArgs>(this, new WebContentLoadedEventArgs(RequestType.Quote, text));
                App.IsBusy = false;
            });
        }

        private void HandleEdit(SAPost post)
        {
            App.IsBusy = true;
            string id = post.ID.ToString();
            replySvc.GetEdit(id, (result, text) =>
            {
                ContentLoaded.Fire<WebContentLoadedEventArgs>(this, new WebContentLoadedEventArgs(RequestType.Edit, text));
                App.IsBusy = false;
            });
        }
    }
}
