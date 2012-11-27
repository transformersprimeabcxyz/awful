using System;
using System.Windows.Input;
using Microsoft.Phone.Tasks;

namespace Awful.Commands
{
    public class EmailCommand : CommandBase
    {
        private readonly EmailComposeTask email = new EmailComposeTask();
        public string To { get; set; }
        public string Body { get; set; }
        public string Subject { get; set; }

        public override bool CanExecute(object parameter)
        {
            return true;
        }

        public override void Execute(object parameter)
        {
            try
            {
                email.Subject = Subject;
                email.Body = Body;
                email.To = To;
                email.Show();
            }
            catch (Exception) { }
        }
    }
}
