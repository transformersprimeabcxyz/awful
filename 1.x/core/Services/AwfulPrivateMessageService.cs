using System;
using System.Linq;
using System.Net;
using Awful.Core.Models;
using Awful.Core.Models.Messaging;
using Awful.Core.Models.Messaging.Interfaces;
using Awful.Core.Web.Parsers;
using System.Windows;
using Awful.Core.Event;
using System.IO;
using System.Threading;
using System.Text;
using System.Collections.Generic;
using Awful.Core.Web;

namespace Awful.Core.Services
{
    /// <summary>
    /// TODO: Add summary here.
    /// </summary>
    internal class AwfulPrivateMessageService : IPrivateMessagingService
    {
        private bool HasStarted { get; set; }
        private AwfulPrivateMessageService() { this.HasStarted = false; }

        #region fields
        private const string SEND_MESSAGE_ACTION_VALUE = "dosend";
        private const string NEW_MESSAGE_ACTION_VALUE = "newmessage";
        private const string MOVE_MESSAGE_ACTION_VALUE = "dostuff";
        private const string SEND_MESSAGE_SUBMIT_VALUE = "Send Message";
        private const string MOVE_MESSAGE_SUBMIT_VALUE = "Move";
        private const string DELETE_MESSAGE_SUBMIT_ACTION = "Delete";
        #endregion

        #region static
        internal static AwfulPrivateMessageService Service { get; private set; }
        static AwfulPrivateMessageService() { Service = new AwfulPrivateMessageService(); }
        #endregion

        #region public methods
        public void FetchMessageAsync(int privateMessageId, Action<ActionResult, IPrivateMessage> result)
        {
            var web = new Web.WebGet();
            var url = string.Format("{0}/{1}?action=show&privatemessageid={2}", Constants.BASE_URL,
                Constants.PRIVATE_MESSAGE_URI,
                privateMessageId);

            web.LoadAsync(url, (status, args) =>
                {
                    IPrivateMessage message = null;
                    if (status == ActionResult.Success)
                    {
                        var doc = args.Document;
                        message = AwfulPrivateMessageParser.ParsePrivateMessageDocument(doc);
                    }

                    result(status, message);
                });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="result"></param>
        public void SendMessageAsync(IPrivateMessageRequest request, Action<Models.ActionResult> result)
        {
            string url = string.Format("{0}/{1}", Constants.BASE_URL, Constants.PRIVATE_MESSAGE_URI);
            string postdata = string.Format("prevmessageid={0}&action={1}&forward={2}&touser={3}&title={4}&iconid={5}&message={6}&parseurl=yes&savecopy=yes&submit={7}",
                request.PrivateMessageID,
                SEND_MESSAGE_ACTION_VALUE,
                request.IsForward ? "yes" : string.Empty,
                HttpUtility.UrlEncode(request.To),
                HttpUtility.UrlEncode(request.Subject),
                request.SelectedTag.Value,
                HttpUtility.UrlEncode(request.Body),
                HttpUtility.UrlEncode(SEND_MESSAGE_SUBMIT_VALUE));

            this.SendPostAsync(postdata, url, result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="privateMessageID"></param>
        /// <param name="thisFolderID"></param>
        /// <param name="folderID"></param>
        /// <param name="result"></param>
        public void MoveMessageAsync(int privateMessageID, int thisFolderID, int folderID, Action<Models.ActionResult> result)
        {
            List<int> ids = new List<int>();
            ids.Add(privateMessageID);
            this.MoveMessagesAsync(ids, thisFolderID, folderID, result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="privateMessageID"></param>
        /// <param name="thisFolderID"></param>
        /// <param name="folderID"></param>
        /// <param name="result"></param>
        public void DeleteMessageAsync(int privateMessageID, int thisFolderID, int folderID, Action<Models.ActionResult> result)
        {
            List<int> list = new List<int>();
            list.Add(privateMessageID);
            this.DeleteMessagesAsync(list, thisFolderID, folderID, result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="privateMessageIDs"></param>
        /// <param name="thisFolderID"></param>
        /// <param name="folderID"></param>
        /// <param name="result"></param>
        public void DeleteMessagesAsync(IList<int> privateMessageIDs, int thisFolderID, int folderID, Action<ActionResult> result)
        {
            string deleteAction = string.Format("&delete={0}", DELETE_MESSAGE_SUBMIT_ACTION);
            this.HandleMessagesAsync(privateMessageIDs, thisFolderID, folderID, deleteAction, result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        public void FetchFoldersAsync(Action<Models.ActionResult, ICollection<IPrivateMessageFolder>> result)
        {
            // pull inbox html from server
            string url = string.Format("{0}/{1}?folderid={2}", Constants.BASE_URL, Constants.PRIVATE_MESSAGE_URI, 0);
            var web = new Web.WebGet();
            web.LoadAsync(url, (status, args) =>
            {
                List<IPrivateMessageFolder> folders = null;
                switch (status)
                {
                    case ActionResult.Success:
                        folders = new List<IPrivateMessageFolder>();
                        folders.AddRange(AwfulPrivateMessageParser.ParseFolderList(args.Document));
                        if (folders.Count > 0)
                        {
                            // let's cheat a little bit by parsing the inbox as well...
                            var messages = AwfulPrivateMessageParser.ParseMessageList(args.Document);
                            foreach (var message in messages)
                            {
                                folders[0].Messages.Add(message);
                            }
                        }
                        break;
                }
                result(status, folders);
            });
        }

        /// <summary>
        /// TODO: Add summary here.
        /// </summary>
        /// <param name="result"></param>
        public void FetchInboxAsync(Action<Models.ActionResult, IPrivateMessageFolder> result)
        {
            FetchFolderAsync(0, result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        public void FetchSentItemsAsync(Action<Models.ActionResult, IPrivateMessageFolder> result)
        {
            FetchFolderAsync(-1, result);
        }

        /// <summary>
        /// TODO: Add summary here.
        /// </summary>
        /// <param name="folderID"></param>
        /// <param name="result"></param>
        public void FetchFolderAsync(int folderID, Action<Models.ActionResult, IPrivateMessageFolder> result)
        {
            string url = string.Format("{0}/{1}?folderid={2}", Constants.BASE_URL, Constants.PRIVATE_MESSAGE_URI, folderID);
            var web = new Web.WebGet();
            web.LoadAsync(url, (status, args) =>
            {
                IPrivateMessageFolder folder = null;
                switch (status)
                {
                    case ActionResult.Success:
                        folder = AwfulPrivateMessageParser.ParsePrivateMessageFolder(args.Document);
                        break;
                }
                result(status, folder);
            });
        }

        /// <summary>
        /// TODO : Add summary here.
        /// </summary>
        /// <param name="result"></param>
        public void BeginEditFolderRequest(Action<ActionResult, IPrivateMessageFolderRequest> result)
        {
            var web = new Web.WebGet();
            var url = string.Format("{0}/{1}?action=editfolders", Constants.BASE_URL, Constants.PRIVATE_MESSAGE_URI);
            web.LoadAsync(url, (status, args) =>
                {
                    IPrivateMessageFolderRequest request = null;
                    switch (status)
                    {
                        case ActionResult.Success:
                            request = AwfulPrivateMessageParser.ParseEditFolderPage(args.Document);
                            break;
                    }

                    result(status, request);
                });
        }

        /// <summary>
        /// TODO : Add summary here.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="result"></param>
        public void SendEditFolderRequest(IPrivateMessageFolderRequest request, Action<ActionResult> result)
        {
            string url = string.Format("{0}/{1}", Constants.BASE_URL, Constants.PRIVATE_MESSAGE_URI);
            string postData = request.GenerateRequestData();
            this.SendPostAsync(postData, url, result);
        }

        /// <summary>
        /// TODO: Add summary here. 
        /// </summary>
        /// <param name="result"></param>
        public void BeginNewMessageRequestAsync(Action<Models.ActionResult, IPrivateMessageRequest> result)
        {
            var web = new Web.WebGet();
            var url = string.Format("{0}/{1}?action={2}", Constants.BASE_URL, Constants.PRIVATE_MESSAGE_URI, NEW_MESSAGE_ACTION_VALUE);
            this.GetMessageRequest(url, result);
        }

        /// <summary>
        /// TODO: Add summary here.
        /// </summary>
        /// <param name="privateMessageID"></param>
        /// <param name="result"></param>
        public void BeginReplyToMessageRequest(int privateMessageID, Action<Models.ActionResult, IPrivateMessageRequest> result)
        {
            var web = new Web.WebGet();
            var url = string.Format("{0}/{1}?action={2}&privatemessageid={3}", 
                Constants.BASE_URL, Constants.PRIVATE_MESSAGE_URI, NEW_MESSAGE_ACTION_VALUE, privateMessageID);
            this.GetMessageRequest(url, result);
        }

        /// <summary>
        /// TODO: Add summary here.
        /// </summary>
        /// <param name="privateMessageID"></param>
        /// <param name="result"></param>
        public void BeginForwardToMessageRequest(int privateMessageID, Action<Models.ActionResult, IPrivateMessageRequest> result)
        {
            var web = new Web.WebGet();
            var url = string.Format("{0}/{1}?action={2}&forward=true&privatemessageid={3}",
                Constants.BASE_URL, Constants.PRIVATE_MESSAGE_URI, NEW_MESSAGE_ACTION_VALUE, privateMessageID);
            this.GetMessageRequest(url, result);
        }

        /// <summary>
        /// TODO: Add summary here.
        /// </summary>
        /// <param name="privateMessageIDs"></param>
        /// <param name="thisFolderID"></param>
        /// <param name="folderID"></param>
        /// <param name="result"></param>
        public void MoveMessagesAsync(System.Collections.Generic.List<int> privateMessageIDs, int thisFolderID, int folderID, Action<ActionResult> result)
        {
            string submitMove = string.Format("&move={0}", MOVE_MESSAGE_SUBMIT_VALUE);
            this.HandleMessagesAsync(privateMessageIDs, thisFolderID, folderID, submitMove, result);
        }

        /// <summary>
        /// TODO: Add summary here.
        /// </summary>
        /// <param name="context"></param>
        public void StartService(ApplicationServiceContext context)
        {
            if (!this.HasStarted) { this.HasStarted = true; }
        }

        /// <summary>
        /// TODO: Add summary here.
        /// </summary>
        public void StopService()
        {
            if (this.HasStarted) { this.HasStarted = false; }
        }
        #endregion

        #region private methods

        private void HandleMessagesAsync(IList<int> privateMessageIDs, int thisFolderID, int folderID, 
            string action, Action<ActionResult> result)
        {

            StringBuilder urlBuilder = new StringBuilder();
            // first, create private message id string (privatemessage[id])
            var iterator = privateMessageIDs.GetEnumerator();
            if (iterator.MoveNext() != null)
            {
                urlBuilder.Append(this.FormatPrivateMessageIDUrlString(iterator.Current));
                urlBuilder.Append("=yes");
            }
            while (iterator.MoveNext())
            {
                urlBuilder.Append("&");
                urlBuilder.Append(this.FormatPrivateMessageIDUrlString(iterator.Current));
                urlBuilder.Append("yes");
                iterator.MoveNext();
            }

            // next, add 'dostuff' action
            urlBuilder.AppendFormat("&action={0}", MOVE_MESSAGE_ACTION_VALUE);
            // then this folder id
            urlBuilder.AppendFormat("&thisfolder={0}", thisFolderID);
            // then to folder id
            urlBuilder.AppendFormat("&folderid={0}", folderID);
            // then submit command
            urlBuilder.Append(action);

            string url = string.Format("{0}/{1}", Constants.BASE_URL, Constants.PRIVATE_MESSAGE_URI);
            string postdata = urlBuilder.ToString();

            // submit to something Awful.Core...
            this.SendPostAsync(postdata, url, result);
        }

        private void SendPostAsync(string postData, string url, Action<ActionResult> result)
        {
            var httpRequest = AwfulWebRequest.CreatePostRequest(url);

            Logger.AddEntry("Send Post start...");
            httpRequest.BeginGetRequestStream(callback =>
                {
                    Logger.AddEntry("url = " + url);
                    this.BeginPostRequest(postData, callback, result);

                }, httpRequest);
        }

        private void BeginPostRequest(string postData, IAsyncResult asyncResult, Action<ActionResult> result)
        {
            try
            {
                Logger.AddEntry("post data = " + postData);
                HttpWebRequest getRequest = asyncResult.AsyncState as HttpWebRequest;
                using (StreamWriter writer = new StreamWriter(getRequest.EndGetRequestStream(asyncResult)))
                {
                    var postBody = postData;
                    writer.Write(postData);
                    writer.Close();
                }

                var async = getRequest.BeginGetResponse(callback =>
                    {
                        result(this.ProcessPostResponse(callback));

                    }, getRequest);
            }

            catch (Exception)
            {
                result(ActionResult.Failure);
            }
        }

        private ActionResult ProcessPostResponse(IAsyncResult callback)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)callback.AsyncState;
                HttpWebResponse response = request.EndGetResponse(callback) as HttpWebResponse;
                if (response.StatusCode == HttpStatusCode.OK)
                    return ActionResult.Success;
                else
                    return ActionResult.Failure;
            }

            catch (Exception)
            {
                return ActionResult.Failure;
            }
        }

        private void GetMessageRequest(string url, Action<ActionResult, IPrivateMessageRequest> result)
        {
            var web = new Web.WebGet();
            web.LoadAsync(url, (webResult, args) =>
            {
                IPrivateMessageRequest request = null;
                if (webResult == ActionResult.Success)
                {
                    request = AwfulPrivateMessageParser
                        .ParseNewPrivateMessageFormDocument(args.Document);
                }
                result(webResult, request);
            });           
        }

        private string FormatPrivateMessageIDUrlString(int id)
        {
            return HttpUtility.UrlEncode(string.Format("privatemessage[{0}]", id));
        }
    
        #endregion
    }
}
