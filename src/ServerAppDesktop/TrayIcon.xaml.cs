using H.NotifyIcon;
using ServerAppDesktop.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ServerAppDesktop
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary
    public sealed partial class TrayIcon : TaskbarIcon
    {
        public TrayViewModel ViewModel => App.GetRequiredService<TrayViewModel>();

        public TrayIcon()
        {
            InitializeComponent();
            DataContext = ViewModel;
        }
    }
}
