using System;
using Microsoft.Phone.Controls;
using KollaSoft;
using System.Net;
using Awful.Core.Models;
using System.Text;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using Awful.Core.Event;
using System.IO;

namespace Awful.Core.Web
{
    public class AwfulAuthenticator : PropertyChangedBase
    {
        private readonly WebBrowser browser = new WebBrowser();
        private List<Cookie> _cookies;
        private CookieContainer _container;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _status;
        private bool _isLoading;
        private LoginStates _currentState;

        private const string READY_URL = "http://forums.somethingawful.com/account.php?action=loginform";
        private const string SUCCESS_URL = "http://forums.somethingawful.com/index.php";
        private const string USER_CP_URL = "http://forums.somethingawful.com/usercp.php";

        // this is necessary because of the stupid domain bug in silverlight
        private const string COOKIE_DOMAIN_URL = "http://fake.forums.somethingawful.com";
        private const int MAX_LOGIN_ATTEMPTS = 5;
        public enum LoginResult
        {
            CANNOT_REACH_LOGIN_PAGE,
            LOGIN_FAILED,
            ACCOUNT_LOCKED,
            LOGIN_SUCCESSFUL,
            GRABBING_COOKIES
        }

        private enum LoginStates 
        { 
            READY, 
            AUTHENTICATING, 
            SUCCESS, 
            FAILED,
            SUBMITTING_MANUALLY,
            AUTO_LOGIN,
            NON_BROWSER_LOGIN,
            PARSING_INDEX
        };

        public string Username
        {
            get { return this._username; }
            set 
            { 
                this._username = value;
                this.NotifyPropertyChangedAsync("Username");
            }
        }

        public string Password
        {
            get { return this._password; }
            set
            {
                this._password = value;
                this.NotifyPropertyChangedAsync("Password");
            }
        }

        public WebBrowser LoginBrowser { get { return browser; } }

        public bool IsLoading
        {
            get { return this._isLoading; }
            private set
            {
                this._isLoading = value;
                this.NotifyPropertyChangedAsync("IsLoading");
            }
        }

        public string Status
        {
            get { return this._status; }
            set
            {
                this._status = value;
                this.NotifyPropertyChangedAsync("Status");
            }
        }

        public CookieContainer CookieJar
        {
            get { return this._container; }
        }

        public List<Cookie> AuthenticationCookies 
        { 
            get 
            {
                return this._cookies;
               
            }
            private set
            {
                this._cookies = value;
            }
        }

        public event EventHandler<ValueChangedEventArgs<LoginResult>> Result;
        public static event EventHandler<ProfileChangedEventArgs> LoginSuccessful;

        public AwfulAuthenticator()
        {
            browser.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            browser.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            browser.NavigationFailed += new System.Windows.Navigation.NavigationFailedEventHandler(OnBrowserNavigationFailed);
            browser.Navigating += new EventHandler<NavigatingEventArgs>(OnBrowserNavigating);
            browser.Navigated += new EventHandler<System.Windows.Navigation.NavigationEventArgs>(OnBrowserNavigated);
            browser.IsScriptEnabled = true;
            browser.ScriptNotify += new EventHandler<NotifyEventArgs>(OnBrowserScriptNotify);
            this._currentState = LoginStates.READY;
            this._status = string.Empty;
            this.IsLoading = false;
        }

        public void LoginAsync()
        {
            this.Status = string.Empty;
            this._currentState = LoginStates.NON_BROWSER_LOGIN;
            switch (this._currentState)
            {
                // if we aren't logged in and just getting started
                case LoginStates.READY:
                    this.IsLoading = true;
                    browser.Navigate(new Uri(READY_URL));
                    break;

                // if we failed earlier, we can assume we're on a page where we can login
                case LoginStates.FAILED:
                    this.RunLoginScript();
                    break;

                case LoginStates.NON_BROWSER_LOGIN:
                    this.NonBrowserLoginAsync();
                    break;
            }
        }

        public void ManualLoginAsync()
        {
            this.Status = string.Empty;
            this._currentState = LoginStates.AUTO_LOGIN;

            switch(this._currentState)
            {
                case LoginStates.SUBMITTING_MANUALLY:
                    browser.Navigate(new Uri(READY_URL));
                    break;
           
                case LoginStates.AUTO_LOGIN:
                    this.NonBrowserLoginAsync();
                    break;
            }
        }

        #region Login via HttpWebRequest

        public void NonBrowserLoginAsync()
        {
            this.IsLoading = true;
            this.Status = "contacting forum servers...";
            var request = AwfulWebRequest.CreatePostRequest("http://forums.somethingawful.com/account.php?");
            request.BeginGetRequestStream(callback => ProcessNonBrowserLoginRequest(callback,
                success =>
                {
                    if (success)
                    {
                        this.Status = "login successful!";
                        Logger.AddEntry(this.Status);
                        this._currentState = LoginStates.SUCCESS;
                        this.Result.Fire(this, new ValueChangedEventArgs<LoginResult>(LoginResult.LOGIN_SUCCESSFUL));
                        var profile = new AwfulProfile() { Username = this.Username, Password = this.Password };
                        LoginSuccessful.Fire(this, new ProfileChangedEventArgs(profile, this.AuthenticationCookies));
                    }

                    else { this.ReportLoginFailed(); }
                    this.IsLoading = false;
                }),
                request);
        }

        private void ProcessNonBrowserLoginRequest(IAsyncResult callback, Action<bool> predicate)
        {
            try
            {
                this.Status = "logging in...";
                Thread.Sleep(1000);
                HttpWebRequest request = callback.AsyncState as HttpWebRequest;
                StreamWriter writer = new StreamWriter(request.EndGetRequestStream(callback));
                var postData = String.Format("action=login&username={0}&password={1}",
                    this.Username.Replace(" ", "+"),
                    HttpUtility.UrlEncode(this.Password));
                writer.Write(postData);
                writer.Close();
                request.BeginGetResponse(response => ProcessNonBrowserLoginResponse(response, predicate), 
                    request);
            }
            catch (Exception ex)
            {
                Logger.AddEntry(string.Format(
                    "Authenticator: An exception was thrown during the request process. [{0}] {1}",
                    ex.Message,
                    ex.StackTrace));

                predicate(false);
            }
        }

        private void ProcessNonBrowserLoginResponse(IAsyncResult callback, Action<bool> predicate)
        {
            HttpWebRequest request = (HttpWebRequest)callback.AsyncState;
            try
            {
                HttpWebResponse response = request.EndGetResponse(callback) as HttpWebResponse;
                if (response.ResponseUri.AbsoluteUri != "http://forums.somethingawful.com/account.php?")
                {
                    // connecting to a wireless network without authenticating first will get you here
                    throw new Exception("Login failed. Check your internet connection settings and try again.");
                }

                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    string html = reader.ReadToEnd();
                    Logger.AddEntry(html);
                }

                if (request.CookieContainer.Count == 0)
                {
                    throw new Exception("Cookie count less than expected. Please try again.");
                }

                else
                {
                    var collection = request.CookieContainer.GetCookies(new Uri(COOKIE_DOMAIN_URL));
                    this.ManageCookies(collection);
                    new Action(() => { predicate(true); }).InvokeOnUIThread();
                }
            }
            catch (Exception ex)
            {
                Logger.AddEntry(string.Format(
                    "Authenticator: There was an error while processing the response. [{0}] {1}",
                    ex.Message,
                    ex.StackTrace));

                new Action(() => { predicate(false); }).InvokeOnUIThread();
            }
        }

        #endregion

        private void OnBrowserScriptNotify(object sender, NotifyEventArgs e)
        {
            // do nothing for now //
        }

        private void OnBrowserNavigationFailed(object sender, System.Windows.Navigation.NavigationFailedEventArgs e)
        {
           this.IsLoading = false;
           this.ReportLoginPageInaccessible();
        }

        private void OnBrowserNavigating(object sender, NavigatingEventArgs e)
        {
            this.IsLoading = true;
            switch (this._currentState)
            {
                case LoginStates.READY:
                    this.Status = "Connecting to authentication server...";
                    break;
            }
        }

        private void OnBrowserNavigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            var url = e.Uri.AbsoluteUri;
            
            // scrape html for username
            if (this._currentState.Equals(LoginStates.SUBMITTING_MANUALLY) &&
                url.Equals(SUCCESS_URL))
            {
                try
                {
                    this.Result.Fire(this, new ValueChangedEventArgs<LoginResult>(LoginResult.GRABBING_COOKIES));
                    new Action(this.ReportLoginSuccess).DelayThenInvoke(new TimeSpan(0, 0, 10)); 
                }
                catch (Exception ex) 
                { 
                    Logger.AddEntry("An error occurred while logging in:", ex);
                    this.ReportLoginFailed();
                }
            }

            else
            {
                switch (this._currentState)
                {
                    case LoginStates.READY:
                        if (url == READY_URL)
                        {
                            this.Status = "Connected. Logging in...";
                            this.RunLoginScript();
                        }
                        break;

                    case LoginStates.AUTHENTICATING:
                        if (url.Contains("loginerror")) { this.ReportLoginFailed(); }
                        else if (url.Contains("index.php")) 
                        {
                            this.Result.Fire(this, new ValueChangedEventArgs<LoginResult>(LoginResult.GRABBING_COOKIES));
                            new Action(this.ReportLoginSuccess).DelayThenInvoke(new TimeSpan(0, 0, 10)); 
                        }
                        break;
                }
            }
        }

        public List<Cookie> ManageCookies(CookieCollection collection)
        {
            var cookies = collection;
            List<Cookie> cookieList = new List<Cookie>(cookies.Count);
            this._container = new CookieContainer();
            foreach (Cookie cookie in cookies)
            {
                var awfulCookie = new Cookie(
                    cookie.Name,
                    cookie.Value,
                    "/",
                    ".somethingawful.com");
                cookieList.Add(awfulCookie);
            }

            this.AuthenticationCookies = cookieList;
            AwfulWebRequest.SetCookieJar(cookieList);
            return cookieList;
        }

        private void ReportLoginSuccess()
        {
            this.Status = "Success! Welcome to the forums.";
            this.IsLoading = false;
            List<Cookie> cookies = null;
            bool cookiesGrabbed = false;
            int attempts = 0;
            while (!cookiesGrabbed && attempts < MAX_LOGIN_ATTEMPTS)
            {
                // try to get the cookies from the browser. browser.GetCookies() throws an
                // index out of range exception often...
                try
                {
                    
                    cookies = this.ManageCookies(browser.GetCookies());
                    cookiesGrabbed = true;
                }
                catch (Exception ex) 
                { 
                    Logger.AddEntry("An error occurred: ", ex);
                    attempts = attempts + 1;
                }
            }

            //  try to parse username from index //
            var web = new WebGet();
            web.LoadAsync(SUCCESS_URL, (a, args) =>
                {
                    if (a == ActionResult.Success)
                    {
                        string username = AwfulIndexParser.ParseUserSessionName(args.Document);
                        this.Username = username;
                        this._currentState = LoginStates.SUCCESS;
                        this.Result.Fire(this, new ValueChangedEventArgs<LoginResult>(LoginResult.LOGIN_SUCCESSFUL));
                        var profile = new AwfulProfile() { Username = this.Username, Password = this.Password };
                        LoginSuccessful.Fire(this, new ProfileChangedEventArgs(profile, cookies));
                    }
                });

            this.Status = "Grabbing username...";
        }

        private void ReportLoginFailed()
        {
            this.IsLoading = false;
            this.Status = "Login failed.";
            this._currentState = LoginStates.FAILED;
            this.Result.Fire(this, new ValueChangedEventArgs<LoginResult>(LoginResult.LOGIN_FAILED));
        }

        private void ReportLoginPageInaccessible()
        {
            this.IsLoading = false;
            this.Status = string.Empty;
            this._currentState = LoginStates.READY;
            this.Result.Fire(this, new ValueChangedEventArgs<LoginResult>(LoginResult.CANNOT_REACH_LOGIN_PAGE));
        }

        private void FocusUsername()
        {
            string script = "javascript:{ try { $('input[name=username]').focus() } catch(e) { } }";
            browser.Navigate(new Uri(script));
        }

        private void RunLoginScript()
        {
            this.IsLoading = true;
           
            Action failAfterTimeout = () =>
                {
                    if (this._currentState == LoginStates.AUTHENTICATING)
                    {
                        this._currentState = LoginStates.FAILED;
                        this.ReportLoginPageInaccessible();
                    }
                };

           
            try 
            {
                new Action(this.AddUsernameAndPassword).DelayThenInvoke(new TimeSpan(0, 0, 3));
            }
            catch (Exception ex)
            {
                Logger.AddEntry(ex);
                this.ReportLoginFailed();
                //new Action(this.AddUsernameAndPassword).DelayThenInvoke(new TimeSpan(0, 0, 1));
            }
        }

        private void AddUsernameAndPassword()
        {
            // use javascript to fill in username and password input fields, then submit
            string usernameScript = string.Format("$('input[name=\"username\"]').val('{0}')", this.Username);
            string passwordScript = string.Format("$('input[name=\"password\"]').val('{0}')", this.Password);

            Logger.AddEntry(usernameScript);
            Logger.AddEntry(passwordScript);

            // run script
            this._currentState = LoginStates.AUTHENTICATING;
            browser.InvokeScript("eval", usernameScript);
            browser.InvokeScript("eval", passwordScript);
            this.SubmitAndProcess();
        }

        private void SubmitAndProcess()
        {
            bool scriptInvoked = false;
            while (!scriptInvoked)
            {
                try
                {
                    string loginScript = "$('form[action=\"account.php\"]').submit()";
                    browser.InvokeScript("eval", loginScript);
                    scriptInvoked = true;
                }
                catch (Exception ex)
                {
                    scriptInvoked = false;
                }
            }
        }
    }
}
