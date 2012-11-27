using System;
using System.ComponentModel;
using System.Collections.Generic;
using KollaSoft;

namespace Awful.Core.Models
{
    public class RatingsSource
    {
        private readonly List<RatingsItem> m_ratings = new List<RatingsItem>();
        public List<RatingsItem> Ratings
        {
            get { return m_ratings; }
        }
    }

    public class RatingsItem : PropertyChangedBase
    {
        private string m_color;
        private object m_name;

        public object Value
        {
            get {return m_name; }
            set
            {
                m_name = value;
                NotifyPropertyChangedAsync("Value");
            }
        }

        public string Color
        {
            get { return m_color; }
            set
            {
                if (m_color == value) return;
                m_color = value;
                NotifyPropertyChangedAsync("Color");
            }
        }
    }
}
