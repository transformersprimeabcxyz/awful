using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HtmlAgilityPack;
using Awful.Core.Web.Parsers;
using KollaSoft;
using Awful.Core.Models;

namespace Awful.Core.Test.Unit
{
    [Tag("unit")]
    [Tag("parsers")]
    [TestClass]
    public class AwfulThreadParserTest : SilverlightTest
    {
        private HtmlNode sampleThreadNode;
        private HtmlDocument sampleThreadPage;
        private const int SAMPLE_THREAD_FORUM_ID = 27;

        [TestInitialize]
        public void Initialize()
        {
            if (sampleThreadNode == null)
            {
                sampleThreadNode = new HtmlNode(HtmlNodeType.Element, new HtmlDocument(), 0);
                sampleThreadNode.Name = "element";
                var text = Extensions.GetTextFromResourceFile(TestResources.sample_threadnode);
                sampleThreadNode.InnerHtml = text;
            }

            if (sampleThreadPage == null)
            {
                sampleThreadPage = new HtmlDocument();
                var text = Extensions.GetTextFromResourceFile(TestResources.sample_threadpage);
                sampleThreadPage.LoadHtml(text);
            }
        }

        [TestMethod]
        public void TestParseThreadFromNode()
        {
            var thread = AwfulThreadParser.ParseFromNode(SAMPLE_THREAD_FORUM_ID, sampleThreadNode.FirstChild);
            Assert.IsNotNull(thread);
            Assert.AreEqual("The World God Only Knows - Dating Sim Scenarios = Valid Dating Advice", thread.Title);
            Assert.AreEqual("organism", thread.Author);
            Assert.IsTrue(thread.HasBeenReadByUser);
            Assert.AreEqual(3152393, thread.ID);
            Assert.AreEqual(605, thread.NewPostCount);
        }

        [TestMethod]
        public void TestParseFromThreadPage()
        {
            var page = AwfulThreadParser.ParseFromThreadPage(this.sampleThreadPage);
            Assert.IsNotNull(page);
            Assert.AreEqual("Naruto Manga Thread 16: You won't defeat me by becoming Dr. Snakes.", page.Parent.Title);
            Assert.AreEqual(85, page.PageNumber);
            Assert.AreEqual(3415929, page.Parent.ID);
            Assert.AreEqual(15, (page as AwfulThreadPage).Posts.Count);
        }
    }
}
