using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;

namespace ServerAppDesktop.Converters
{
    public sealed partial class StringToImageSourceConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, string language)
            => (value is string s) ? new BitmapImage(new Uri(s)) : null;

        public object? ConvertBack(object value, Type targetType, object parameter, string language)
            => DependencyProperty.UnsetValue;
    }
}
