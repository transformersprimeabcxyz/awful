using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Telerik.Windows.Controls;
using Awful.Models;
using Telerik.Windows.Controls.LoopingList;
using Awful.Helpers;

namespace Awful
{
    public partial class NewThread : PhoneApplicationPage
    {
        private const int SEND_POST_BUTTON_INDEX = 0;
        private const int PREVIEW_POST_BUTTON_INDEX = 1;
        private const int CANCEL_POST_BUTTON_INDEX = 2;

        private ViewModels.ThreadRequestViewModel _context;
        private ViewModels.ThreadRequestPreviewViewModel _previewContext;
        private readonly IApplicationBar _defaultBar;
        private AwfulTextBoxTagOptionAdapter _tagSource;

        private bool _block;
        private LoopingPanelScrollState _lastState = LoopingPanelScrollState.NotScrolling;
        private readonly Dictionary<string, int> _loopTable = new Dictionary<string, int>(10);

        private bool _hasPreviewed;
        
        private bool _prevMode;
        public bool IsInPreviewMode
        {
            get { return this._prevMode; }
            private set
            {
                if (value == this._prevMode) return;
                this._prevMode = value;
                if (value) { VisualStateManager.GoToState(this, "ShowPreview", true); }
                else { VisualStateManager.GoToState(this, "HidePreview", true); }
            }
        }

        private bool _tagsMode;
        public bool IsInTagsMode
        {
            get { return this._tagsMode; }
            set
            {
                this._tagsMode = value;
                if (value) { VisualStateManager.GoToState(this, "ShowTags", true); }
                else { VisualStateManager.GoToState(this, "HideTags", true); }
            }
        }

        public NewThread()
        {
            InitializeComponent();
            BindEvents();

            InteractionEffectManager.AllowedTypes.Add(typeof(RadDataBoundListBoxItem));
            InteractionEffectManager.AllowedTypes.Add(typeof(RadListPicker));
            InteractionEffectManager.AllowedTypes.Add(typeof(RadListPickerItem));

            this._context = this.LayoutRoot.DataContext as ViewModels.ThreadRequestViewModel;
            
            if (this._context == null)
                throw new Exception("DataContext should not be null.");

            this._tagSource = new AwfulTextBoxTagOptionAdapter(this.ThreadTextBox, this.tagOptions, this._context.Tags);
            
            this._previewContext = new ViewModels.ThreadRequestPreviewViewModel();
            this.PreviewViewer.SetViewModel(this._previewContext);
            this.PreviewViewer.PageHeaderPanel.Visibility = System.Windows.Visibility.Collapsed;
            this._defaultBar = this.ApplicationBar;
        }

        private void BindEvents()
        {
            this.SizeChanged += (sender, args) =>
            {
                this.tagOptions.ItemWidth = this.leftButtonBorder.ActualWidth;
            };

            this.ThreadTextBox.LostFocus += new RoutedEventHandler(this.TextBox_LostFocus);
            this.ThreadTextBox.GotFocus += new RoutedEventHandler(this.TextBox_GotFocus);
            this.ThreadTitleText.LostFocus += new RoutedEventHandler(this.TextBox_LostFocus);
            this.ThreadTitleText.GotFocus += new RoutedEventHandler(this.TextBox_GotFocus);

        }
    
        void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            // I don't want the user to make a send / preview decision until the text box
            // is databound. See TextBox_LostFocus.
            this._defaultBar.IsVisible = false;
        }

        void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // If the focus is lost, we can ensure that any changes made while the
            // textbox had focus is applied to the databound property.
            this._defaultBar.IsVisible = true;
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (this.IsInPreviewMode == true)
            {
                e.Cancel = true;
                this.IsInPreviewMode = false;
                this.ApplicationBar.IsVisible = true;
            }

            base.OnBackKeyPress(e);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var forum = PhoneApplicationService.Current.State["Forum"] as Models.ForumData;
            if (forum == null)
            {
                // then application has been removed from memory. Retrieve _context from storage.
                var state = ViewModels.ThreadRequestState.LoadState();
                EventHandler handler = null;
                handler = (obj, args) =>
                    {
                        this._context.Activated -= new EventHandler(handler);
                        this._context.Request.Title = state.RequestTitle;
                        this._context.Request.Text = state.RequestText;
                        this._context.Request.SelectedIcon = this._context.Request.Icons[state.SelectedIconIndex];
                    };

                this._context.Activated += new EventHandler(handler);
            }

            else
            {
                this._context.Forum = forum;
                this._context.Activate();
            }

            this._hasPreviewed = false;
            (this._defaultBar.Buttons[SEND_POST_BUTTON_INDEX] as IApplicationBarIconButton).IsEnabled = false;
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            if (e.IsNavigationInitiator == false)
            {
                if (this._context != null)
                    ViewModels.ThreadRequestState.SaveState(this._context);
            }
        }

        private void AppBarButtonClick(object sender, System.EventArgs e)
        {
            var button = sender as IApplicationBarIconButton;
            if (button == null) return;
            int index = this._defaultBar.Buttons.IndexOf(button);

            switch (index)
            {
                case SEND_POST_BUTTON_INDEX:
                    this.HandleSend();
                    break;

                case CANCEL_POST_BUTTON_INDEX:
                    this.NavigationService.GoBack();
                    break;

                case PREVIEW_POST_BUTTON_INDEX:
                    this.HandlePreview();
                    break;
            }
        }

        private void HandleSend()
        {
            MessageBoxResult response = MessageBox.Show("Create thread? Once started, this cannot be undone.",
                ":o",
                MessageBoxButton.OKCancel);

            if (response == MessageBoxResult.OK)
            {
                this._context.CreateThreadAsync(result =>
                 {
                     switch (result)
                     {
                         case Awful.Core.Models.ActionResult.Success:
                             MessageBox.Show("You created a thread!", ":)", MessageBoxButton.OK);
                             break;

                         case Awful.Core.Models.ActionResult.Failure:
                             MessageBox.Show("Request failed.", ":(", MessageBoxButton.OK);
                             break;
                     }
                 });

            }
        }

        private void HandlePreview()
        {
            this.IsInPreviewMode = true;
            this.ApplicationBar.IsVisible = false;

            this._context.GetPreviewAsync(result =>
                {
                    if (result != null)
                    {
                        this._hasPreviewed = true;
                        (this._defaultBar.Buttons[SEND_POST_BUTTON_INDEX] as IApplicationBarIconButton).IsEnabled = true;
                        this._previewContext.ShowPreviewAsync(Awful.Core.Models.ActionResult.Success, result);  
                    }
                });
        }

        // TODO: Remove this event.

        private void ThreadIconPicker_StateChanged(object sender, 
			Telerik.Windows.Controls.ListPickerStateChangedEventArgs e)
        {
        	if (e.PreviousState == ListPickerState.Expanding)
			{
				//SystemTray.IsVisible = false;	
			}
			else
			{
				//SystemTray.IsVisible = !App.Settings.HideSystemTray;	
			}
        }

        private void OnButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;
            switch (button.Tag.ToString())
            {
                case "TAGS":
                    this.IsInTagsMode = !this.IsInTagsMode;
                    break;

                case "SMILEY":
                    this.SmileyWindow.IsOpen = true;
                    break;
            }
        }

        private void OnTagOptionSelected(object sender, System.EventArgs e)
        {
        	// TODO: Add event handler implementation here.
        }

        private void OnOptionsButtonClick(object sender, System.EventArgs e)
        {
            var content = (sender as Button).Content as LoopingListDataItem;
            var title = content.Text;
            var index = this._tagSource.TitleTable[title];

            if (this.tagOptions.SelectedIndex == index)
            {
                this.tagOptions.SelectedIndex = -1;
            }

            this.tagOptions.SelectedIndex = index;
        }


        private const string MARGIN_UP = "LayoutMargin_PortraitUp";
        private const string MARGIN_DOWN = "LayoutMargin_PortraitDown";
        private const string MARGIN_LEFT = "LayoutMargin_LandscapeLeft";
        private const string MARGIN_RIGHT = "LayoutMargin_LandscapeRight";

        private void Page_OrientationChanged(object sender, Microsoft.Phone.Controls.OrientationChangedEventArgs e)
        {
            var orientation = e.Orientation;
            switch (orientation)
            {
                case PageOrientation.LandscapeLeft:
                    this.LayoutRoot.Margin = (Thickness)this.Resources[MARGIN_LEFT];
                    break;

                case PageOrientation.LandscapeRight:
                    this.LayoutRoot.Margin = (Thickness)this.Resources[MARGIN_RIGHT];
                    break;

                case PageOrientation.PortraitUp:
                    this.LayoutRoot.Margin = (Thickness)this.Resources[MARGIN_UP];
                    break;

                case PageOrientation.PortraitDown:
                    this.LayoutRoot.Margin = (Thickness)this.Resources[MARGIN_DOWN];
                    break;
            }
        }
    }
}
