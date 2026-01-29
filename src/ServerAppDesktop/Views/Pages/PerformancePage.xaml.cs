using Microsoft.UI.Xaml.Controls;
using ServerAppDesktop.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ServerAppDesktop.Views.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PerformancePage : Page
    {
        public PerformanceViewModel ViewModel => App.GetRequiredService<PerformanceViewModel>();
        public PerformancePage()
        {
            InitializeComponent();
        }
    }
}
