using System;
using System.Windows.Input;
using Awful.Models;
using Awful.Helpers;
using System.Windows;
using KollaSoft;


namespace Awful.Commands
{
    public class EditCommand : CommandBase
    {
        private readonly Services.SomethingAwfulThreadService _threadSvc = Services.SomethingAwfulThreadService.Current;
        private PostData m_Post;

        public event EventHandler<ActionResultEventArgs> RequestCompleted;

        public PostData Post
        {
            get { return m_Post; }
            set
            {
                if (m_Post == value) return;
                m_Post = value;
                NotifyCanExecuteChanged();
                NotifyPropertyChangedAsync("Post");
            }
        }

        public override bool CanExecute(object parameter)
        {
            if (m_Post == null) return false;

            if ((m_Post as SAPost).IsEditable == false)
                return false;

            if (parameter == null) return false;
            
            var text = parameter.ToString();
            if (String.IsNullOrEmpty(text)) return false;
            
            return true;
        }

        public override void Execute(object parameter)
        {
            if (CanExecute(parameter))
            {
                var text = parameter.ToString();
                var post = m_Post as SAPost;
                var id = post.ID.ToString();

                App.IsBusy = true;
                _threadSvc.SendEditPostTextAsync(id, text, result =>
                {
                    if (result == Awful.Core.Models.ActionResult.Success)
                        MessageBox.Show("Edit successful!", ":)", MessageBoxButton.OK);
                    else
                        MessageBox.Show("Edit failed.", ":(", MessageBoxButton.OK);

                    App.IsBusy = false;
                    RequestCompleted.Fire<ActionResultEventArgs>(this, new ActionResultEventArgs(result));
                });
            }
        }
    }
}
