using System;
using System.ComponentModel;
using System.Windows.Media;
using KollaSoft;

namespace Awful
{
    public class Theme : PropertyChangedBase
    {
        private string _tag;
        private Color _background;
        private Color _foreground;
        private Color _postForeground;
        private Color _postBackground;
        private Color _userForeground;
        private Color _userBackground;
        private Color _userPostDate;
        private Color _postHasSeen;

        public string Tag
        {
            get { return _tag; }
            set { _tag = value; NotifyPropertyChangedAsync("Tag"); }
        }
        public Color Background
        {
            get { return _background; }
            set 
            { 
                if(_background.Equals(value)) return;

                _background = value; 
                NotifyPropertyChangedAsync("Background"); 
            }
        }

        public Color PostHasSeen
        {
            get { return _postHasSeen; }
            set
            {
                if (value != _postHasSeen)
                {
                    _postHasSeen = value;
                    NotifyPropertyChangedAsync("PostHasSeen");
                }
            }
        }
        public Color Foreground
        {
            get { return _foreground; }
            set
            {
                if (_foreground.Equals(value)) return;

                _foreground = value;
                NotifyPropertyChangedAsync("Foreground");
            }
        }
        public Color PostForeground
        {
            get { return _postForeground; }
            set
            {
                if (_postForeground.Equals(value)) return;

                _postForeground = value;
                NotifyPropertyChangedAsync("PostForeground");
            }
        }
        public Color PostBackground
        {
            get { return _postBackground; }
            set
            {
                if (_postBackground.Equals(value)) return;

                _postBackground = value;
                NotifyPropertyChangedAsync("PostBackground");
            }
        }
        public Color UserForeground
        {
            get { return _userForeground; }
            set
            {
                if (_userForeground.Equals(value)) return;

                _userForeground = value;
                NotifyPropertyChangedAsync("UserForeground");
            }
        }
        public Color UserBackground
        {
            get { return _userBackground; }
            set
            {
                if (_userBackground.Equals(value)) return;

                _userBackground = value;
                NotifyPropertyChangedAsync("UserBackground");
            }
        }
        public Color UserPostDate
        {
            get { return _userPostDate; }
            set
            {
                if (_userPostDate.Equals(value)) return;

                _userPostDate = value;
                NotifyPropertyChangedAsync("_userPostDate");
            }
        }

        public override string ToString()
        {
            return _tag;
        }
    }

    public sealed class LayoutManager : PropertyChangedBase
    {
        private Theme _currentTheme;
        public Theme CurrentTheme
        {
            get { return _currentTheme; }
            set { _currentTheme = value; NotifyPropertyChangedAsync("CurrentTheme"); }
        }
    }
}
