using System;
using System.Windows.Input;
using Microsoft.Phone.Controls;
using Awful.Models;
using Microsoft.Phone.Shell;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using KollaSoft;

namespace Awful.Commands
{
    public class ViewThreadCommand : CommandBase
    {
        public string Url { get; set; }
        public object ThreadID { get; set; }
        
        public const string USE_URL = "URL";
        public const string USE_THREAD_ID = "THREADID";
        public const string USE_URL_OR_THREADID = "URLORTHREADID";

        public event EventHandler Navigating;

        public override bool CanExecute(object parameter)
        {
            if (parameter == null) return false;
            bool result = true;

            switch (parameter.ToString())
            {
                case USE_URL:
                    result = string.IsNullOrEmpty(Url) == false;
                    break;

                case USE_THREAD_ID:
                    result = ThreadID != null;
                    break;

                case USE_URL_OR_THREADID:
                    result = string.IsNullOrEmpty(Url) == false;
                    result = result || ThreadID != null;
                    break;
            }

            return result;
        }

        public ThreadData GenerateThreadData(object parameter)
        {
            SAThread thread = null;

            switch (parameter.ToString())
            {
                case USE_URL:
                    thread = CreateThreadDataFromUrl(Url);
                    break;

                case USE_THREAD_ID:
                    try
                    {
                        int threadID = int.Parse(this.ThreadID.ToString());
                        thread = new SAThread(null) { ID = threadID };
                    }
                    catch (Exception) { }
                    break;

                case USE_URL_OR_THREADID:
                    int id = -1;
                    if (int.TryParse(ThreadID.ToString(), out id))
                    {
                        thread = new SAThread(null) { ID = id };
                    }
                    else
                    {
                        thread = CreateThreadDataFromUrl(this.Url);
                    }
                    break;
                   
            }

            if (thread == null && parameter.ToString().Equals(USE_URL))
            {
                MessageBox.Show(string.Format("Could not parse the target '{0}'.", Url), ":(", 
                    MessageBoxButton.OK);
            }

            return thread;
        }

        public override void Execute(object parameter)
        {
            if (CanExecute(parameter))
            {
                var thread = GenerateThreadData(parameter);
                if (thread != null)
                {
                    int page = thread.LastViewedPageIndex;
                    Navigating.Fire(this);
                    NavigateToThread(thread, page);
                }   
            }
        }

        private SAThread CreateThreadDataFromUrl(string url)
        {
            SAThread thread = null;
            try 
            { 
                // parse the url
                KollaSoft.UrlParser parser = new KollaSoft.UrlParser(url);
                
                // check to see if url contains the something awful forums base
                if (!parser.Base.Contains("forums.somethingawful.com/showthread.php")) throw new Exception();

                thread = new SAThread(null);
                thread.ThreadURL = url;

                // pull thread id value: if not present throw exception (can't navigate to an id-less thread!)
                if (parser.Query.ContainsKey("threadid"))
                {
                    int threadID = int.Parse(parser.Query["threadid"]);
                    thread.ID = threadID;
                }

                else throw new Exception();

                // check to see if there is a page number there, if not then load first page
                thread.LastViewedPageIndex = parser.Query.ContainsKey("pagenumber") ?
                    Int32.Parse(parser.Query["pagenumber"]) : 1;

                // check to see if a postID is specified
                string postID = parser.Hash;

                if (!string.IsNullOrEmpty(postID) && postID.Contains("post"))
                {
                    // remove 'post' from hash (i.e. 'post30340507' -> '33040507'
                    postID = postID.Replace("post", "");

                    // parse into integer
                    thread.LastViewedPostID = int.Parse(postID);
                }
            }
            
            catch (Exception)
            {
                thread = null;
            }

            return thread;
        }

        private void NavigateToThread(ThreadData thread, int page)
        {
            if (thread == null)
            {
                MessageBox.Show("Could not locate valid thread.", ":(", MessageBoxButton.OK);
                return;
            }

            PhoneApplicationService.Current.State["Thread"] = thread;
            var frame = App.Current.RootVisual as PhoneApplicationFrame;
            if (frame != null)
            {
                string uri = App.Settings.ThreadViewStyle == AwfulSettings.ViewStyle.Horizontal ?
                    "/ViewThread.xaml" :
                    "/ThreadView.xaml";

                string pageQuery = page > 0 ? "?Page=" + page : string.Empty;
                
                frame.Navigate(new Uri(uri + pageQuery, UriKind.RelativeOrAbsolute));
            }
        }
    }
}
