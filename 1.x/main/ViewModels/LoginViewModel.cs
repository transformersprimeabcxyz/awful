using System;
using System.Linq;
using KollaSoft;
using Awful.Models;
using System.Net;
using Awful.Data;
using Awful.Core.Web;
using Awful.Core.Event;

namespace Awful.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly AwfulAuthenticator auth = new AwfulAuthenticator();
        private string _status;
        private bool _isLoading;
        private const string LOGIN_TEXT = "tap here to login";
        
        public string Username
        {
            get { return this.auth.Username; }
            set
            {
                this.auth.Username = value;
                this.NotifyPropertyChangedAsync("Username");
            }
        }

        public string Password
        {
            get { return this.auth.Password; }
            set
            {
                this.auth.Password = value;
                this.NotifyPropertyChangedAsync("Password");
            }
        }

        public bool IsLoading
        {
            get { return this._isLoading; }
            private set
            {
                this._isLoading = value;
                this.NotifyPropertyChangedAsync("IsLoading");
            }
        }

        public string StatusMessage
        {
            get { return this._status; }
            private set
            {
                this._status = value;
                this.NotifyPropertyChangedAsync("StatusMessage");
            }
        }

        public event EventHandler GrabbingCookies;

        public LoginViewModel()
        {
            this.IsLoading = false;
            this.StatusMessage = string.Empty;
            this.auth.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(OnAuthPropertyChanged);
            this.auth.Result += (o, a) => 
            {
                if (a.Value == AwfulAuthenticator.LoginResult.GRABBING_COOKIES)
                {
                    this.StatusMessage = "Grabbing Cookies...";
                    this.GrabbingCookies.Fire(this);
                }
            };
        }

        private void OnAuthPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.IsLoading = this.auth.IsLoading;
            this.StatusMessage = this.auth.Status;
        }

        protected override void InitializeForDesignMode()
        {
            this.Username = "user";
            this.Password = "pass";
            this.IsLoading = true;
            this.StatusMessage = "status goes here";
        }

        public void LoginAsync(Action<Awful.Core.Models.ActionResult> action)
        {
            EventHandler<ValueChangedEventArgs<AwfulAuthenticator.LoginResult>> login = null;
            login = (o, a) =>
                {
                    this.auth.Result -= new EventHandler<ValueChangedEventArgs<AwfulAuthenticator.LoginResult>>(login);
                    switch (a.Value)
                    {
                        case AwfulAuthenticator.LoginResult.LOGIN_SUCCESSFUL:
                            action(Awful.Core.Models.ActionResult.Success);
                            break;

                        default:
                            action(Awful.Core.Models.ActionResult.Failure);
                            break;
                    }
                };

            this.auth.Result += new EventHandler<ValueChangedEventArgs<AwfulAuthenticator.LoginResult>>(login);
            this.StartLoginAsync(action);
        }

        private void StartLoginAsync(Action<Awful.Core.Models.ActionResult> action)
        {
            var user = this.Username;
            if (string.IsNullOrEmpty(user) == false && user.Equals("!!DEBUGUSER!!"))
            {
                this.auth.Username = Data.SAForumDB.DefaultProfile.Username;
                this.auth.Password = Data.SAForumDB.DefaultProfile.Password;
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

            this.auth.LoginAsync();
        }

        public System.Windows.FrameworkElement Browser { get { return this.auth.LoginBrowser; } }
    }
}
