namespace ServerAppDesktop.Converters;

public sealed partial class BoolNegationToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is bool boolValue ? boolValue ? Visibility.Collapsed : Visibility.Visible : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return value is not Visibility visibility || visibility != Visibility.Visible;
    }
}
