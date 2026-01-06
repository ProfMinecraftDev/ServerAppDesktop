using System;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ServerAppDesktop.Helpers;
using ServerAppDesktop.Services;
using ServerAppDesktop.ViewModels;
using Windows.System;
using WinUIEx;

namespace ServerAppDesktop
{
    public sealed partial class MainWindow : WindowEx
    {
        public MainViewModel ViewModel => App.GetRequiredService<MainViewModel>();

        public MainWindow()
        {
            InitializeComponent();
            this.CenterOnScreen();

            var grid = Content as Grid ?? throw new NullReferenceException("Grid no está o no se ha cargado.");
            grid.DataContext = ViewModel;
            grid.KeyDown += (s, e) =>
            {
                var vKey = e.Key;
                if (vKey == VirtualKey.Escape && PresenterKind == AppWindowPresenterKind.FullScreen)
                {
                    PresenterKind = AppWindowPresenterKind.Default;
                    return;
                }

                if (vKey == VirtualKey.F11)
                {
                    OnF11Invoked();
                    return;
                }
            };

            ExtendsContentIntoTitleBar = true;
            IntPtr hwnd = this.GetWindowHandle();
            if (hwnd != IntPtr.Zero)
            {
                var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
                var appWindow = AppWindow.GetFromWindowId(windowId);
                var titleBar = appWindow.TitleBar;
                titleBar.PreferredTheme = TitleBarTheme.UseDefaultAppMode;
                titleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
                appWindow.SetIcon("Assets/AppIcon.ico");
                appWindow.SetTaskbarIcon("Assets/AppIcon.ico");
                appWindow.SetTitleBarIcon("Assets/AppIcon.ico");
                appWindow.Closing += (_, e) =>
                {
                    e.Cancel = true;
                    this.Hide();
                };
            }
        }

        private async void reconnectButton_Click(object sender, RoutedEventArgs e)
        {
            reconnectButton.IsEnabled = false;
            noInternetInfoBar.Severity = InfoBarSeverity.Informational;
            noInternetInfoBar.IsClosable = false;
            noInternetInfoBar.Title = ResourceHelper.GetString("InternetConnection_Reconnecting_Title");
            noInternetInfoBar.Message = ResourceHelper.GetString("InternetConnection_Reconnecting_Message");

            bool result = await NetworkHelper.IsInternetAvailableAsync();

            if (result)
            {
                noInternetInfoBar.IsClosable = false;
                reconnectButton.Visibility = Visibility.Collapsed;
                noInternetInfoBar.Title = ResourceHelper.GetString("InternetConnection_Reconnected_Title");
                noInternetInfoBar.Message = ResourceHelper.GetString("InternetConnection_Reconnected_Message");
                noInternetInfoBar.Severity = InfoBarSeverity.Success;
                await Task.Delay(3000);
                ViewModel.IsConnectedToInternet = true;
            }
            else
            {
                noInternetInfoBar.IsClosable = true;
                noInternetInfoBar.Title = ResourceHelper.GetString("InternetConnection_FailedToReconnect_Title");
                noInternetInfoBar.Message = ResourceHelper.GetString("InternetConnection_FailedToReconnect_Message");
                noInternetInfoBar.Severity = InfoBarSeverity.Error;
                reconnectButton.IsEnabled = true;
                reconnectButton.Style = (Style)Application.Current.Resources["CriticalButtonStyle"];
            }
        }

        private void TitleBar_BackRequested(Microsoft.UI.Xaml.Controls.TitleBar sender, object args)
        {
            if (ViewModel.CanGoBack)
            {
                var navigationService = App.GetRequiredService<INavigationService>();
                navigationService.GoBack();
            }
        }

        private void OnF11Invoked()
        {
            if (PresenterKind != AppWindowPresenterKind.FullScreen)
            {
                PresenterKind = AppWindowPresenterKind.FullScreen;
                return;
            }
            else if (PresenterKind == AppWindowPresenterKind.FullScreen)
            {
                PresenterKind = AppWindowPresenterKind.Default;
                return;
            }
        }
    }
}
