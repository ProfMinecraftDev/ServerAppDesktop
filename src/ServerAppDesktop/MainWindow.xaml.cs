using System;
using Microsoft.UI;
using Microsoft.UI.Windowing;
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
        private static MainWindow? _current;
        public static new MainWindow Current
        {
            get
            {
                if (_current == null)
                    _current = new MainWindow();

                return _current;
            }
        }

        public MainViewModel ViewModel => App.GetRequiredService<MainViewModel>();
        private bool CloseInSystemTray { get => DataHelper.Settings?.Startup.CloseInSystemTray ?? true; }

        private MainWindow()
        {
            InitializeComponent();
            this.CenterOnScreen();
            UpdateFullScreenUI(false);

            NetworkHelper.ConnectionChanged += async (isConnected) =>
            {
                DispatcherQueue.TryEnqueue(async () =>
                {
                    ViewModel.IsConnectedToInternet = isConnected;
                    if (!isConnected)
                    {
                        internetInfoBar.Title = ResourceHelper.GetString("InternetInfoBar_Warning_Title");
                        internetInfoBar.Message = ResourceHelper.GetString("InternetInfoBar_Warning_Message");
                        internetInfoBar.Severity = InfoBarSeverity.Warning;
                    }
                    else
                    {
                        internetInfoBar.Title = ResourceHelper.GetString("InternetInfoBar_Reconnected_Title");
                        internetInfoBar.Message = ResourceHelper.GetString("InternetInfoBar_Reconnected_Message");
                    }
                });
            };


            var grid = Content as Grid ?? throw new NullReferenceException("Grid no está o no se ha cargado.");
            grid.DataContext = ViewModel;
            grid.KeyDown += (_, e) => OnF11OrEscapeInvoked(e.Key);
            fullScreenButton.Click += (_, _) => OnF11OrEscapeInvoked(VirtualKey.F11);

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
                    if (CloseInSystemTray)
                    {
                        e.Cancel = true;
                        this.Hide();
                    }
                    else
                        TrayIcon.Dispose();
                };
            }
        }

        public void SetIcon(string iconPath)
        {
            IntPtr hwnd = this.GetWindowHandle();
            if (hwnd != IntPtr.Zero)
            {
                var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
                var appWindow = AppWindow.GetFromWindowId(windowId);
                appWindow.SetIcon(iconPath);
                appWindow.SetTaskbarIcon(iconPath);
                appWindow.SetTitleBarIcon(iconPath);
            }
        }

        public void SetIcon(IconId iconId)
        {
            IntPtr hwnd = this.GetWindowHandle();
            if (hwnd != IntPtr.Zero)
            {
                var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
                var appWindow = AppWindow.GetFromWindowId(windowId);
                appWindow.SetIcon(iconId);
                appWindow.SetTaskbarIcon(iconId);
                appWindow.SetTitleBarIcon(iconId);
            }
        }

        private void OnF11OrEscapeInvoked(VirtualKey vKey)
        {
            bool isFullScreen = PresenterKind == AppWindowPresenterKind.FullScreen;

            if (vKey == VirtualKey.F11)
            {
                UpdateFullScreenUI(!isFullScreen);
            }
            else if (vKey == VirtualKey.Escape && isFullScreen)
            {
                UpdateFullScreenUI(false);
            }
        }

        private void UpdateFullScreenUI(bool fullScreen)
        {
            if (fullScreen && PresenterKind != AppWindowPresenterKind.FullScreen)
            {
                PresenterKind = AppWindowPresenterKind.FullScreen;
                fullScreenButton.Content = new FontIcon
                {
                    FontSize = 12,
                    Glyph = "\uE92C"
                };
                ToolTipService.SetToolTip(fullScreenButton, "Salir de pantalla completa (ESC o F11)");
            }
            else
            {
                PresenterKind = AppWindowPresenterKind.Default;
                fullScreenButton.Content = new FontIcon
                {
                    FontSize = 12,
                    Glyph = "\uE92D"
                };
                ToolTipService.SetToolTip(fullScreenButton, "Pantalla completa (F11)");
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
    }
}
