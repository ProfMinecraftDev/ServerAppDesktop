namespace ServerAppDesktop.Views.Pages;




public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel { get; }
    public SettingsPage()
    {
        InitializeComponent();
        ViewModel = App.GetRequiredService<SettingsViewModel>();
    }
}
