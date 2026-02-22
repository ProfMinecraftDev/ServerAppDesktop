namespace ServerAppDesktop.Controls;

public sealed partial class MenuFlyoutItemWithThemedIcon : MenuFlyoutItem
{
    public Style ThemedIconStyle
    { get => (Style)GetValue(ThemedIconStyleProperty); set => SetValue(ThemedIconStyleProperty, value);
    }

    public static readonly DependencyProperty ThemedIconStyleProperty =
        DependencyProperty.Register("ThemedIconStyle", typeof(Style), typeof(MenuFlyoutItemWithThemedIcon), new PropertyMetadata(null, OnThemedIconStyleChanged));

    private static void OnThemedIconStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        d.To<MenuFlyoutItem>().Icon = e.NewValue is not null ? new IconSourceElement() : null;
    }

    public MenuFlyoutItemWithThemedIcon()
    {
        InitializeComponent();
    }
}
