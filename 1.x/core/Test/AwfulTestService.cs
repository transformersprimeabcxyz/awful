using System;
using System.Data.Linq;
using System.Net;
using System.Windows;
using Awful.Core.Database;
using System.Collections.Generic;
using Awful.Core.Web;


namespace Awful.Core.Test
{
    public class AwfulTestService : IApplicationService, IApplicationLifetimeAware
    {
        public const string TEST_FILENAME = "TESTDB";
        public static AwfulDataContext TestContext { get; private set; }

        public void Exited()
        {
            throw new NotImplementedException();
        }

        public void Exiting()
        {
            using (var db = AwfulDataContext.CreateDataContext(TEST_FILENAME))
            {
                db.DeleteDatabase();
            }
        }

        public void Started()
        {
            var dao = new AwfulProfileDAO();

            var testProfile = dao.GetProfileByUsername(StaticParameters.TEST_USERNAME);
            if (testProfile != null)
            {
                AwfulWebRequest.SetCookieJar(testProfile.GetTokensAsCookies());
                if (!AwfulWebRequest.CanAuthenticate)
                {
                    AwfulAuthenticateByBrowser.LoginSuccessful += (o, a) =>
                        {
                            dao.SaveAuthenticationCookiesToProfile(testProfile, a.Cookies);
                            dao.Dispose();
                        };
                }
            }

            TestContext = AwfulDataContext.CreateDataContext(TEST_FILENAME);
        }

        public void Starting()
        {
            using (var db = AwfulDataContext.CreateDataContext(TEST_FILENAME))
            {
                if (db.DatabaseExists()) { db.DeleteDatabase(); }
                db.CreateDatabase();
            }

            using (var db = new AwfulDataContext())
            {
                if (!db.DatabaseExists()) 
                { 
                    db.CreateDatabase();
                    var dao = new AwfulProfileDAO(db);
                    dao.CreateProfile(StaticParameters.TEST_USERNAME, StaticParameters.TEST_PASSWORD, new List<Cookie>());
                }
            }
        }

        public void StartService(ApplicationServiceContext context)
        {
            
        }

        public void StopService()
        {
            
        }
    }
}
