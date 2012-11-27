using System;
using Telerik.Windows.Controls;

namespace Awful
{
    public class AwfulContextMenu : RadContextMenu
    {
        public AwfulContextMenu() : base() { }

        public new bool IsOpen
        {
            get { return base.IsOpen; }
            set
            {
                try { base.IsOpen = value; }
                catch (Exception ex) 
                {
                    string error = string.Format(
                        "An error occured when opening the menu. [{0}] {1}", 
                        ex.Message, ex.StackTrace);
                    Awful.Core.Event.Logger.AddEntry(error);
                }
            }
        }
    }

    public class AwfulContextMenuItem : RadContextMenuItem
    {
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (Command != null)
                Command.CanExecuteChanged += new EventHandler(Command_CanExecuteChanged);
        }

        void Command_CanExecuteChanged(object sender, EventArgs e)
        {
            if (Command.CanExecute(CommandParameter))
                IsEnabled = true;
            else
                IsEnabled = false;
        }

        protected override void OnCommandChanged(System.Windows.Input.ICommand newCommand, System.Windows.Input.ICommand oldCommand)
        {
            base.OnCommandChanged(newCommand, oldCommand);
            if (oldCommand != null)
                oldCommand.CanExecuteChanged -= new EventHandler(Command_CanExecuteChanged);

            if (newCommand != null)
                newCommand.CanExecuteChanged += new EventHandler(Command_CanExecuteChanged);
        }

        protected override void OnCommandParameterChanged(object newParam, object oldParam)
        {
            this.Command_CanExecuteChanged(this, null);
            base.OnCommandParameterChanged(newParam, oldParam);
        }
    }
}
