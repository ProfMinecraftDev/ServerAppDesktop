namespace ServerAppDesktop.Controls;

public partial class TrayIcon : IDisposable
{
    private NOTIFYICONDATAW nid;
    private bool _disposed;
    private const int WM_TRAYICON = 0x0400 + 1;
    private HICON _currentIcon;
    private SUBCLASSPROC? _subclassDelegate;

    private unsafe void CreateTrayIconMenu()
    {
        var realHwnd = (HWND)WindowNative.GetWindowHandle(_window);

        nid = new NOTIFYICONDATAW
        {
            cbSize = sizeof(NOTIFYICONDATAW).To<uint>(),
            hWnd = realHwnd,
            uID = 1,
            uFlags = NOTIFY_ICON_DATA_FLAGS.NIF_MESSAGE,
            uCallbackMessage = WM_TRAYICON
        };

        _subclassDelegate = TrayWindowSubclassProc;
        _ = PInvoke.SetWindowSubclass(realHwnd, _subclassDelegate, 101, 0);

        UpdateToolTip(ToolTip);
        UpdateIcon();

        _ = PInvoke.Shell_NotifyIcon(NOTIFY_ICON_MESSAGE.NIM_ADD, in nid);
    }

    private LRESULT TrayWindowSubclassProc(HWND hwnd, uint msg, WPARAM wParam, LPARAM lParam, nuint id, nuint data)
    {
        try
        {
            if (msg == WM_TRAYICON)
            {
                uint message = (uint)((long)lParam.Value & 0xFFFF);

                switch (message)
                {
                    case PInvoke.WM_RBUTTONUP:
                        OnContextMenuRequested();
                        return new LRESULT(0);
                    case PInvoke.WM_LBUTTONUP:
                        OnClickRequested();
                        return new LRESULT(0);
                    case PInvoke.WM_LBUTTONDBLCLK:
                        OnDoubleClickRequested();
                        return new LRESULT(0);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in TrayWindowSubclassProc: {ex}");
        }

        return PInvoke.DefSubclassProc(hwnd, msg, wParam, lParam);
    }

    partial void OnContextMenuRequested()
    {
        RightClickCommand?.Execute(RightClickCommandParameter);
        RightClick?.Invoke(this, new RoutedEventArgs());

        if (ContextFlyout is not MenuFlyout originalMenu || _window == null || _frame == null || _isMenuOpen)
        {
            return;
        }

        try
        {
            _ = PInvoke.GetCursorPos(out System.Drawing.Point pt);

            var hwnd = (HWND)WindowNative.GetWindowHandle(_window);

            _ = PInvoke.SetWindowPos(hwnd, HWND.Null, pt.X, pt.Y, 1, 1,
                SET_WINDOW_POS_FLAGS.SWP_NOZORDER);
            _window.AppWindow.Show();

            _ = PInvoke.SetForegroundWindow(hwnd);

            MenuFlyout clonedMenu = CloneMenuFlyout(originalMenu);
            clonedMenu.Closed += OnContextMenuClosed;

            clonedMenu.ShowAt(_frame);
            _isMenuOpen = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error showing context menu: {ex.Message}");
            HideMenu();
        }
    }

    partial void OnDoubleClickRequested()
    {
        DoubleClickCommand?.Execute(DoubleClickCommandParameter);
        DoubleClick?.Invoke(this, new RoutedEventArgs());
    }
}
