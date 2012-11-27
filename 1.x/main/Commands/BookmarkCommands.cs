using System;
using System.Windows;
using Awful.Models;
using KollaSoft;

namespace Awful.Commands
{
    public sealed class AddBookmarkCommand : CommandBase<ThreadData>
    {
        public override void Execute(object parameter)
        {
            if(CanExecute(parameter))
            {
                var thread = parameter as ThreadData;
                App.IsBusy = true;
                Services.SomethingAwfulThreadService.Current.AddBookmarkAsync(thread,
                    result =>
                    {
                        if (result == Awful.Core.Models.ActionResult.Success)
                            MessageBox.Show("Thread added to bookmarks.", ":)", MessageBoxButton.OK);
                        else
                            MessageBox.Show("Unable to add thread to bookmarks.", ":(", MessageBoxButton.OK);

                        App.IsBusy = false;
                    });
            }
        }
    }

    public sealed class RemoveBookmarkCommand : CommandBase<ThreadData>
    {
        public static event EventHandler BookmarkRemoved;

        public override void Execute(object parameter)
        {
            if (CanExecute(parameter))
            {
                var thread = parameter as ThreadData;
                var confirm = MessageBox.Show("Are you sure you want to remove this thread from bookmarks?", ":o", MessageBoxButton.OKCancel);
                if (confirm == MessageBoxResult.Cancel)
                    return;

                App.IsBusy = true;
                Services.SomethingAwfulThreadService.Current.RemoveBookmarkAsync(thread,
                    result =>
                    {
                        if (result == Awful.Core.Models.ActionResult.Success)
                        {
                            MessageBox.Show("Thread removed from bookmarks.", ":)", MessageBoxButton.OK);
                            BookmarkRemoved.Fire(this);
                        }

                        else
                            MessageBox.Show("Unable to remove thread from bookmarks.", ":(", MessageBoxButton.OK);

                        App.IsBusy = false;
                    });
            }
        }
    }

}
