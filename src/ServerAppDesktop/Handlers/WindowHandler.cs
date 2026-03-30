namespace ServerAppDesktop.Handlers;

public class WindowHandler : IWindowHandler
{
    private MainWindow? _window;
    private readonly DispatcherTimer _timer;
    private readonly IProcessService _processService;
    private static bool CloseInSystemTray => DataHelper.Settings?.Startup.CloseInSystemTray ?? true;
    public bool WindowHidden { get; set; } = false;

    public bool WindowClosed { get; private set; } = false;

    public WindowHandler(IProcessService processService)
    {
        _processService = processService;
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
    }

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

        appWindow.SetIcon("Assets/AppIcon.ico");
        appWindow.SetTaskbarIcon("Assets/AppIcon.ico");
        appWindow.SetTitleBarIcon("Assets/AppIcon.ico");

        appWindow.Closing += (s, e) =>
        {
            e.Cancel = true;
            if (CloseInSystemTray)
            {
                WindowHidden = true;
                _window.Hide();
                if (!_processService.IsRunning)
                    ProcessHelper.SetEfficiencyMode(true);
            }
            else
            {
                WindowClosed = true;
                _window.TrayIcon.Dispose();
                e.Cancel = false;
            }
        };

        _window.WindowStateChanged += (_, s) =>
        {
            if (_processService.IsRunning)
                return;

            if (s == WindowState.Minimized)
            {
                ProcessHelper.SetProcessQualityOfServiceLevel(QualityOfServiceLevel.Low);
                ProcessHelper.SetProcessPriorityClass(ProcessPriorityClass.BelowNormal);
            }
            else
            {
                ProcessHelper.SetProcessQualityOfServiceLevel(QualityOfServiceLevel.Default);
                ProcessHelper.SetProcessPriorityClass(ProcessPriorityClass.Normal);
            }
        };

        _timer.Tick += (_, _) =>
        {
            if (!WindowHidden)
            {
                SavePersistence();
            }
        };
        _timer.Start();
    }

    private unsafe void SavePersistence()
    {
        if (_window == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(_window.PersistenceId))
        {
            IDictionary<string, object>? winuiExSettings = WindowManager.PersistenceStorage;
            if (winuiExSettings is not null)
            {
                using var data = new System.IO.MemoryStream();
                using var sw = new System.IO.BinaryWriter(data);
                IList<MonitorInfo> monitors = MonitorInfo.GetDisplayMonitors();
                sw.Write(monitors.Count);
                foreach (MonitorInfo monitor in monitors)
                {
                    sw.Write(monitor.Name);
                    sw.Write(monitor.RectMonitor.Left);
                    sw.Write(monitor.RectMonitor.Top);
                    sw.Write(monitor.RectMonitor.Right);
                    sw.Write(monitor.RectMonitor.Bottom);
                }
                var placement = new WINDOWPLACEMENT();
                _ = PInvoke.GetWindowPlacement(new Windows.Win32.Foundation.HWND(_window.GetWindowHandle()), ref placement);

                int structSize = sizeof(WINDOWPLACEMENT);
                IntPtr buffer = Marshal.AllocHGlobal(structSize);
                *(WINDOWPLACEMENT*)buffer = placement;
                byte[] placementData = new byte[structSize];
                Marshal.Copy(buffer, placementData, 0, structSize);
                Marshal.FreeHGlobal(buffer);
                sw.Write(placementData);
                sw.Flush();
                winuiExSettings[$"WindowPersistance_{_window.PersistenceId}"] = Convert.ToBase64String(data.ToArray());
            }
        }
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
            return;

        if (fullScreen)
        {
            fsButton.Content = new FontIcon { FontSize = 12, Glyph = "\uE92C" };
            ToolTipService.SetToolTip(fsButton, ResourceHelper.GetString("Window_FullScreen_Exit"));

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
            ToolTipService.SetToolTip(fsButton, ResourceHelper.GetString("Window_FullScreen_Enter"));

            _window.PresenterKind = AppWindowPresenterKind.Overlapped;
            _window.TitleBar.Height = 48;
        }
    }

    public unsafe void LoadPersistence()
    {
        if (_window == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(_window.PersistenceId))
        {
            try
            {
                IDictionary<string, object>? winuiExSettings = WindowManager.PersistenceStorage;
                if (winuiExSettings is null)
                {
                    return;
                }

                byte[]? data = null;
                if (winuiExSettings.ContainsKey($"WindowPersistance_{_window.PersistenceId}"))
                {
                    if (winuiExSettings[$"WindowPersistance_{_window.PersistenceId}"] is string base64)
                    {
                        data = Convert.FromBase64String(base64);
                    }
                }
                if (data is null)
                {
                    return;
                }
                IList<MonitorInfo> monitors = MonitorInfo.GetDisplayMonitors();
                var br = new System.IO.BinaryReader(new System.IO.MemoryStream(data));
                int monitorCount = br.ReadInt32();
                if (monitorCount != monitors.Count)
                {
                    return;
                }

                for (int i = 0; i < monitorCount; i++)
                {
                    MonitorInfo pMonitor = monitors[i];
                    _ = br.ReadString();
                    if (pMonitor.RectMonitor.Left != br.ReadDouble() ||
                        pMonitor.RectMonitor.Top != br.ReadDouble() ||
                        pMonitor.RectMonitor.Right != br.ReadDouble() ||
                        pMonitor.RectMonitor.Bottom != br.ReadDouble())
                    {
                        return;
                    }
                }
                int structSize = sizeof(WINDOWPLACEMENT);
                byte[] placementData = br.ReadBytes(structSize);
                IntPtr buffer = Marshal.AllocHGlobal(structSize);
                Marshal.Copy(placementData, 0, buffer, structSize);
                WINDOWPLACEMENT retobj = (*(WINDOWPLACEMENT*)buffer)!;
                Marshal.FreeHGlobal(buffer);
                if (retobj.showCmd == SHOW_WINDOW_CMD.SW_SHOWMINIMIZED && retobj.flags == WINDOWPLACEMENT_FLAGS.WPF_RESTORETOMAXIMIZED)
                {
                    retobj.showCmd = SHOW_WINDOW_CMD.SW_MAXIMIZE;
                }
                else if (retobj.showCmd != SHOW_WINDOW_CMD.SW_MAXIMIZE)
                {
                    retobj.showCmd = SHOW_WINDOW_CMD.SW_NORMAL;
                }

                _ = PInvoke.SetWindowPlacement(new HWND(_window.GetWindowHandle()), in retobj);
            }
            catch { }
        }
    }
}
