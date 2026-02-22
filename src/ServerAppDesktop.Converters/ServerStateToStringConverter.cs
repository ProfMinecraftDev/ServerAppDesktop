
namespace ServerAppDesktop.Converters;

public sealed partial class ServerStateToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return (value is ServerState state) ? ServerUIHelper.GetStateString(state.State) : ResourceHelper.GetString("NoneItem");
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return DependencyProperty.UnsetValue;
    }
}
