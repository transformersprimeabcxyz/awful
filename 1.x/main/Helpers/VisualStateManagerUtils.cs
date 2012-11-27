using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Awful.Helpers
{
    public static class VisualStateManagerUtils
    {
        public static Storyboard FindStoryboard(this FrameworkElement parent, string groupName, string stateName)
        {
            var vsgs = VisualStateManager.GetVisualStateGroups(parent);
            foreach(VisualStateGroup vsg in vsgs)
            {
                if(vsg.Name != groupName)
                    continue;
                foreach (VisualState vs in vsg.States)
                {
                    if(vs.Name == stateName)
                        return vs.Storyboard;
                }
            }
            return null;
        }

        public static void OnVisualStateFinish(this Control control, string name, Storyboard vsStoryboard, Action action)
        {
            EventHandler handler = null;
            handler = (obj, args) =>
            {
                vsStoryboard.Completed -= new EventHandler(handler);
                action();
            };
            vsStoryboard.Completed += new EventHandler(handler);
            VisualStateManager.GoToState(control, name, true);
        }
    }
}
