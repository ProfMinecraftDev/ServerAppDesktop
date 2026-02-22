
namespace ServerAppDesktop;

public sealed partial class AppTrayIcon : TaskbarIcon
{
    public TrayViewModel ViewModel { get; } = App.GetRequiredService<TrayViewModel>();
    public HomeViewModel HomeViewModel { get; } = App.GetRequiredService<HomeViewModel>();


    public AppTrayIcon()
    {
        InitializeComponent();
    }
}
