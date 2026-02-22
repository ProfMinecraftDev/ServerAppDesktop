namespace ServerAppDesktop.Views.Pages;




public sealed partial class PerformancePage : Page
{
    public PerformanceViewModel ViewModel => App.GetRequiredService<PerformanceViewModel>();
    public PerformancePage()
    {
        InitializeComponent();
    }
}
