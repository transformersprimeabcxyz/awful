using System;
using System.Windows;
using Awful.Core.Web;
using Awful.Core.Database;
using System.Collections.Generic;
using System.Net;

namespace Awful.Core.Services
{
    public class AwfulAuthenticationService : IApplicationService, IApplicationLifetimeAware
    {
        private const int NONE = -1;
        private AwfulApplicationSettings _settings;
        
        public AwfulAuthenticationService(AwfulApplicationSettings settings)
        {
            this._settings = settings;
            AwfulAuthenticator.LoginSuccessful += 
                new EventHandler<Event.ProfileChangedEventArgs>(AwfulAuthenticateByBrowser_LoginSuccessful);
        }

        private void AwfulAuthenticateByBrowser_LoginSuccessful(object sender, Event.ProfileChangedEventArgs e)
        {
            // update AwfulWebRequest Cookie jar
            AwfulWebRequest.SetCookieJar(e.Cookies);

            // save profile to database, along with authentication tokens
            using (var dao = new AwfulProfileDAO())
            {
                var updatedProfile = dao.SaveAuthenticationCookiesToProfile(e.Value, e.Cookies);
                if (updatedProfile == null)
                {
                    updatedProfile = dao.CreateProfile(e.Value.Username, e.Value.Password, e.Cookies);
                    if (updatedProfile != null) { this._settings.CurrentProfileID = updatedProfile.ID; }
                    else { this._settings.CurrentProfileID = NONE; }
                }
            }
        }

        #region ApplicationService Members

        public void StartService(ApplicationServiceContext context) { }

        public void StopService() { }

        public void Exited()
        {
            // nothing to do here, yet. //
        }

        public void Exiting()
        {
            // nothing to do here, yet. //
        }

        public void Started()
        {
            // nothing to do here, yet. //
        }

        public void Starting()
        {
            // Load up the current profile if it exists //
            using (var dao = new AwfulProfileDAO())
            {
                var currentProfile = dao.GetProfileByID(this._settings.CurrentProfileID);
                // set cookie jar 
                if (currentProfile != null)
                {
                    List<Cookie> cookies = new List<Cookie>();
                    var tokens = currentProfile.Tokens;
                    foreach (var token in tokens) { cookies.Add(token.AsCookie()); }
                    AwfulWebRequest.SetCookieJar(cookies);
                }
            }
        }

        #endregion
    }
}
