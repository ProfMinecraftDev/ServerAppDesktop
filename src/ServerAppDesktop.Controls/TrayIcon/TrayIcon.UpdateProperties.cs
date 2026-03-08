namespace ServerAppDesktop.Controls;

public partial class TrayIcon
{
    private void OnActualThemeChanged(FrameworkElement sender, object args)
    {
        _frame.RequestedTheme = ActualTheme;
    }

    private void OnVisibilityChanged(DependencyObject sender, DependencyProperty dp)
    {
        OnVisibilityChanged(Visibility);
    }

    private void OnContextMenuClosed(object? sender, object e)
    {
        if (sender is MenuFlyout flyout)
        {
            flyout.Closed -= OnContextMenuClosed;
        }
        HideMenu();
    }

    partial void OnVisibilityChanged(Visibility value)
    {
        if (value == Visibility.Visible)
        {
            nid.uFlags |= NOTIFY_ICON_DATA_FLAGS.NIF_MESSAGE |
                          NOTIFY_ICON_DATA_FLAGS.NIF_ICON |
                          NOTIFY_ICON_DATA_FLAGS.NIF_TIP;

            UpdateToolTip();
            UpdateIcon();

            _ = PInvoke.Shell_NotifyIcon(NOTIFY_ICON_MESSAGE.NIM_ADD, in nid);
            _ = PInvoke.Shell_NotifyIcon(NOTIFY_ICON_MESSAGE.NIM_MODIFY, in nid);
        }
        else
        {
            _ = PInvoke.Shell_NotifyIcon(NOTIFY_ICON_MESSAGE.NIM_DELETE, in nid);
        }
    }

    partial void OnToolTipChanged(string value)
    {
        UpdateToolTip();
        _ = PInvoke.Shell_NotifyIcon(NOTIFY_ICON_MESSAGE.NIM_MODIFY, in nid);
    }

    private unsafe void UpdateToolTip()
    {
        if (string.IsNullOrEmpty(ToolTip))
        {
            nid.uFlags &= ~NOTIFY_ICON_DATA_FLAGS.NIF_TIP;
            return;
        }

        nid.uFlags |= NOTIFY_ICON_DATA_FLAGS.NIF_TIP;
        fixed (char* pTip = ToolTip)
        fixed (NOTIFYICONDATAW* pNid = &nid)
        {
            int length = Math.Min(ToolTip.Length, 127);
            Marshal.Copy(ToolTip.ToCharArray(), 0, (IntPtr)pNid->szTip.Value, length);
            pNid->szTip.Value[length] = '\0';
        }
    }

    partial void OnIconSourceChanged(ImageSource? value)
    {
        UpdateIcon();
        _ = PInvoke.Shell_NotifyIcon(NOTIFY_ICON_MESSAGE.NIM_MODIFY, in nid);
    }

    private async void UpdateIcon()
    {
        if (!_currentIcon.IsNull)
        {
            _ = PInvoke.DestroyIcon(_currentIcon);
            _currentIcon = HICON.Null;
        }

        if (IconSource == null)
        {
            nid.uFlags &= ~NOTIFY_ICON_DATA_FLAGS.NIF_ICON;
            _ = PInvoke.Shell_NotifyIcon(NOTIFY_ICON_MESSAGE.NIM_MODIFY, in nid);
            return;
        }

        try
        {
            using Icon icon = await IconSource.ToIconAsync(true);
            var hIcon = (HICON)icon.Handle;

            if (!hIcon.IsNull)
            {
                _currentIcon = hIcon;
                nid.hIcon = hIcon;
                nid.uFlags |= NOTIFY_ICON_DATA_FLAGS.NIF_ICON;

                _ = PInvoke.Shell_NotifyIcon(NOTIFY_ICON_MESSAGE.NIM_MODIFY, in nid);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading icon: {ex.Message}");
        }
    }
}
