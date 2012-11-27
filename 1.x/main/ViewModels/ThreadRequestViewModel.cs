using System;
using Awful.Services;
using Awful.Models;
using System.Windows;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using Awful.Menus;
using KollaSoft;

namespace Awful.ViewModels
{
    public class ThreadRequestViewModel : KollaSoft.ViewModelBase
    {
        private readonly AwfulThreadCreatorService _creator = new AwfulThreadCreatorService();
        private ForumData _forum;
        private AwfulThreadRequest _request;
        private bool _isEnabled;
        private bool _isLoading;

        private readonly List<TagOption> _tags = new List<TagOption>()
        {
            new BoldTag(),
            new ItalicTag(),
            new QuoteTag()
        };

        public List<TagOption> Tags { get { return this._tags; } }

        public event EventHandler Activated;

        public ForumData Forum
        {
            get { return this._forum; }
            set
            {
                if (this._forum == value) return;
                this._forum = value;
                NotifyPropertyChangedAsync("Forum");
            }
        }

        public AwfulThreadRequest Request
        {
            get { return this._request; }
            set
            {
                this._request = value;
                NotifyPropertyChangedAsync("Request");

                this.IsEnabled = value != null;
            }
        }

        public bool IsEnabled
        {
            get { return this._isEnabled; }
            set
            {
                this._isEnabled = value;
                NotifyPropertyChangedAsync("IsEnabled");
            }
        }

        public bool IsLoading
        {
            get { return this._isLoading; }
            private set
            {
                this._isLoading = value;
                NotifyPropertyChangedAsync("IsLoading");
                if (value) { this.IsEnabled = false; }
                else { this.IsEnabled = true; }
            }
        }

        public void Activate()
        {
            if (this.IsInDesignMode) return;
            if (this.Forum == null) return;

            else
            {
                this.IsLoading = true;
                this._creator.RequestNewThreadAsync(this.Forum, (result, request) =>
                    {
                        switch (result)
                        {
                            case Awful.Core.Models.ActionResult.Success:
                                this.Request = request;
                                this.Activated.Fire(this);
                                break;

                            case Awful.Core.Models.ActionResult.Cancelled:
                                MessageBox.Show("Request canceled.", ":)", MessageBoxButton.OK);
                                break;

                            case Awful.Core.Models.ActionResult.Failure:
                                MessageBox.Show("An error occured while trying to reach the forums. Try again later",
                                    ":(", MessageBoxButton.OK);
                                break;
                        }

                        this.IsLoading = false;
                    });
            }
        }

        public void GetPreviewAsync(Action<SAThreadPage> result)
        {
            this.IsLoading = true;
            this._creator.GetPreviewAsync(this.Request, (aresult, page) =>
                {
                    switch (aresult)
                    {
                        case Awful.Core.Models.ActionResult.Success:
                            result(page);
                            break;

                        case Awful.Core.Models.ActionResult.Cancelled:
                            MessageBox.Show("Request canceled.", ":)", MessageBoxButton.OK);
                            break;

                        case Awful.Core.Models.ActionResult.Failure:
                            MessageBox.Show("An error occured while trying to reach the forums. Try again later",
                                ":(", MessageBoxButton.OK);
                            break;
                    }

                    this.IsLoading = false;
                });
        }

        public ThreadRequestViewModel()
        {
            if (IsInDesignMode)
                this.InitializeForDesignMode();
        }

     

        protected override void InitializeForDesignMode()
        {
            var req = new NewThreadRequest();
            req.Forum = new SAForum() { ForumName = "Sample Forum" };
            req.Icons = new SampleThreadIcons();
            req.Title = "Sample Thread Title";
            req.Text = "Sample Thread Text";

            this.Request = req;
            this.Forum = req.Forum;
            this.IsEnabled = false;
            this.IsLoading = true;
        }

        internal void CreateThreadAsync(Action<Awful.Core.Models.ActionResult> finish)
        {
            this.IsLoading = true;
            this._creator.SendNewThreadRequestAsync(this.Request, result =>
                {
                    this.IsLoading = false;
                    finish(result);
                });
        }
    }

    [DataContract]
    public sealed class ThreadRequestState
    {
        [DataMember]
        public string RequestTitle { get; set; }
        [DataMember]
        public string RequestText { get; set; }
        [DataMember]
        public int SelectedIconIndex { get; set; }
        [DataMember]
        public int ForumID { get; set; }

        public ThreadRequestState()
        {
            this.RequestText = string.Empty;
            this.RequestTitle = string.Empty;
            this.SelectedIconIndex = -1;
            this.ForumID = 0;
        }

        public static void SaveState(ThreadRequestViewModel viewModel)
        {
            if (viewModel != null)
            {
                var save = new ThreadRequestState();
                if (viewModel.Request != null)
                {
                    save.RequestText = viewModel.Request.Text;
                    save.RequestTitle = viewModel.Request.Title;
                    save.SelectedIconIndex = viewModel.Request.Icons.IndexOf(
                        viewModel.Request.SelectedIcon);
                }

                if (viewModel.Forum != null) { save.ForumID = viewModel.Forum.ID; }

                save.SeralizeToFile(Globals.Constants.STATE_DIRECTORY, "thread_request.state");
            }
        }

        public static ThreadRequestState LoadState()
        {
            string path = Globals.Constants.STATE_DIRECTORY + '\\' + "thread_request.state";
            ThreadRequestState state = path.DeseralizeFromFile<ThreadRequestState>();
            return state;
        }
    }
}
