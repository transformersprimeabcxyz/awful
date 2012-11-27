using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Awful.Helpers;
using System.ComponentModel;
using System.Threading;
using Awful.Core.Web;

namespace Awful
{
    public partial class ShowBrowser : PhoneApplicationPage
    {
        private string html;
        private BackgroundWorker worker;
        private AutoResetEvent signal;
        private ProgressIndicator _progressIndicator;
        private WebGet web;

        public ShowBrowser()
        {
            InitializeComponent();
            Browser.Loaded += new RoutedEventHandler(OnBrowserLoaded);
            worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(OnBackgroundDoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(OnBackgroundWorkCompleted);
            worker.WorkerSupportsCancellation = true;
            signal = new AutoResetEvent(false);
        }

        void ContextLoaded()
        {
            _progressIndicator.IsIndeterminate = false;
        }
        void ContextLoading()
        {
            if (null == _progressIndicator)
            {
                _progressIndicator = new ProgressIndicator();
                _progressIndicator.IsVisible = true;
                SystemTray.ProgressIndicator = _progressIndicator;
            }

            _progressIndicator.IsIndeterminate = true;
        }

        void OnBackgroundWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Browser.NavigateToString(html);
            ContextLoaded();
        }

        void OnBackgroundDoWork(object sender, DoWorkEventArgs e)
        {
            var url = e.Argument as string;
            web = new WebGet();
            HtmlAgilityPack.HtmlDocument doc = null;
            web.LoadAsync(url, (obj, loadDocumentArgs) =>
            {
                doc = loadDocumentArgs.Document;
                signal.Set();
            });

            signal.WaitOne(10000);

            if (doc != null)
                html = doc.DocumentNode.OuterHtml;
            else
                e.Cancel = true;
        }

        void Cancel() 
        { 
            web.CancelAsync(); 
            worker.CancelAsync(); 
            signal.Set(); 
        }

        void OnBrowserLoaded(object sender, RoutedEventArgs e)
        {
           
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            string url = PhoneApplicationService.Current.State["BrowserRequest"] as string;
            if (url != null)
            {
                ContextLoading();
                worker.RunWorkerAsync(url);
            }
            
        }

        private void OnLoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {

        }

        private void OnNavigating(object sender, NavigatingEventArgs e)
        {

        }

        private void OnNavigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {

        }
    }
}
