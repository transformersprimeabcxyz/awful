using System;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HtmlAgilityPack;
using Awful.Core.Web.Parsers;
using KollaSoft;

namespace Awful.Core.Test
{
    [Tag("unit")]
    [Tag("parsers")]
    [TestClass]
    public class AwfulForumParserTest : SilverlightTest
    {
        private HtmlDocument samplePage;

        [TestInitialize]
        public void Initialize()
        {
            if (samplePage == null)
            {
                samplePage = new HtmlDocument();
                samplePage.LoadHtml(Extensions.GetTextFromResourceFile(TestResources.sample_forumPage));
            }
        }

        [TestMethod]
        public void TestForumList()
        {
            var forums = AwfulForumParser.ParseForumList(samplePage);
            Assert.IsNotNull(forums);
        }

        [TestMethod]
        public void TestForumPage()
        {
            var page = AwfulForumParser.ParseForumPage(samplePage);
            Assert.IsNotNull(page);
        }
    }
}
