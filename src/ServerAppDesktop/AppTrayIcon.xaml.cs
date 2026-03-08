
namespace ServerAppDesktop;

public sealed partial class AppTrayIcon : Controls.TrayIcon
{
    public TrayViewModel ViewModel { get; } = App.GetRequiredService<TrayViewModel>();
    public HomeViewModel HomeViewModel { get; } = App.GetRequiredService<HomeViewModel>();
    public SettingsViewModel SettingsViewModel { get; } = App.GetRequiredService<SettingsViewModel>();


    public AppTrayIcon()
    {
        InitializeComponent();
    }
}
