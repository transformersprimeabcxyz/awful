using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Silverlight.Testing;
using Awful.Core.Web;
using System.Threading;
using Awful.Core.Models;

namespace Awful.Core.Test
{
    [Tag("web")]
    [Tag("auth")]
    [TestClass]
    public class AuthenticatorTest : SilverlightTest
    {
        [Asynchronous]
        [TestMethod]
        [Description("Test successful login to SA.")]
        public void TestSuccessfulLogin()
        {
            var auth = new AwfulAuthenticateByBrowser();
            string username = StaticParameters.TEST_USERNAME;
            string password = StaticParameters.TEST_PASSWORD;
            
            auth.Username = username;
            auth.Password = password;
            auth.Result += (o, a) =>
                {
                    Assert.AreEqual(a.Value, AwfulAuthenticateByBrowser.LoginResult.LOGIN_SUCCESSFUL);
                    this.EnqueueTestComplete();
                };

            auth.LoginAsync();
        }

        [Asynchronous]
        [TestMethod]
        [Description("Test for unsuccessful login")]
        public void TestUnsuccessfulLogin()
        {
            var auth = new AwfulAuthenticateByBrowser();
            string username = "bootleg robot";
            string password = "farts";

            auth.Username = username;
            auth.Password = password;
            auth.Result += (o, a) =>
                {
                    Assert.AreEqual(a.Value, AwfulAuthenticateByBrowser.LoginResult.LOGIN_FAILED);
                    this.EnqueueTestComplete();
                };

            auth.LoginAsync();
        }
    }
}
