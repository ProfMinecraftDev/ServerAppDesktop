
namespace ServerAppDesktop.Controls;

public partial class TrayIcon
{
    public static readonly DependencyProperty LeftClickCommandProperty =
        DependencyProperty.Register(nameof(LeftClickCommand), typeof(ICommand), typeof(TrayIcon), new PropertyMetadata(null));

    public ICommand LeftClickCommand
    {
        get => (ICommand)GetValue(LeftClickCommandProperty);
        set => SetValue(LeftClickCommandProperty, value);
    }

    public static readonly DependencyProperty RightClickCommandProperty =
        DependencyProperty.Register(nameof(RightClickCommand), typeof(ICommand), typeof(TrayIcon), new PropertyMetadata(null));

    public ICommand RightClickCommand
    {
        get => (ICommand)GetValue(RightClickCommandProperty);
        set => SetValue(RightClickCommandProperty, value);
    }

    public static readonly DependencyProperty LeftClickCommandParameterProperty =
        DependencyProperty.Register(nameof(LeftClickCommandParameter), typeof(object), typeof(TrayIcon), new PropertyMetadata(null));

    public object LeftClickCommandParameter
    {
        get => GetValue(LeftClickCommandParameterProperty);
        set => SetValue(LeftClickCommandParameterProperty, value);
    }

    public static readonly DependencyProperty RightClickCommandParameterProperty =
        DependencyProperty.Register(nameof(RightClickCommandParameter), typeof(object), typeof(TrayIcon), new PropertyMetadata(null));

    public object RightClickCommandParameter
    {
        get => GetValue(RightClickCommandParameterProperty);
        set => SetValue(RightClickCommandParameterProperty, value);
    }

    public static readonly DependencyProperty DoubleClickCommandProperty =
        DependencyProperty.Register(nameof(DoubleClickCommand), typeof(ICommand), typeof(TrayIcon), new PropertyMetadata(null));

    public ICommand DoubleClickCommand
    {
        get => (ICommand)GetValue(DoubleClickCommandProperty);
        set => SetValue(DoubleClickCommandProperty, value);
    }

    public static readonly DependencyProperty DoubleClickCommandParameterProperty =
        DependencyProperty.Register(nameof(DoubleClickCommandParameter), typeof(object), typeof(TrayIcon), new PropertyMetadata(null));

    public object DoubleClickCommandParameter
    {
        get => GetValue(DoubleClickCommandParameterProperty);
        set => SetValue(DoubleClickCommandParameterProperty, value);
    }
}
