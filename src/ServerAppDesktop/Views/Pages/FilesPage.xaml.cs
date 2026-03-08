namespace ServerAppDesktop.Views.Pages;

public sealed partial class FilesPage : Page
{
    public FilesViewModel ViewModel { get; }
    public FilesPage()
    {
        InitializeComponent();
        ViewModel = App.GetRequiredService<FilesViewModel>();
    }
}
