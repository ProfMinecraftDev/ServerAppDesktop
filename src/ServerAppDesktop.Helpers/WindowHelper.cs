namespace ServerAppDesktop.Helpers;

public static class WindowHelper
{
    private static string? _lastIconPath = null;
    public static void FlashTaskbarIcon(Window? window)
    {
        if (window == null)
        {
            return;
        }

        FLASHWINFO flashInfo = new()
        {
            cbSize = Marshal.SizeOf<FLASHWINFO>().To<uint>(),
            hwnd = new HWND(window.GetWindowHandle()),
            dwFlags = FLASHWINFO_FLAGS.FLASHW_ALL | FLASHWINFO_FLAGS.FLASHW_TIMERNOFG | FLASHWINFO_FLAGS.FLASHW_TRAY,
            uCount = uint.MaxValue,
            dwTimeout = 0
        };
        _ = PInvoke.FlashWindowEx(flashInfo);
    }

    public static void ShowAndFocus(Window? window)
    {
        if (window == null)
        {
            return;
        }

        var hWnd = new HWND(window.GetWindowHandle());
        H.NotifyIcon.WindowExtensions.Show(window, true);
        _ = PInvoke.SetForegroundWindow(hWnd);
        if (!string.IsNullOrWhiteSpace(_lastIconPath))
        {
            SetBadge(window, _lastIconPath);
        }
        window.Activate();
    }

    public static async Task<bool> IsRedirectedAsync()
    {
        AppActivationArguments args = AppInstance.GetCurrent().GetActivatedEventArgs();
        var existingInstance = AppInstance.FindOrRegisterForKey(DataHelper.WindowIdentifier);

        if (!existingInstance.IsCurrent)
        {
            await existingInstance.RedirectActivationToAsync(args);
            return true;
        }

        return false;
    }

    public static void SetTheme(Window? window, ElementTheme theme)
    {
        if (window?.Content is not FrameworkElement content)
        {
            return;
        }

        content.RequestedTheme = theme;
        window.AppWindow.TitleBar.PreferredTheme = theme switch
        {
            ElementTheme.Light => TitleBarTheme.Light,
            ElementTheme.Dark => TitleBarTheme.Dark,
            _ => TitleBarTheme.UseDefaultAppMode
        };
    }

    public static void SetSystemBackdrop(Window? window, int index)
    {
        if (window == null)
        {
            return;
        }

        window.SystemBackdrop = index switch
        {
            0 => new MicaBackdrop { Kind = MicaKind.Base },
            1 => new MicaBackdrop { Kind = MicaKind.BaseAlt },
            2 => new DesktopAcrylicBackdrop(),
            _ => new MicaBackdrop { Kind = MicaKind.Base }
        };
    }

    public static unsafe void SetBadge(Window window, string? iconPath)
    {
        if (string.IsNullOrWhiteSpace(iconPath))
        {
            return;
        }

        if (!File.Exists(iconPath))
        {
            return;
        }

        if (iconPath != _lastIconPath)
        {
            _lastIconPath = iconPath;
        }

        var icon = new System.Drawing.Icon(iconPath);

        HRESULT hr = PInvoke.CoCreateInstance(
            typeof(TaskbarList).GUID,
            null,
            Windows.Win32.System.Com.CLSCTX.CLSCTX_INPROC_SERVER,
            out ITaskbarList3* taskbarList
            );

        if (hr.Succeeded)
        {
            taskbarList->HrInit();
            var safeHIcon = new SafeFileHandle(icon.Handle, ownsHandle: false);
            taskbarList->SetOverlayIcon(new HWND(window.GetWindowHandle()), safeHIcon, "Badge");
            _ = taskbarList->Release();
        }
    }

    public static unsafe void ClearBadge(Window window)
    {
        HRESULT hr = PInvoke.CoCreateInstance(
            typeof(TaskbarList).GUID,
            null,
            Windows.Win32.System.Com.CLSCTX.CLSCTX_INPROC_SERVER,
            out ITaskbarList3* taskbarList
            );

        if (hr.Succeeded)
        {
            taskbarList->HrInit();
            taskbarList->SetOverlayIcon(new HWND(window.GetWindowHandle()), new SafeFileHandle(nint.Zero, ownsHandle: false), null);
            _ = taskbarList->Release();
        }
    }
}
