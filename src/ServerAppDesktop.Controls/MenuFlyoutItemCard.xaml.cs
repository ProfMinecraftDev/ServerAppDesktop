namespace ServerAppDesktop.Controls;

[StyleTypedProperty(Property = nameof(ThemedIconStyle), StyleTargetType = typeof(ThemedIcon))]
public sealed partial class MenuFlyoutItemCard : MenuFlyoutItem
{
    public static readonly DependencyProperty CaptionProperty =
        DependencyProperty.Register(nameof(Caption), typeof(string), typeof(MenuFlyoutItemCard), new PropertyMetadata(string.Empty));

    public string Caption
    {
        get => (string)GetValue(CaptionProperty);
        set => SetValue(CaptionProperty, value);
    }

    public static readonly DependencyProperty CaptionForegroundProperty =
        DependencyProperty.Register(nameof(CaptionForeground), typeof(Microsoft.UI.Xaml.Media.Brush), typeof(MenuFlyoutItemCard), new PropertyMetadata(null));

    public Microsoft.UI.Xaml.Media.Brush CaptionForeground
    {
        get => (Microsoft.UI.Xaml.Media.Brush)GetValue(CaptionForegroundProperty);
        set => SetValue(CaptionForegroundProperty, value);
    }
    public Style ThemedIconStyle
    {
        get => (Style)GetValue(ThemedIconStyleProperty); set => SetValue(ThemedIconStyleProperty, value);
    }

    public static readonly DependencyProperty ThemedIconStyleProperty =
        DependencyProperty.Register("ThemedIconStyle", typeof(Style), typeof(MenuFlyoutItemCard), new PropertyMetadata(null, OnThemedIconStyleChanged));

    private static void OnThemedIconStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        d.To<MenuFlyoutItem>().Icon = e.NewValue is not null ? new IconSourceElement() : null;
    }

    public MenuFlyoutItemCard()
    {
        InitializeComponent();
    }
}
