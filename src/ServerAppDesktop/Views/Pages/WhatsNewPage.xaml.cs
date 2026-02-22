namespace ServerAppDesktop.Views.Pages;




public sealed partial class WhatsNewPage : Page
{
    public WhatsNewViewModel ViewModel { get; } = new WhatsNewViewModel();
    public WhatsNewPage()
    {
        InitializeComponent();
    }
}
