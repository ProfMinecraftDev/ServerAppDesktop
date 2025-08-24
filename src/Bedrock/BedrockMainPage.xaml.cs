using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using ServerAppDesktop.Bedrock.Pages;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ServerAppDesktop.Bedrock
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BedrockMainPage : Page
    {
        public BedrockMainPage()
        {
            InitializeComponent();
            App.Window.Title = "Server App Desktop (Preview) - Bedrock Edition";
            Loaded += BedrockMainPage_Loaded;
        }

        private void BedrockMainPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Si el frame está vacío, navegar a la página inicial
            if (NavigateFrame.Content == null)
            {
                NavigateFrame.Navigate(typeof(HomePage));
            }
        }

        private void NavigationView_SelectionChanged(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewSelectionChangedEventArgs args)
        {
            var item = args.SelectedItem as NavigationViewItem;
            var pageTag = item?.Tag?.ToString();

            switch (pageTag)
            {
                case "HomePage":
                    NavigateToPage(typeof(HomePage));
                    break;
                case "FilesPage":
                    NavigateToPage(typeof(FilesPage));
                    break;
                case "AboutPage":
                    NavigateToPage(typeof(AboutPage));
                    break;
            }

        }

        private void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                NavigateToPage(typeof(SettingsPage));
            }
        }

        private void NavigateToPage(Type page)
        {
            NavigateFrame.Navigate(page);
        }
    }
}
