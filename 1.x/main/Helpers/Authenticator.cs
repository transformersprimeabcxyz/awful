using System;
using System.Linq;
using System.Net;
using System.ComponentModel;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Windows;
using System.Collections.Generic;
using KollaSoft;
using Awful.Models;

namespace Awful.Helpers
{
    public class Authenticator : PropertyChangedBase
    {
        private string m_username;
        private string m_password;
        private bool m_isLoading;
        private string m_message;
        private BackgroundWorker worker;
        private AutoResetEvent signal;
        private bool success;

        private readonly string METHOD = "POST";
        private readonly string CONTENT_TYPE = "application/x-www-form-urlencoded";
        private readonly string HOST = "forums.somethingawful.com";
        private readonly string LOGIN_PAGE = "/account.php?";

        public static event EventHandler<ProfileChangedEventArgs> ProfileChanged;
        public static event EventHandler LoginSuccessful;

        public bool IsLoading
        {
            get { return m_isLoading; }
            set { m_isLoading = value; NotifyPropertyChangedAsync("IsLoading"); }
        }

        public string StatusMessage
        {
            get { return m_message; }
            set { m_message = value; NotifyPropertyChangedAsync("StatusMessage"); }
        }

        public string Username
        {
            get
            {
                return this.m_username;
            }

            set
            {
                this.m_username = value;
                NotifyPropertyChangedAsync("Username");
            }
        }

        public string Password
        {
            get 
            {
                return this.m_password;
            }

            set
            {
                this.m_password = value;
                NotifyPropertyChangedAsync("Password");
            }
        }

        public static CookieContainer AuthCookies;
        static Authenticator() { AuthCookies = new CookieContainer(); }

        public Authenticator()
        {
            worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;
            worker.WorkerReportsProgress = true;
            worker.DoWork += new DoWorkEventHandler(OnDoWork);
            worker.ProgressChanged += new ProgressChangedEventHandler(ShowProgress);
            signal = new AutoResetEvent(false);
        }

        public static CookieContainer LoadSavedCookies()
        {
            var settings = App.Settings;
            var cookiejar = settings.CookieJar;
            if(cookiejar.Count == 0)
                return null;

            var cookies = new CookieCollection();
            foreach (var cookie in cookiejar)
            {
                Cookie saCookie = new Cookie(cookie.Name, cookie.Value, "/", cookie.Domain)
                {
                    Expires = cookie.Expires,
                    HttpOnly = false,
                };
               
                cookies.Add(saCookie);
            }

            var container = new CookieContainer();
            container.Add(new Uri("http://forums.somethingawful.com"), cookies);
            return container;
        }

        public void LoginAsync(Action<Awful.Core.Models.ActionResult> result)
        {
            var user = this.Username;
            if (string.IsNullOrEmpty(user) == false && user.Equals("!!DEBUGUSER!!"))
            {
                this.m_username = Data.SAForumDB.DefaultProfile.Username;
                this.m_password = Data.SAForumDB.DefaultProfile.Password;
            }

            else 
            {
                using (var db = new Data.SAForumDB())
                {
                    var profile = db.Profiles.SingleOrDefault(p => p.ID == App.Settings.CurrentProfileID);
                    if (profile != null) 
                    {
                        this.Username = profile.Username;
                        this.Password = profile.Password;
                    }
                }
            }

            this.StartLoginAsync(result);
        }

        private void StartLoginAsync(Action<Awful.Core.Models.ActionResult> result)
        {
            if (worker.IsBusy)
            {
                result(Awful.Core.Models.ActionResult.Busy);
                return;
            }

            IsLoading = true;

            RunWorkerCompletedEventHandler handler = null;
            handler = (obj, args) => {
                
                worker.RunWorkerCompleted -= new RunWorkerCompletedEventHandler(handler);
                if (args.Cancelled)
                    result(Awful.Core.Models.ActionResult.Failure);
                else
                    result(Awful.Core.Models.ActionResult.Success);

                IsLoading = false;
            };

            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(handler);
            worker.RunWorkerAsync();
        }

        private void ShowProgress(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 0)
            {
                StatusMessage = e.UserState as string;
                signal.Set();
            }

            else
                MessageBox.Show(e.UserState as string);
        }

        private void OnDoWork(object sender, DoWorkEventArgs e)
        {
            this.success = false;

            string requestUrl = "http://" + HOST + LOGIN_PAGE;
            Awful.Core.Event.Logger.AddEntry(string.Format("Authenticator: attempting login request to '{0}'...", requestUrl));
            
            var request = HttpWebRequest.CreateHttp(requestUrl);
            AuthCookies = new CookieContainer();
            request.CookieContainer = AuthCookies;
            request.Method = METHOD;
            request.ContentType = CONTENT_TYPE;

            string status = "establishing connection...";
            Awful.Core.Event.Logger.AddEntry("Authenticator: " + status);

            worker.ReportProgress(0, status);
            signal.WaitOne(10000);

            status = "logging in...";
            Awful.Core.Event.Logger.AddEntry("Authenticator: " + status);
            
            worker.ReportProgress(0, status);
            signal.WaitOne(10000);
            
            request.BeginGetRequestStream(ProcessRequest, request);
            
            // we'll make this timeout pretty long.
            signal.WaitOne(AwfulSettings.THREAD_TIMEOUT_DEFAULT);
            
            if (worker.CancellationPending) 
            { 
                e.Cancel = true; 
                return; 
            }

            if (success)
            {
                status = "success! welcome to the forums.";
                LoginSuccessful.Fire(this);

                Awful.Core.Event.Logger.AddEntry("Authenticator: " + status);
                worker.ReportProgress(0, status);
                //ProfileChangedEventArgs args = new ProfileChangedEventArgs(this.Username, this.Password);
                //ProfileChanged.Fire(this, args);
                signal.WaitOne(10000);
            }

            else 
            { 
                e.Cancel = true; 
                return; 
            }

            Thread.Sleep(2000);
            if (worker.CancellationPending) { e.Cancel = true; }
        }

        private void ProcessRequest(IAsyncResult callback)
        {
            try
            {
                HttpWebRequest request = callback.AsyncState as HttpWebRequest;
                StreamWriter writer = new StreamWriter(request.EndGetRequestStream(callback));
                var postData = String.Format("action=login&username={0}&password={1}",
                    this.Username.Replace(" ", "+"),
                    HttpUtility.UrlEncode(this.Password));
                writer.Write(postData);
                writer.Close();
                request.BeginGetResponse(ProcessResponse, request);
            }
            catch (Exception ex) 
            {
                Awful.Core.Event.Logger.AddEntry(string.Format(
                    "Authenticator: An exception was thrown during the request process. [{0}] {1}",
                    ex.Message,
                    ex.StackTrace));

                worker.CancelAsync(); 
                success = false;
                signal.Set();
            }
        }

        private void ProcessResponse(IAsyncResult callback)
        {
            HttpWebRequest request = (HttpWebRequest)callback.AsyncState;
            try
            {
                HttpWebResponse response = request.EndGetResponse(callback) as HttpWebResponse;
                if (response.ResponseUri.AbsoluteUri != "http://forums.somethingawful.com/account.php?")
                {
                    // connecting to a wireless network without authenticating first will get you here...
                    worker.ReportProgress(0, String.Empty);
                    throw new Exception("Login failed. Check your internet connection settings and try again.");
                }

                if (response.Cookies.Count < 3)
                {
                    success = false;
                    signal.Set();
                }

                else
                {
                    AuthCookies = new CookieContainer();
                    AuthCookies.Add(new Uri("http://" + HOST), response.Cookies);
                 
                    success = true;

                    signal.Set();
                }
            }
            catch (Exception ex)
            {
                Awful.Core.Event.Logger.AddEntry(string.Format(
                    "Authenticator: There was an error while processing the response. [{0}] {1}",
                    ex.Message,
                    ex.StackTrace));

                success = false;
                worker.CancelAsync();
                signal.Set();
            }
        }
    }
}
