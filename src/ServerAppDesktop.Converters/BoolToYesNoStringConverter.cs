namespace ServerAppDesktop.Converters;

public sealed partial class BoolToYesNoStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return (value is bool b && b) ? ResourceHelper.GetString("String_Yes") : ResourceHelper.GetString("String_No");
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return value is string s && s.Equals(ResourceHelper.GetString("String_Yes"), StringComparison.OrdinalIgnoreCase);
    }
}
