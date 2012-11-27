using System;
using Telerik.Windows.Controls;

namespace Awful.SampleData
{
    public class SampleLoopingListSource : LoopingListDataSource
    {
        public SampleLoopingListSource()
            : base(10)
        {
            this.ItemNeeded += (sender, args) =>
                {
                    args.Item = new LoopingListDataItem(args.Index.ToString());
                };

            this.ItemUpdated += (sender, args) =>
                {
                    args.Item.Text = args.Index.ToString();
                };
        }
    }
}
