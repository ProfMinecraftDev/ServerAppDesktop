namespace ServerAppDesktop.Views.Pages;

public sealed partial class SystemInfoPage : Page
{
    public SystemInfoViewModel ViewModel { get; }
    public SystemInfoPage()
    {
        InitializeComponent();
        ViewModel = App.GetRequiredService<SystemInfoViewModel>();
    }
}
