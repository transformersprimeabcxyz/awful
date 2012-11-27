using System;
using System.Net;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using ImageTools.Controls;
using System.Windows.Media.Imaging;
using ImageTools;
using Awful.Models;
using System.Windows;
using System.ComponentModel;
using System.Windows.Media;
using System.Collections.Generic;

namespace Awful
{
    /*
    public class GroupToBrushValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ViewModels.ForumsInGroup group = value as ViewModels.ForumsInGroup;
            object result = null;

            if (group != null)
            {
                if (group.Count == 0)
                {
                    result = (SolidColorBrush)Application.Current.Resources["PhoneChromeBrush"];
                }
                else
                {
                    result = (SolidColorBrush)Application.Current.Resources["PhoneAccentBrush"];
                }
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    */

    public class ThreadViewStyleOptionFormatter : IValueConverter
    {
        private const string _HORIZONTAL = "Horizontal";
        private const string _VERTICAL = "Vertical (Recommended)";

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is AwfulSettings.ViewStyle))
                throw new ArgumentException(string.Format("ThreadViewStyleOptionFormatter: Expected ViewStyle, not {0}.", value.GetType()));

            AwfulSettings.ViewStyle style = (AwfulSettings.ViewStyle)value;
            string result = null;

            switch (style)
            {
                case AwfulSettings.ViewStyle.Horizontal:
                    result = _HORIZONTAL;
                    break;

                case AwfulSettings.ViewStyle.Vertical:
                    result = _VERTICAL;
                    break;
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is string))
                throw new ArgumentException(string.Format("ThreadViewStyleOptionFormatter: Expected ViewStyle, not {0}.", value.GetType()));

            string selected = value as string;
            AwfulSettings.ViewStyle result = AwfulSettings.ViewStyle.Vertical;
            switch (selected)
            {
                case _HORIZONTAL:
                    result = AwfulSettings.ViewStyle.Horizontal;
                    break;

                case _VERTICAL:
                    result = AwfulSettings.ViewStyle.Vertical;
                    break;
            }

            return result;
        }
    }

    public class AwfulImageConverter : IValueConverter
    {
      
        private ImageConverter _gifConverter;
        public AwfulImageConverter()
        {
            _gifConverter = new ImageConverter();
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var uri = value as Uri;
            if (uri == null)
            {
                if (value is string)
                {
                    uri = new Uri(value as string, UriKind.RelativeOrAbsolute);
                }

                else
                {
                    return value;
                }
            }

            string uriString;
            try { uriString = uri.AbsolutePath; }
            catch (InvalidOperationException) { uriString = uri.OriginalString; }

            string extension = uriString.Split('.').Last();
            if (extension.Equals("gif", StringComparison.OrdinalIgnoreCase))
            {
                ExtendedImage gifImage = null;
                try
                {
                    gifImage = _gifConverter.Convert(uri, typeof(object), null, null) as ExtendedImage;
                }
                catch (ImageTools.IO.UnsupportedImageFormatException e)
                {
                    MessageBox.Show(e.InnerException.Message);
                }

                var aniImage = new AnimatedImage() { Source = gifImage, Stretch = Stretch.Fill };
                aniImage.Unloaded += new RoutedEventHandler(OnAnimatedImageUnloaded);
                return aniImage;
            }

            else
            {
                var source = new BitmapImage(uri);
                var img = new Image() { Source = source, Stretch = Stretch.Fill };
                img.Unloaded += new RoutedEventHandler(OnImageUnloaded);
                return img;
            }
        }

        private void OnAnimatedImageUnloaded(object sender, RoutedEventArgs e)
        {
            AnimatedImage image = sender as AnimatedImage;
            image.Unloaded -= new RoutedEventHandler(OnAnimatedImageUnloaded);

            var ext = image.Source as ExtendedImage;
            ext.UriSource = null;
            image.Source = null;
        }

        private void OnImageUnloaded(object sender, RoutedEventArgs e)
        {
            Image image = sender as Image;
            image.Unloaded -= new RoutedEventHandler(OnImageUnloaded);

            var bitmap = image.Source as BitmapImage;
            bitmap.UriSource = null;
            image.Source = null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SAThreadFormatter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            object result = String.Empty;
            if (value == null) return string.Empty;

            switch (parameter.ToString())
            {
                case "COUNT":
                    result = FormatNewPostCount(value as SAThread);
                    break;

                case "VIEWED":
                    result = FormatViewedThread((bool)value);
                    break;

                default:
                    throw new ArgumentException("Incorrect parameter '"
                    + parameter.ToString() + "' passed to thread formatter.");
            }

            return result;
        }

        private double FormatViewedThread(bool hasViewedToday)
        {
            if (hasViewedToday) { return 0.4; }

            return 1.0;
        }

        private string FormatNewPostCount(SAThread thread)
        {
            int value = thread.NewPostCount;

            if (value == Int32.MaxValue)
            {
                thread.ShowPostCount = true;
                return "no";
            }

            if (value == -1)
            {
                thread.ShowPostCount = false;
                return String.Empty;
            }

            thread.ShowPostCount = true;
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

   

    public class SAForumManager : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            object result = null;

            switch (parameter.ToString())
            {
                case "HEADER":
                    bool isFavorite = (bool)value;
                    if (isFavorite)
                        result = "remove from favorites";
                    else
                        result = "add to favorites";
                    break;
                        
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SAPostManager : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            object result = null;
            SAPost post = null;
            switch (parameter.ToString())
            {
                case "Foreground":
                    post = value as SAPost;
                    result = HandleForeground(post);
                    break;

                case "Background":
                    bool hasSeen = (bool)value;
                    result = HandleBackground(hasSeen);
                    break;

                case "ADMIN":
                    post = value as SAPost;
                    result = HandleAccountType(post);
                    break;
            }

            return result;
        }

        private object HandleAccountType(SAPost post)
        {
            var colorString = ((Color)App.Current.Resources["PhoneForegroundColor"]).ToString();
            
            if (post == null)
                return colorString;

            switch (post.AccountType)
            {
                case AccountType.Moderator:
                    colorString = "gold";
                    break;

                case AccountType.Admin:
                    colorString = "red";
                    break;
            }

            return colorString;
        }

        private Brush HandleBackground(bool hasSeen)
        {
            if (hasSeen)
                return new SolidColorBrush() { Color = App.Layout.CurrentTheme.PostHasSeen };

            return new SolidColorBrush() { Color = (Color)App.Current.Resources["PhoneChromeColor"] };
        }

        private Brush HandleForeground(SAPost post)
        {
            if (post != null && post.HasSeen)
                return new SolidColorBrush() { Color = App.Layout.CurrentTheme.PostHasSeen };

            return new SolidColorBrush() { Color = App.Layout.CurrentTheme.PostForeground };
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
