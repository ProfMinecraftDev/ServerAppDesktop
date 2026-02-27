namespace ServerAppDesktop.Controls;

public partial class TrayIcon
{
    public event RoutedEventHandler? Click;
    public event RoutedEventHandler? RightClick;
    public event RoutedEventHandler? DoubleClick;

    partial void OnContextMenuRequested();
    partial void OnClickRequested();
    partial void OnDoubleClickRequested();
    partial void OnVisibilityChanged(Visibility value);
    partial void OnToolTipChanged(string value);
    partial void OnIconSourceChanged(ImageSource? value);
}
