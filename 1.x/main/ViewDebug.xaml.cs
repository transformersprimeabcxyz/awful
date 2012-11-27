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
using System.ComponentModel;
using System.IO.IsolatedStorage;
using System.IO;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using System.Threading;

namespace Awful
{
    public partial class ViewDebug : PhoneApplicationPage, INotifyPropertyChanged
    {
        private string m_selectedFile;
        private IList<string> m_debugText;
        private IList<string> m_debugList;
        private const int emailButton = 0;
        private const int deleteButton = 1;
        private string debugText;
        private readonly IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();

        public bool IsEmailEnabled
        {
            set
            {
                var button = ApplicationBar.Buttons[emailButton] as IApplicationBarIconButton;
                button.IsEnabled = value;
            }
        }

        public bool IsDeleteEnabled
        {
            set
            {
                var button = ApplicationBar.Buttons[deleteButton] as IApplicationBarIconButton;
                button.IsEnabled = value;
            }
        }

        public string SelectedFile
        {
            get 
            {
                return m_selectedFile; 
            }
            set
            {
                if (m_selectedFile == value) return;
                m_selectedFile = value;
                NotifyPropertyChanged("SelectedFile");

                ViewDebugText(value);
                if (value == null)
                {
                    IsEmailEnabled = false;
                    IsDeleteEnabled = false;
                }
            }
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            SystemTray.IsVisible = !App.Settings.HideSystemTray;
            base.OnNavigatedTo(e);
        }

        public IList<string> DebugList
        {
            get { return m_debugList; }
            set
            {
                if (m_debugList == value) return;
                m_debugList = value;
                NotifyPropertyChanged("DebugList");

                if (value == null)
                {
                    IsEmailEnabled = false;
                    IsDeleteEnabled = false;
                    DebugFilePicker.IsEnabled = false;
                }
                else
                {
                    DebugFilePicker.IsEnabled = true;
                }
            }
        }

        public IList<string> LogText
        {
            get { return m_debugText; }
            set
            {
                if (m_debugText == value) return;
                m_debugText = value;
                NotifyPropertyChanged("LogText");
            }
        }

        public ViewDebug()
        {
            InitializeComponent();
            DebugFilePicker.PopupClosed += new EventHandler<EventArgs>(DebugFilePicker_PopupClosed);
            Awful.Core.Event.Logger.ExportToFile();

            GenerateFileNames();
           
            IsDeleteEnabled = false;
            IsEmailEnabled = false;

            this.DataContext = this;
        }

        void DebugFilePicker_PopupClosed(object sender, EventArgs e)
        {
            if (SelectedFile != null)
            {
                IsEmailEnabled = true;
                IsDeleteEnabled = true;
            }
        }

        private void GenerateFileNames()
        {
            var logs = store.GetFileNames("logs\\*.*");
            
            if (logs.Length == 0)
                DebugList = null;
            else
                DebugList = new List<string>(logs);
        }

        private void ViewDebugText(string file)
        {
            if (string.IsNullOrEmpty(file))
            {
                LogText = null;
                return;
            }
            string filePath = string.Format("logs/{0}", file);
            Stream stream = null;
            try
            {
                stream = store.OpenFile(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                using (StreamReader reader = new StreamReader(stream))
                {
                    debugText = reader.ReadToEnd();
                    var virtualReader = new Helpers.VirtualTextReader(debugText, 300, 40);
                    LogText = virtualReader;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Could not open the file '" + filePath + "'.", ":(", MessageBoxButton.OK);
                if (stream != null)
                    stream.Close();

                SelectedFile = null;
            }
        }

        void virtualReader_SizeChanged(object sender, EventArgs e)
        {
            var dispatch = Deployment.Current.Dispatcher;
            dispatch.BeginInvoke(() =>
                {
                    NotifyPropertyChanged("LogText");
                });
        }

        private void DebugFileList_SelectionChanged(object sender, 
            System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var selected = e.AddedItems[0];
                DebugFilePicker.Content = selected;
            }
            else
                DebugFilePicker.Content = null;

            DebugFilePicker.IsPopupOpen = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        private void ApplicationBarInfo_ButtonClick(object sender, Telerik.Windows.Controls.ApplicationBarButtonClickEventArgs e)
        {
            DebugFilePicker.IsPopupOpen = false;
        }

        private void DebugFileList_Loaded(object sender, RoutedEventArgs e)
        {
            var element = sender as FrameworkElement;
            element.DataContext = this;
        }

        private void AppButton_Click(object sender, System.EventArgs e)
        {
            var button = sender as IApplicationBarIconButton;
            switch (button.Text)
            {
                case "email":
                    HandleEmail();
                    break;

                case "delete":
                    HandleDelete();
                    break;
            }
        }

        private void HandleDelete()
        {
            var response = MessageBox.Show(string.Format("Are you sure you want to delete '{0}'?", SelectedFile), ":o", MessageBoxButton.OKCancel);
            if (response == MessageBoxResult.OK)
            {
                    var path = System.IO.Path.Combine("logs", SelectedFile.ToString());
                    SelectedFile = null;

                    ThreadPool.QueueUserWorkItem(new WaitCallback(state =>
                        {
                            try 
                            {
                                store.DeleteFile(path);
                                var dispatch = Deployment.Current.Dispatcher;
                                 dispatch.BeginInvoke(() =>
                                {
                                    MessageBox.Show("File deleted.", ":)", MessageBoxButton.OK);
                                    GenerateFileNames();
                                });
                            }
                            catch (Exception) { }
                        }), null);
            }
                
        }
        
        private void HandleEmail()
        {
            var email = new EmailComposeTask();
           
            email.To = "kollasoftware@gmail.com";
            email.Subject = string.Format("Awful App Debug - {0} - {1}", App.Settings.Username,
                SelectedFile.ToString());
            try
            {
                email.Body = debugText;
                email.Show();
            }
            catch (Exception ex)
            {
                email.Body = debugText.Substring(0, 20000);
                email.Show();
            }
        }
    }
}
