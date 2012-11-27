using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Awful
{
    public interface IThreadPageViewControlViewModel
    {
        event EventHandler PageLoaded;
        event EventHandler PageLoading;
        event EventHandler PageFailed;

    }

	public partial class ThreadPageViewControl : UserControl
	{
		public ThreadPageViewControl()
		{
			// Required to initialize variables
			InitializeComponent();
		}

        public static DependencyProperty ViewModelProperty = DependencyProperty.Register(
            "ViewModel", typeof(IThreadPageViewControlViewModel), typeof(ThreadPageViewControl), new PropertyMetadata(null,
                (obj, args) => 
                {
                    var control = obj as ThreadPageViewControl;
                    if (control != null) { control.OnViewModelPropertyChanged(args); }
                }));

        private void OnViewModelPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            IThreadPageViewControlViewModel oldModel = args.OldValue as IThreadPageViewControlViewModel;
            IThreadPageViewControlViewModel newModel = args.NewValue as IThreadPageViewControlViewModel;

            this.UnbindViewModel(oldModel);
            this.BindViewModel(newModel);
            this.DataContext = newModel;

        }

        private void BindViewModel(IThreadPageViewControlViewModel viewModel)
        {
            if (viewModel == null) { return; }

            viewModel.PageLoaded += new EventHandler(OnViewModelPageLoaded);
            viewModel.PageLoading += new EventHandler(OnViewModelPageLoading);
            viewModel.PageFailed += new EventHandler(OnViewModelPageFailed);
        }

        private void UnbindViewModel(IThreadPageViewControlViewModel viewModel)
        {
            if (viewModel == null) { return; }

            viewModel.PageLoaded -= new EventHandler(OnViewModelPageLoaded);
            viewModel.PageLoading -= new EventHandler(OnViewModelPageLoading);
            viewModel.PageFailed -= new EventHandler(OnViewModelPageFailed);
        }

        private void OnViewModelPageFailed(object sender, EventArgs e)
        {
            MessageBox.Show(ErrorMessages.PAGE_LOAD_FAILED, ":(", MessageBoxButton.OK);
            VisualStateManager.GoToState(this, "PageFailed", true);
        }

        private void OnViewModelPageLoading(object sender, EventArgs e)
        {
            VisualStateManager.GoToState(this, "PageLoading", true);
        }

        private void OnViewModelPageLoaded(object sender, EventArgs e)
        {
            VisualStateManager.GoToState(this, "PageLoaded", true);
        }
	}
}