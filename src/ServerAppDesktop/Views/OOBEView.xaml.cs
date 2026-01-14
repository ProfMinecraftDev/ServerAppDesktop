using Microsoft.UI.Xaml.Controls;
using ServerAppDesktop.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ServerAppDesktop.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class OOBEView : Page
    {
        public OOBEViewModel ViewModel { get; }

        public OOBEView()
        {
            InitializeComponent();
            ViewModel = App.GetRequiredService<OOBEViewModel>();
        }
    }
}
