namespace ServerAppDesktop.Converters;

public sealed partial class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is bool boolValue ? boolValue ? Visibility.Visible : Visibility.Collapsed : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return value is Visibility visibility && visibility == Visibility.Visible;
    }
}
