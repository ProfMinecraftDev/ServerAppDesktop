using H.NotifyIcon;
using ServerAppDesktop.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ServerAppDesktop
{
    public sealed partial class AppTrayIcon : TaskbarIcon
    {
        public TrayViewModel ViewModel => App.GetRequiredService<TrayViewModel>();
        public AppTrayIcon()
        {
            InitializeComponent();
        }

    }
}
