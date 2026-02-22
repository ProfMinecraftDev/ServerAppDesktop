namespace ServerAppDesktop.Converters;

public sealed partial class StringToImageSourceConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        return (value is string s) ? new BitmapImage(new Uri(s)) : null;
    }

    public object? ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return DependencyProperty.UnsetValue;
    }
}
