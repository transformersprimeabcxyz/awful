﻿//      *********    DO NOT MODIFY THIS FILE     *********
//      This file is regenerated by a design tool. Making
//      changes to this file can cause errors.
namespace Expression.Blend.SampleData.SampleRatingViewModel
{
	using System; 

// To significantly reduce the sample data footprint in your production application, you can set
// the DISABLE_SAMPLE_DATA conditional compilation constant and disable sample data at runtime.
#if DISABLE_SAMPLE_DATA
	internal class SampleRatingViewModel { }
#else

	public class SampleRatingViewModel : System.ComponentModel.INotifyPropertyChanged
	{
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}

		public SampleRatingViewModel()
		{
			try
			{
				System.Uri resourceUri = new System.Uri("/Awful;component/SampleData/SampleRatingViewModel/SampleRatingViewModel.xaml", System.UriKind.Relative);
				if (System.Windows.Application.GetResourceStream(resourceUri) != null)
				{
					System.Windows.Application.LoadComponent(this, resourceUri);
				}
			}
			catch (System.Exception)
			{
			}
		}

		private Ratings _Ratings = new Ratings();

		public Ratings Ratings
		{
			get
			{
				return this._Ratings;
			}
		}
	}

	public class RatingsItem : System.ComponentModel.INotifyPropertyChanged
	{
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}

		private double _Value = 0;

		public double Value
		{
			get
			{
				return this._Value;
			}

			set
			{
				if (this._Value != value)
				{
					this._Value = value;
					this.OnPropertyChanged("Value");
				}
			}
		}

		private string _Color = string.Empty;

		public string Color
		{
			get
			{
				return this._Color;
			}

			set
			{
				if (this._Color != value)
				{
					this._Color = value;
					this.OnPropertyChanged("Color");
				}
			}
		}
	}

	public class Ratings : System.Collections.ObjectModel.ObservableCollection<RatingsItem>
	{ 
	}
#endif
}
