
namespace ServerAppDesktop.Controls;

public partial class TrayIcon : FrameworkElement
{
    public string ToolTip
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            OnToolTipChanged(value);
        }
    } = string.Empty;

    public ImageSource? IconSource
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            OnIconSourceChanged(value);
        }
    }

    public TrayIcon()
    {
        ActualThemeChanged += OnActualThemeChanged;
        Loaded += OnLoaded;
        _ = RegisterPropertyChangedCallback(VisibilityProperty, OnVisibilityChanged);
    }

    ~TrayIcon()
    {
        ActualThemeChanged -= OnActualThemeChanged;
        Loaded -= OnLoaded;
        Dispose();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        CreateWindow();
    }

}
