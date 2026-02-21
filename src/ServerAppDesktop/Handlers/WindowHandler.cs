namespace ServerAppDesktop.Handlers;

public class WindowHandler : IWindowHandler
{
    private bool _windowClosed = false;
    private MainWindow? _window;
    private static bool CloseInSystemTray => DataHelper.Settings?.Startup.CloseInSystemTray ?? true;

    public bool WindowClosed => _windowClosed;

    public void SetWindow(MainWindow window)
    {
        _window = window;
    }

    public void Configure()
    {
        if (_window == null)
            return;
        _window.ExtendsContentIntoTitleBar = true;

        var hwnd = _window.GetWindowHandle();
        if (hwnd == nint.Zero)
            return;

        var appWindow = _window.AppWindow;
        appWindow.TitleBar.PreferredTheme = TitleBarTheme.UseDefaultAppMode;
        appWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
        appWindow.TitleBar.IconShowOptions = IconShowOptions.ShowIconAndSystemMenu;

        SetIcon("Assets/AppIcon.ico");

        appWindow.Closing += (s, e) =>
        {
            e.Cancel = true;
            if (CloseInSystemTray)
                H.NotifyIcon.WindowExtensions.Hide(_window, true);
            else
            {
                _windowClosed = true;
                _window.TrayIcon.Dispose();
                e.Cancel = false;
            }
        };
    }

    public void SetIcon(string iconPath) => _window?.AppWindow.SetIcon(iconPath);

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
            return;
        if (fullScreen)
        {
            _window.PresenterKind = AppWindowPresenterKind.FullScreen;
            fsButton.Content = new FontIcon { FontSize = 12, Glyph = "\uE92C" };
            ToolTipService.SetToolTip(fsButton, "Salir de pantalla completa (ESC o F11)");

            await Task.Delay(1000);
            if (_window.PresenterKind == AppWindowPresenterKind.FullScreen && !_window.IsMouseOverTitleBar)
            {
                _window.TitleBar.Height = 2;
            }
        }
        else
        {
            _window.PresenterKind = AppWindowPresenterKind.Default;
            fsButton.Content = new FontIcon { FontSize = 12, Glyph = "\uE92D" };
            ToolTipService.SetToolTip(fsButton, "Pantalla completa (F11)");
            _window.TitleBar.Height = 48;
        }
    }
}
