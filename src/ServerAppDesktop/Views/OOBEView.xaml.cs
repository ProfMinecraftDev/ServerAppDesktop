using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ServerAppDesktop.Helpers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ServerAppDesktop.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class OOBEView : Page
    {
        public OOBEView()
        {
            InitializeComponent();
            Loaded += OOBEView_Loaded;
        }

        private void OOBEView_Loaded(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = ResourceHelper.GetString("OOBEView_WelcomeDialog_Title"),
                Content = ResourceHelper.GetString("OOBEView_WelcomeDialog_Description"),
                CloseButtonText = ResourceHelper.GetString("OOBEView_WelcomeDialog_Close"),
                CloseButtonStyle = (Style)Application.Current.Resources["AccentButtonStyle"],
                XamlRoot = this.XamlRoot,
                RequestedTheme = this.RequestedTheme
            };

            _ = dialog.ShowAsync();
        }
    }
}
