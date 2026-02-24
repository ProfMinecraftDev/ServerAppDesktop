namespace ServerAppDesktop.Handlers;

public class WindowHandler : IWindowHandler
{
    private MainWindow? _window;
    private static bool CloseInSystemTray => DataHelper.Settings?.Startup.CloseInSystemTray ?? true;

    public bool WindowClosed { get; private set; } = false;

    public void SetWindow(MainWindow window)
    {
        _window = window;
    }

    public void Configure()
    {
        if (_window == null)
        {
            return;
        }

        _window.ExtendsContentIntoTitleBar = true;

        nint hwnd = _window.GetWindowHandle();
        if (hwnd == nint.Zero)
        {
            return;
        }

        WindowId windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
        var appWindow = AppWindow.GetFromWindowId(windowId);
        appWindow.TitleBar.PreferredTheme = TitleBarTheme.UseDefaultAppMode;
        appWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
        appWindow.TitleBar.ExtendsContentIntoTitleBar = true;
        appWindow.TitleBar.IconShowOptions = IconShowOptions.ShowIconAndSystemMenu;
        appWindow.TitleBar.BackgroundColor = Colors.Red;

        appWindow.SetIcon("Assets/AppIcon.ico");
        appWindow.SetTaskbarIcon("Assets/AppIcon.ico");
        appWindow.SetTitleBarIcon("Assets/AppIcon.ico");

        appWindow.Closing += (s, e) =>
        {
            e.Cancel = true;
            if (CloseInSystemTray)
            {
                H.NotifyIcon.WindowExtensions.Hide(_window, true);
            }
            else
            {
                WindowClosed = true;
                _window.TrayIcon.Dispose();
                e.Cancel = false;
            }
        };
    }

    public void SetBadgeIcon(string iconPath)
    {
        if (_window == null)
        {
            return;
        }
        if (string.IsNullOrEmpty(iconPath))
        {
            WindowHelper.ClearBadge(_window);
            return;
        }
        WindowHelper.SetBadge(_window, iconPath);
    }

    public void HandleNetworkUIUpdate(InfoBar infoBar, bool isConnected)
    {
        infoBar.Title = ResourceHelper.GetString(isConnected ? "InternetInfoBar_Reconnected_Title" : "InternetInfoBar_Warning_Title");
        infoBar.Message = ResourceHelper.GetString(isConnected ? "InternetInfoBar_Reconnected_Message" : "InternetInfoBar_Warning_Message");
        infoBar.Severity = isConnected ? InfoBarSeverity.Informational : InfoBarSeverity.Warning;
        infoBar.IsOpen = true;
    }

    public async Task UpdateFullScreenLogic(bool fullScreen, Button fsButton)
    {
        if (_window == null)
        {
            return;
        }

        if (fullScreen)
        {
            fsButton.Content = new FontIcon { FontSize = 12, Glyph = "\uE92C" };
            ToolTipService.SetToolTip(fsButton, "Salir de pantalla completa (ESC o F11)");
            _window.PresenterKind = AppWindowPresenterKind.FullScreen;

            await Task.Delay(1000);
            if (_window.PresenterKind == AppWindowPresenterKind.FullScreen && !_window.IsMouseOverTitleBar)
            {
                _window.TitleBar.Height = 2;
            }
        }
        else
        {
            fsButton.Content = new FontIcon { FontSize = 12, Glyph = "\uE92D" };
            ToolTipService.SetToolTip(fsButton, "Pantalla completa (F11)");
            _window.PresenterKind = AppWindowPresenterKind.Default;
            _window.TitleBar.Height = 48;
        }
    }
}
