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
using System.Collections.Generic;
using Awful.Models;

namespace Awful.SampleData
{
    public class SampleSmileyData : List<AwfulSmiley>
    {
        public SampleSmileyData()
            : base()
        {
            for(int i = 0; i < 20; i++)
            {
                this.Add(new AwfulSmiley() { Text = ":sample:", Uri = "icons/appbar.close.rest.png" });
            }
        }

        public List<AwfulSmiley> Page { get { return this; } }
    }

    public class SampleSmileyPages
    {
        private List<SampleSmileyData> pages = new List<SampleSmileyData>();
        public List<SampleSmileyData> Pages { get { return this.pages; } }

        public SampleSmileyPages()
        {
            pages.Add(new SampleSmileyData());
            pages.Add(new SampleSmileyData());
        }
    }
}
