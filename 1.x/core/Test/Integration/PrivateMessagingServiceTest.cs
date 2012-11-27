using System;
using System.Linq;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Awful.Core.Web;
using Awful.Core.Models.Messaging.Interfaces;
using Awful.Core.Services;
using System.Threading;
using System.Windows;
using Awful.Core.Database;


namespace Awful.Core.Test
{
    [Tag("web")]
    [Tag("pm")]
    [TestClass]
    public class PrivateMessagingServiceTest : SilverlightTest
    {
        private AwfulAuthenticationService auth;
        private IPrivateMessagingService pmService;
       
        private const int PRIVATE_MESSAGE_TEST_NUMBER_OF_FOLDERS = 3;

        private const string PRIVATE_MESSAGE_TEST_NEW_MESSAGE_SUBJECT = "Test Message by Awful!";

        private const string PRIVATE_MESSAGE_TEST_CREATE_FOLDER_NAME = "Created By Awful!";
        private const string PRIVATE_MESSAGE_TEST_RENAME_FOLDER_NAME = "Renamed by Awful!";

        private const int PRIVATE_MESSAGE_TEST_MESSAGE_ID = 4345297;
        private const string PRIVATE_MESSAGE_TEST_MESSAGE_SUBJECT = "Sample PM";
        private const string PRIVATE_MESSAGE_TEST_MESSAGE_SENDER = "Dead Man Posting";
        private const int PRIVATE_MESSAGE_TEST_MESSAGE_POSTMARK_DAY = 22;
        private const int PRIVATE_MESSAGE_TEST_MESSAGE_POSTMARK_MONTH = 4;
        private const int PRIVATE_MESSAGE_TEST_MESSAGE_POSTMARK_YEAR = 2012;
        
        private const int PRIVATE_MESSAGE_TEST_FOLDER_ID = 4;
        private const string PRIVATE_MESSAGE_TEST_FOLDER_NAME = "Awful! -- testing";
       
        [TestInitialize]
        public void Initialize()
        {
            if (this.pmService == null) { this.pmService = AwfulServiceManager.PrivateMessageService; }
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
        public void TestFetchMessageAsync()
        {
            this.TestForAuthentication();
            int id = PRIVATE_MESSAGE_TEST_MESSAGE_ID;
            this.pmService.FetchMessageAsync(id, (status, result) =>
            {
                switch (status)
                {
                    case Models.ActionResult.Success:
                        Assert.IsFalse(result == null);
                        Assert.AreEqual(result.PrivateMessageID, id);
                        Assert.AreEqual(PRIVATE_MESSAGE_TEST_MESSAGE_SENDER, result.From);
                        Assert.AreEqual(PRIVATE_MESSAGE_TEST_MESSAGE_SUBJECT, result.Subject);
                        Assert.AreEqual(PRIVATE_MESSAGE_TEST_MESSAGE_POSTMARK_MONTH, result.Postmark.Value.Month);
                        Assert.AreEqual(PRIVATE_MESSAGE_TEST_MESSAGE_POSTMARK_DAY, result.Postmark.Value.Day);
                        Assert.AreEqual(PRIVATE_MESSAGE_TEST_MESSAGE_POSTMARK_YEAR, result.Postmark.Value.Year);
                        break;

                    default:
                        Assert.Fail("Fetch failed.");
                        break;
                }

                this.EnqueueTestComplete();
            });
        }

        [Asynchronous]
        [TestMethod]
        public void TestCreateAndSendMessageAsync()
        {
            this.TestForAuthentication();
            pmService.BeginNewMessageRequestAsync((status, request) =>
                {
                    if (status == Models.ActionResult.Success)
                    {
                        Assert.IsNotNull(request);
                        request.To = StaticParameters.TEST_USERNAME;
                        request.Subject = PRIVATE_MESSAGE_TEST_NEW_MESSAGE_SUBJECT;
                        request.Body = "If all goes well, then you should see this on SA! :v:";
                        pmService.SendMessageAsync(request, sendresult =>
                            {
                                if (sendresult == Models.ActionResult.Success)
                                {
                                    this.EnqueueTestComplete();
                                }
                            });
                    }

                    else 
                    { 
                        Assert.Fail("Failed to create message request.");
                        this.EnqueueTestComplete();
                    }
                });
        }

        [Asynchronous]
        [TestMethod]
        public void TestFetchFolderAsync()
        {
            this.TestForAuthentication();
            this.pmService.FetchFolderAsync(PRIVATE_MESSAGE_TEST_FOLDER_ID, (status, folder) =>
            {
                if (status == Models.ActionResult.Success)
                {
                    Assert.IsNotNull(folder);
                    // TODO: Uncomment this when SA fixes their page!
                    // Assert.AreEqual(PRIVATE_MESSAGE_TEST_FOLDER_NAME, folder.Name);
                    
                    Assert.IsNotNull(folder.Messages);
                    var enumerator = folder.Messages.GetEnumerator();
                    enumerator.MoveNext();
                    var message = enumerator.Current;
                    
                    Assert.AreEqual(PRIVATE_MESSAGE_TEST_MESSAGE_SUBJECT, message.Subject);
                    Assert.AreEqual(PRIVATE_MESSAGE_TEST_MESSAGE_SENDER, message.From);
                }
                else { Assert.Fail("Folder fetch failed."); }
                this.EnqueueTestComplete();
            });
        }

        [Asynchronous]
        [TestMethod]
        public void TestFetchFoldersAsync()
        {
            this.TestForAuthentication();
            this.pmService.FetchFoldersAsync((result, folders) =>
                {
                    if (result == Models.ActionResult.Success)
                    {
                        Assert.IsNotNull(folders);
                        Assert.IsTrue(folders.Count > 0);
                        int expectedCount = PRIVATE_MESSAGE_TEST_NUMBER_OF_FOLDERS;
                        int actualCount = folders.Count;
                        Assert.AreEqual(expectedCount, actualCount);
                    }
                    else { Assert.Fail("Failed to retrieve folders."); }
                    this.EnqueueTestComplete();
                });
        }

        [Asynchronous]
        [TestMethod]
        public void TestMoveMessageAsync()
        {
            this.TestForAuthentication();
            // take the test case message
            int messageID = PRIVATE_MESSAGE_TEST_MESSAGE_ID;
            // and move it from it's current folder
            int fromFolderID = PRIVATE_MESSAGE_TEST_FOLDER_ID;
            // to the inbox
            int toFolderID = 0;

            this.pmService.MoveMessageAsync(messageID, fromFolderID, toFolderID, result =>
                {
                    // success! now put it back.
                    if (result == Models.ActionResult.Success)
                    {
                        this.pmService.MoveMessageAsync(messageID, toFolderID, fromFolderID, result2 =>
                            {

                                if (result != Models.ActionResult.Success)
                                {
                                    Assert.Fail("The move back the original folder failed."); 
                                }
                                this.EnqueueTestComplete();
                            });
                    }
                    else
                    {
                        Assert.Fail("The move to the specified folder failed.");
                        this.EnqueueTestComplete();
                    }
                });
        }

        [Asynchronous]
        [TestMethod]
        public void TestDeleteMessageAsync()
        {
            this.TestForAuthentication();
            this.pmService.FetchInboxAsync((inboxStatus, inbox) =>
                {
                    if (inboxStatus == Models.ActionResult.Success)
                    {
                        Assert.IsNotNull(inbox);
                        Assert.IsNotNull(inbox.Messages);
                        var messageToDelete = inbox.Messages
                            .Where(message => message.Subject.Equals(PRIVATE_MESSAGE_TEST_NEW_MESSAGE_SUBJECT))
                            .FirstOrDefault();

                        if (messageToDelete == null) 
                        { 
                            Assert.Fail("Could not find generated test message in inbox.");
                            this.EnqueueTestComplete();
                        }
                        else
                        {
                            int id = messageToDelete.PrivateMessageID;
                            this.pmService.DeleteMessageAsync(id, inbox.FolderID, inbox.FolderID, deleteResult =>
                                {
                                    if (deleteResult != Models.ActionResult.Success){ Assert.Fail("Inbox delete failed."); }
                                    this.EnqueueTestComplete();
                                });
                        }
                    }
                    else 
                    { 
                        Assert.Fail("Inbox request failed.");
                        this.EnqueueTestComplete();
                    }
                });
        }

        [Asynchronous]
        [TestMethod]
        public void TestCreateFolderAsync()
        {
            this.TestForAuthentication();
            string folderName = PRIVATE_MESSAGE_TEST_CREATE_FOLDER_NAME;
            this.pmService.BeginEditFolderRequest((requestResult, request) =>
                {
                    if (requestResult == Models.ActionResult.Success)
                    {
                        Assert.IsNotNull(request);
                        request.CreateFolder(folderName);
                        this.pmService.SendEditFolderRequest(request, sendResult =>
                            {
                                if (sendResult != Models.ActionResult.Success)
                                {
                                    Assert.Fail("send edit folder request failed!");
                                }
                                this.EnqueueTestComplete();
                            });
                    }
                    else
                    {
                        Assert.Fail("edit folder request failed!");
                        this.EnqueueTestComplete();
                    }
                });
        }

        [Asynchronous]
        [TestMethod]
        public void TestDeleteFolderAsync()
        {
            this.TestForAuthentication();
            this.pmService.FetchFoldersAsync((fetchResult, folders) =>
                {
                    if (fetchResult != Models.ActionResult.Success)
                    {
                        Assert.Fail("failed to fetch folders!");
                        this.EnqueueTestComplete();
                    }
                    else
                    {
                        Assert.IsNotNull(folders);
                        var folder = folders.Where(f => f.Name.Equals(PRIVATE_MESSAGE_TEST_CREATE_FOLDER_NAME)).FirstOrDefault();
                        if (folder == null) { Assert.Fail("failed to find created test folder!"); this.EnqueueTestComplete(); }
                        else
                        {
                            this.pmService.BeginEditFolderRequest((editResult, request) =>
                                {
                                    if (editResult == Models.ActionResult.Success)
                                    {
                                        Assert.IsNotNull(request);
                                        request.DeleteFolder(folder);
                                        this.pmService.SendEditFolderRequest(request, sendResult =>
                                            {
                                                if (sendResult != Models.ActionResult.Success)
                                                {
                                                    Assert.Fail("failed to delete created test folder!");
                                                }
                                                this.EnqueueTestComplete();
                                            });
                                    }
                                    else { Assert.Fail("failed to create edit request!"); this.EnqueueTestComplete(); }
                                });
                        }
                    }
                });
        }

        private void TestForAuthentication()
        {
            if (AwfulWebRequest.CanAuthenticate == false) { Assert.Fail("Authenticaion failed!"); }
        }
    }
}
