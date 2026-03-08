namespace ServerAppDesktop.Controls;

public unsafe partial class TrayIcon
{
    private NOTIFYICONDATAW nid;
    private const int WM_TRAYICON = 0x0400 + 1;
    private HICON _currentIcon;
    private delegate* unmanaged[Stdcall]<HWND, uint, WPARAM, LPARAM, nuint, nuint, LRESULT> _subclassPtr;

    private void CreateTrayIconMenu()
    {
        var realHwnd = (HWND)Win32Interop.GetWindowFromWindowId(XamlRoot.ContentIslandEnvironment.AppWindowId);


        var trayGuid = new Guid("A1B2C3D4-E5F6-4A5B-8C9D-0E1F2A3B4C5D");

        nid = new NOTIFYICONDATAW
        {
            cbSize = (uint)sizeof(NOTIFYICONDATAW),
            hWnd = realHwnd,
            uID = (uint)Environment.ProcessPath!.GetHashCode(),
            uFlags = NOTIFY_ICON_DATA_FLAGS.NIF_MESSAGE |
                     NOTIFY_ICON_DATA_FLAGS.NIF_GUID |
                     NOTIFY_ICON_DATA_FLAGS.NIF_SHOWTIP,
            uCallbackMessage = WM_TRAYICON,
            guidItem = trayGuid
        };

        _subclassPtr = &TrayDelegateHandler.TrayWindowSubclassProc;
        TrayDelegateHandler.Invoked += HandleTrayEvents;
        _ = PInvoke.SetWindowSubclass(realHwnd, _subclassPtr, 101, 0);

        UpdateToolTip();
        UpdateIcon();

        _ = PInvoke.Shell_NotifyIcon(NOTIFY_ICON_MESSAGE.NIM_ADD, in nid);
    }

    private void HandleTrayEvents(HWND hwnd, uint msg, WPARAM wParam, LPARAM lParam, nuint id, nuint data)
    {
        try
        {
            if (msg == WM_TRAYICON)
            {
                uint message = (uint)((long)lParam.Value & 0xFFFF);

                switch (message)
                {
                    case PInvoke.WM_LBUTTONUP:
                        _window?.DispatcherQueue.TryEnqueue(OnClickRequested);
                        return;
                    case PInvoke.WM_RBUTTONUP:
                        _window?.DispatcherQueue.TryEnqueue(OnContextMenuRequested);
                        return;
                    case PInvoke.WM_LBUTTONDBLCLK:
                        _window?.DispatcherQueue.TryEnqueue(OnDoubleClickRequested);
                        return;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in TrayWindowSubclassProc: {ex}");
        }
    }

    partial void OnContextMenuRequested()
    {
        XamlRoot?.Content?.DispatcherQueue?.TryEnqueue(() =>
        {
            RightClickCommand?.Execute(RightClickCommandParameter);
            RightClick?.Invoke(this, new RoutedEventArgs());
        });

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

            if (clonedMenu.Placement == FlyoutPlacementMode.Auto)
                clonedMenu.Placement = default;
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
        XamlRoot?.Content?.DispatcherQueue?.TryEnqueue(() =>
        {
            DoubleClickCommand?.Execute(DoubleClickCommandParameter);
            DoubleClick?.Invoke(this, new RoutedEventArgs());
        });
    }
}
