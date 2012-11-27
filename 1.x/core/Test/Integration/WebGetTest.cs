using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Silverlight.Testing;
using Awful.Core.Web;
using System.Threading;
using Awful.Core.Database;

namespace Awful.Core.Test
{
    [Tag("web")]
    [Tag("webget")]
    [TestClass]
    public class WebGetTest : SilverlightTest
    {
        [TestInitialize]
        public void Initialize()
        {
            if (!AwfulWebRequest.CanAuthenticate)
            {
                using (var dao = new AwfulProfileDAO())
                {
                    var profile = dao.GetProfileByUsername(StaticParameters.TEST_USERNAME);
                    if (profile != null)
                    {
                        AwfulWebRequest.SetCookieJar(profile.GetTokensAsCookies());
                    }
                }
            }
        }

        [Asynchronous]
        [TestMethod]
        [Description("Test html grab from SA.")]
        public void TestLoadAsync()
        {
            var web = new WebGet();
            if (!AwfulWebRequest.CanAuthenticate)
            {
                Assert.Fail("Failed to login to SA.");
            }

            string url = "http://forums.somethingawful.com/forumdisplay.php?forumid=1";
            string target = "<title>General Bullshit - The Something Awful Forums</title>";
            web.LoadAsync(url, (result, args) =>
                {
                    switch (result)
                    {
                        case Models.ActionResult.Failure:
                            Assert.Fail("LoadAsync failed to load the page.");
                            break;

                        default:
                            try
                            {
                                string html = args.Document.DocumentNode.OuterHtml;
                                bool expected = true;
                                bool actual = html.Contains(target);
                                Assert.AreEqual(expected, actual);
                            }
                            catch (Exception) { Assert.Fail("LoadAsync failed to parse the web document."); }
                            break;
                    }
                    EnqueueTestComplete();
                });
        }
    }
}
