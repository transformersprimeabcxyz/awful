using System;
using Awful.Services;
using Awful.Models;
using System.Windows;

namespace Awful.Commands
{
    public class ClearMarkedPostsCommand : CommandBase<ThreadData>
    {
        private readonly Services.SomethingAwfulThreadService _svc = Services.SomethingAwfulThreadService.Current;
        public override void Execute(object parameter)
        {
            if (CanExecute(parameter))
            {
                if (MessageBox.Show("Clicking OK will mark all posts in this thread as unseen. Do you still want to proceed?",
                  ":o", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    // Execute the action
                    var thread = parameter as ThreadData;
                    if (thread == null) throw new ArgumentException("Parmeter is not of type ThreadData.");

                    App.IsBusy = true;

                    _svc.ClearMarkedPostsAsync(thread, result =>
                        {
                            if (result == Awful.Core.Models.ActionResult.Success)
                                MessageBox.Show("Marks cleared!", ":)", MessageBoxButton.OK);
                            else
                                MessageBox.Show("There was an error processing your request. Please try again.",
                                    ":(",
                                    MessageBoxButton.OK);

                            App.IsBusy = false;
                        });
                }
            }
        }
    }
}
