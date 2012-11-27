using System;
using System.Linq;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Awful.Core.Models;
using Awful.Core.Database;


namespace Awful.Core.Test.Integration
{
    [Tag("db")]
    [Tag("forums")]
    [TestClass]
    public class AwfulForumsIntegrationTest : SilverlightTest
    {
        private bool IsSetUp { get; set; }
        private readonly AwfulThread thread1 = new AwfulThread();
        
        [TestInitialize]
        public void Initialize()
        {
            if (!IsSetUp)
            {
                var context = AwfulTestService.TestContext;
                using (var dao = new AwfulForumsDAO(context))
                {
                    dao.SetDatabaseToDefaults();
                }
                try
                {
                    var forum = new AwfulForum() { ForumName = "test forum" };
                    context.Forums.InsertOnSubmit(forum);
                    context.SubmitChanges();

                    thread1.Author = "author";
                    thread1.Forum = forum;
                    for (int i = 0; i < 5; i++)
                    {
                        thread1.Pages.Add(new AwfulThreadPage(thread1, i));
                    }

                    context.Threads.InsertOnSubmit(thread1);
                    context.SubmitChanges();
                    this.IsSetUp = true;
                }
                catch (Exception ex)
                {
                    Assert.Fail(ex.Message);
                    this.IsSetUp = false;
                }
            }
        }

        [TestMethod]
        public void TestGeneralThreadIntegration()
        {
            // make assertions based on test initialization
            var context = AwfulDataContext.CreateDataContext(AwfulTestService.TEST_FILENAME);
            Assert.IsTrue(context.Threads.Count() == 2);
            Assert.IsTrue(context.ThreadPages.Count() == 6);
            var thread = context.Threads.Where(t => t.ID == thread1.ID).SingleOrDefault();
            Assert.IsNotNull(thread);
            Assert.AreEqual("test forum", thread.Forum.ForumName);
            Assert.AreEqual(5, thread.Pages.Count);
            Assert.AreEqual("author", thread.Author);

            // let's add a post
            var page = thread.Pages[0];
            var pageID = page.ID;
            // in order to relate thread page : post, create post, add post to page,
            // insert post to page on submit, then submit changes
            var post = new AwfulPost() { ID = 1, PostAuthor = "post author" };
            page.Posts.Add(post);
            Assert.IsTrue(page.Posts.Count > 0);
            context.Posts.InsertOnSubmit(post);
            context.SubmitChanges();
            context.Dispose();

            context = AwfulDataContext.CreateDataContext(AwfulTestService.TEST_FILENAME);
            
            // did the post persist?
            int actualPostCount = context.Posts.Count();
            Assert.AreEqual(1, actualPostCount);
            
            // is the thread persisted?
            thread = context.Threads.Where(t => t.ID == thread.ID).SingleOrDefault();
            Assert.IsNotNull(thread);
            
            // is the page releated to the thread?
            page = thread.Pages.Where(p => p.ID == pageID).SingleOrDefault();
            post = context.Posts.FirstOrDefault();
            Assert.IsNotNull(post);
            Assert.IsNotNull(page);
            Assert.IsTrue(page.Posts.Count > 0);

            // is the post releated to the page?
            post = page.Posts[0];
            Assert.IsNotNull(post);
            Assert.AreEqual(page, post.ThreadPage);
        }
    }
}
