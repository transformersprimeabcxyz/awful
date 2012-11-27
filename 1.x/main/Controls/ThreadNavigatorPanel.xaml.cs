using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace Awful
{
	public partial class ThreadNavigatorPanel : UserControl, INotifyPropertyChanged
	{
        private int customValue;

        public event EventHandler<ThreadNavigationArgs> NavigationRequested;
        public event EventHandler<EventArgs> NavigationCancelled;
        public event PropertyChangedEventHandler PropertyChanged;

		public ThreadNavigatorPanel()
		{
			// Required to initialize variables
			InitializeComponent();
            m_customVisible = false;
            customValue = -1;
            DataContext = this;
		}

        private Models.ThreadData m_thread;
        public Models.ThreadData Thread
        {
            get { return m_thread; }
            set
            {
                m_thread = value;
                NotifyPropertyChanged("Thread");
                MaxPages = m_thread.MaxPages > 0 ? m_thread.MaxPages.ToString() : "??";
            }
        }

        private string m_maxPages;
        public string MaxPages
        {
            get { return m_maxPages; }
            set
            {
                m_maxPages = value;
                NotifyPropertyChanged("MaxPages");
            }
        }

        private string m_customPage;
        public string CustomPageNumber
        {
            get { return m_customPage; }
            set
            {
                if(!ValidateEntry(value))
                {
                    if(String.IsNullOrEmpty(value))
                        throw new Exception("Invalid page entry.");
                }
                m_customPage = value;
                NotifyPropertyChanged("CustomPageNumber");
            }
        }

        private bool m_isVisible;
        public bool IsVisible
        {
            get { return m_isVisible; }
            set
            {
                if (m_isVisible != value)
                {
                    if (value)
                        VisualStateManager.GoToState(this, "Normal", true);
                    else
                        VisualStateManager.GoToState(this, "Hidden", true);

                    m_isVisible = value;
                }
            }
        }

        private bool ValidateEntry(string text)
        {
            if (String.IsNullOrEmpty(text)) return false;
            
            int page = -1;
            if (Int32.TryParse(text, out page))
            {
                if (page < 1 || page > m_thread.MaxPages)
                    return false;

                customValue = page;
                return true;
            }
            return false;
        }

        private void NotifyPropertyChanged(string p)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(p));
        }
        private void OnNavigationRequested(int page)
        {
            if (NavigationRequested != null)
            {
                NavigationRequested(this, new ThreadNavigationArgs(Thread, page));
            }
        }
        private void OnNavigationCancelled()
        {
            if (NavigationCancelled != null)
                NavigationCancelled(this, EventArgs.Empty);
        }

		private void FirstPageButtonClick(object sender, RoutedEventArgs e)
		{
            if (IsCustomPanelVisible)
                OnNavigationRequested(customValue);
            
            else
                OnNavigationRequested(1);
		}
		private void LastPageButtonClick(object sender, RoutedEventArgs e)
		{
            if (!IsCustomPanelVisible)
            {
                OnNavigationRequested(Thread.MaxPages);
            }
		}
		private void CustomPageButtonClick(object sender, RoutedEventArgs e)
		{
            IsCustomPanelVisible = !IsCustomPanelVisible;
		}
        private void CustomPageValueTextChanged(object sender, TextChangedEventArgs e)
        {
            // TODO: Add event handler implementation here.
        }
        private void CustomPageValue_BindingValidationError(object sender, ValidationErrorEventArgs e)
        {
            if (IsCustomPanelVisible)
            {
                FirstPageButton.IsEnabled = e.Action == ValidationErrorEventAction.Removed ? false : true;
            }
        }

        private bool m_customVisible;
        public bool IsCustomPanelVisible 
        {
            get { return m_customVisible; }
            set
            {
                if (m_customVisible != value)
                {
                    if (value) VisualStateManager.GoToState(this, "ShowCustom", true);
                    else VisualStateManager.GoToState(this, "Normal", true);
                    m_customVisible = value;
                }
            }
        }
    }

    public sealed class ThreadNavigationArgs : EventArgs
    {
        public Models.ThreadData Thread { get; private set; }
        public int Page { get; private set; }

        public ThreadNavigationArgs(Models.ThreadData thread,
            int page = 0)
        {
            Thread = thread;
            Page = page;
        }
    }
}