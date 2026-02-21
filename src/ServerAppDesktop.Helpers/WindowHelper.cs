namespace ServerAppDesktop.Helpers;

public static class WindowHelper
{
    public static void FlashTaskbarIcon(Window? window)
    {
        if (window == null)
            return;

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
            return;

        var hWnd = new HWND(window.GetWindowHandle());
        H.NotifyIcon.WindowExtensions.Show(window, true);
        _ = PInvoke.SetForegroundWindow(hWnd);
        window.Activate();
    }

    public static async Task<bool> IsRedirectedAsync()
    {
        var args = AppInstance.GetCurrent().GetActivatedEventArgs();
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
            return;

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
            return;

        window.SystemBackdrop = index switch
        {
            0 => new MicaBackdrop { Kind = MicaKind.Base },
            1 => new MicaBackdrop { Kind = MicaKind.BaseAlt },
            2 => new DesktopAcrylicBackdrop(),
            _ => new MicaBackdrop { Kind = MicaKind.Base }
        };
    }
}
