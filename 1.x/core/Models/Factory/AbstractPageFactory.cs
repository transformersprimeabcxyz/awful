using System;
using Awful.Core.Models.Factory.Interfaces;
using System.ComponentModel;
using HtmlAgilityPack;
using System.Threading;

namespace Awful.Core.Models.Factory
{
    public abstract class AbstractPageFactory<TPage> : PageFactory<TPage>
        where TPage : Models.Interfaces.WebBrowseable
    {
        private HtmlNode FetchHtmlFromWeb(string url)
        {
            var saWebPageFetch = new Web.WebGet();
            var signal = new AutoResetEvent(false);
            HtmlNode node = null;

            saWebPageFetch.LoadAsync(url, (result, args) =>
                {
                    node = this.HandleWebGetResult(result, args);
                    signal.Set();
                });

            signal.WaitOne();
            return node;
        }

        private HtmlNode HandleWebGetResult(ActionResult result, Web.WebGetDocumentArgs args)
        {
            HtmlNode node = null;
            switch (result)
            {
                case ActionResult.Success:
                    node = args.Document.DocumentNode;
                    break;
            }

            return node;
        }

        protected abstract TPage BuildItem(HtmlNode node, TPage page);
        public void BuildPage(TPage page, Action<TPage> result)
        {
            ThreadPool.QueueUserWorkItem(state =>
                {
                    try
                    {
                        string url = page.Url;
                        var node = this.FetchHtmlFromWeb(url);
                        page = this.BuildItem(node, page);
                    }
                    catch (Exception) { }
                    finally { result(page); }
                    
                }, null);
        }
    }
}
