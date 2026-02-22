namespace ServerAppDesktop.Views.Pages;

public sealed partial class HomePage : Page
{
    public HomeViewModel ViewModel => App.GetRequiredService<HomeViewModel>();
    public HomePage()
    {
        InitializeComponent();
    }
}
