using System;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KollaSoft;
using HtmlAgilityPack;
using Awful.Core.Models.Messaging.Interfaces;
using Awful.Core.Web.Parsers;
using Awful.Core.Models.Messaging;

namespace Awful.Core.Test
{
    [Tag("unit")]
    [Tag("parsers")]
    [TestClass]
    public class AwfulPrivateMessageParserTest : SilverlightTest
    {
        private readonly HtmlDocument sampleMessage = new HtmlDocument();
        private readonly HtmlDocument sampleFolder = new HtmlDocument();
        private readonly HtmlDocument sampleMessageForm = new HtmlDocument();
        private readonly HtmlDocument editFolder = new HtmlDocument();

        [TestInitialize]
        public void Initialize()
        {
            sampleMessage.LoadHtml(Extensions.GetTextFromResourceFile(TestResources.sample_privatemessage));
            editFolder.LoadHtml(Extensions.GetTextFromResourceFile(TestResources.sample_editfolder));
            sampleFolder.LoadHtml(Extensions.GetTextFromResourceFile(TestResources.sample_privatemessagefolder));
            sampleMessageForm.LoadHtml(Extensions.GetTextFromResourceFile(TestResources.sample_privatemessageform));
        }

        [TestMethod]
        public void TestParseMessageFolderList()
        {
            var folders = AwfulPrivateMessageParser.ParseFolderList(this.sampleFolder);
            Assert.IsNotNull(folders);
            Assert.AreEqual(6, folders.Count);

            var enumerator = folders.GetEnumerator();
            enumerator.MoveNext();
            var folder = enumerator.Current;
            Assert.AreEqual("Inbox", folder.Name);
            Assert.AreEqual(0, folder.FolderID);
        }

        [TestMethod]
        public void TestParsePrivateMessage()
        {
            IPrivateMessage message = null;
            message = AwfulPrivateMessageParser.ParsePrivateMessageDocument(sampleMessage);
            Assert.IsFalse(message == null);
            Assert.AreEqual(message.PrivateMessageID, 4363289);
            Assert.AreEqual("bootleg robot", message.From);
            Assert.AreEqual("This is a test PM", message.Subject);
            Assert.AreEqual(5, message.Postmark.Value.Month);
            Assert.AreEqual(5, message.Postmark.Value.Day);
            Assert.AreEqual(2012, message.Postmark.Value.Year);
        }

        [TestMethod]
        public void TestParseEditFolderPage()
        {
            AwfulEditFolderRequest request = null;
            request = AwfulPrivateMessageParser.ParseEditFolderPage(editFolder) as AwfulEditFolderRequest;
            Assert.IsNotNull(request);
            Assert.AreEqual(8, request.NewFolderFieldIndex);
            Assert.AreEqual(7, request.FolderTable.Count);
            Assert.AreEqual("one", request.FolderTable[2]);
            Assert.AreEqual("two", request.FolderTable[3]);
            Assert.AreEqual("three", request.FolderTable[4]);
            Assert.AreEqual("four", request.FolderTable[5]);
            Assert.AreEqual(string.Empty, request.FolderTable[6]);
            Assert.AreEqual(string.Empty, request.FolderTable[7]);
            Assert.AreEqual(string.Empty, request.FolderTable[8]);
            Assert.AreEqual(7, request.HighestIndex);
        }

        [TestMethod]
        public void TestParseNewPrivateMessageForm()
        {
            IPrivateMessageRequest request = null;
            request = AwfulPrivateMessageParser.ParseNewPrivateMessageFormDocument(this.sampleMessageForm);
            Assert.IsFalse(request.IsForward);
            Assert.AreEqual("Naffer", request.To);
            Assert.AreEqual("Re: This is a test PM", request.Subject);

            string expectedSnippet = "Now here I am making use of the BBcode for fixed-width source code!  Below I pasted a code sample I found on the internet because I'm some sort of a developer.  Hopefully this doesn't break anything!";
            Assert.IsTrue(request.Body.Contains(expectedSnippet));
        }

        [TestMethod]
        public void TestParsePrivateMessageFolder()
        {
            IPrivateMessageFolder folder = null;
            folder = AwfulPrivateMessageParser.ParsePrivateMessageFolder(this.sampleFolder);
            Assert.IsNotNull(folder);
            Assert.AreEqual(3, folder.Messages.Count);
            // TODO: REMOVE COMMENT WHEN SA FIXES FOLDER NAME IN BREADCRUMBS.
            //Assert.AreEqual("two", folder.Name);
        }

       
        [TestMethod]
        public void TestGetNewPrivateMessageFormIconCollection()
        {
            var icons = AwfulPrivateMessageParser.ParseNewPrivateMessageIcons(this.sampleMessageForm);
            Assert.IsNotNull(icons);
            Assert.AreEqual(46, icons.Count);
        }
        
        [TestMethod]
        public void TestParseMessageList()
        {
            var messages = AwfulPrivateMessageParser.ParseMessageList(this.sampleFolder);
            Assert.IsNotNull(messages);
            Assert.AreEqual(3, messages.Count);
        }
    }
}
