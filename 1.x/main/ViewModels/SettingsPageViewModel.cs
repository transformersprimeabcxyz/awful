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
using System.ComponentModel;
using System.Collections.Generic;

namespace Awful.ViewModels
{
    public class SettingsPageViewModel : INotifyPropertyChanged
    {
        public static SettingsPageViewModel Default { get; private set; }
        static SettingsPageViewModel() { Default = new SettingsPageViewModel(); }

        public List<Theme> Themes
        {
            get;
            set;
        }

        private Theme _selectedTheme;
        public Theme SelectedTheme
        {
            get { return _selectedTheme; }
            set
            {
                if (value == null) return;
                _selectedTheme = value;
                App.Layout.CurrentTheme = value;
                NotifyPropertyChanged("SelectedTheme");
            }
        }

        private SettingsPageViewModel()
        {
            Themes = new List<Theme>()
            {
                App.Current.Resources["Classic"] as Theme,
                App.Current.Resources["Focus"] as Theme
            };

            _selectedTheme = Themes[0];
        }
        
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        #endregion
    }
}
