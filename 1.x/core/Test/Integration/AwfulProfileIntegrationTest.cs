using System;
using System.Linq;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Awful.Core.Database;
using System.Net;
using System.Collections.Generic;
using Awful.Core.Models;

namespace Awful.Core.Test.Integration
{
    [Tag("db")]
    [Tag("profile")]
    [TestClass]
    public class AwfulProfileIntegrationTest : SilverlightTest
    {
        private bool IsSetUp { get; set; }

        private readonly AwfulProfile Profile1 = new AwfulProfile() { Username = "profile1", Password = "password1" };
        private readonly AwfulProfile Profile2 = new AwfulProfile() { Username = "profile2", Password = "password2" };
        private readonly AwfulProfile Profile3 = new AwfulProfile() { Username = "profile3", Password = "password3" };
        private readonly AwfulProfile Profile4 = new AwfulProfile() { Username = "profile4", Password = "password4" };
        private readonly AwfulProfile Profile5 = new AwfulProfile() { Username = "profile5", Password = "password5" };

        [TestInitialize]
        public void Initialize()
        {
            if (!IsSetUp)
            {
                var context = AwfulTestService.TestContext;
                context.Profiles.InsertOnSubmit(Profile1);
                context.Profiles.InsertOnSubmit(Profile2);
                context.Profiles.InsertOnSubmit(Profile3);
                context.Profiles.InsertOnSubmit(Profile4);
                context.Profiles.InsertOnSubmit(Profile5);
                context.SubmitChanges();
                try 
                { 
                    context.SubmitChanges(System.Data.Linq.ConflictMode.FailOnFirstConflict);
                    IsSetUp = true;
                }
                catch (Exception) { Assert.Fail("Database failure"); IsSetUp = false; }
            }
        }

        [TestMethod]
        public void TestCreateProfile()
        {
            using (var dao = new AwfulProfileDAO(AwfulTestService.TestContext))
            {
                List<Cookie> cookies = new List<Cookie>();
                cookies.Add(new Cookie("cookie1", "value1"));
                cookies.Add(new Cookie("cookie2", "value2"));
                var profile = dao.CreateProfile("username", "password", cookies);
                
                // ensure profile was created
                Assert.IsNotNull(profile);
                Assert.AreEqual("username", profile.Username);
                Assert.AreEqual("password", profile.Password);
                Assert.IsTrue(profile.Tokens.Count == 2);


                // ensure profile was persisted
                var profileInDatabase = AwfulTestService.TestContext.Profiles
                    .Where(p => p.Username == "username").SingleOrDefault();

                Assert.IsNotNull(profileInDatabase);
                Assert.IsTrue(profileInDatabase.Tokens.Count == 2);
            }
        }

        [TestMethod]
        public void TestUpdateUsername()
        {
            using (var dao = new AwfulProfileDAO(AwfulTestService.TestContext))
            {
                var profile = dao.RenameProfile(Profile1, "changed");
                Assert.IsNotNull(profile);
                Assert.AreEqual(profile.ID, Profile1.ID);
                Assert.AreEqual("changed", profile.Username);
                Assert.AreEqual(profile.Username, Profile1.Username);
            }
        }
    }
}
