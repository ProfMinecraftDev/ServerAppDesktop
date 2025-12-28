using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace ServerAppDesktop.Converters
{
	public sealed partial class NotNullToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			return value == null ? Visibility.Collapsed : Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			return DependencyProperty.UnsetValue;
		}
	}
}
