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
        NOTIFY_ICON_MESSAGE message = value == Visibility.Visible
            ? NOTIFY_ICON_MESSAGE.NIM_ADD
            : NOTIFY_ICON_MESSAGE.NIM_DELETE;

        _ = PInvoke.Shell_NotifyIcon(message, in nid);
        if (message == NOTIFY_ICON_MESSAGE.NIM_ADD)
        {
            UpdateToolTip(ToolTip);
            UpdateIcon();
        }
    }

    partial void OnToolTipChanged(string value)
    {
        UpdateToolTip(value);
        _ = PInvoke.Shell_NotifyIcon(NOTIFY_ICON_MESSAGE.NIM_MODIFY, in nid);
    }

    private unsafe void UpdateToolTip(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            nid.uFlags &= ~NOTIFY_ICON_DATA_FLAGS.NIF_TIP;
            return;
        }

        nid.uFlags |= NOTIFY_ICON_DATA_FLAGS.NIF_TIP;
        fixed (char* pTip = value)
        fixed (NOTIFYICONDATAW* pNid = &nid)
        {
            int length = Math.Min(value.Length, 127);
            Marshal.Copy(value.ToCharArray(), 0, (IntPtr)pNid->szTip.Value, length);
            pNid->szTip.Value[length] = '\0';
        }
    }

    partial void OnIconSourceChanged(ImageSource? value)
    {
        UpdateIcon();
        _ = PInvoke.Shell_NotifyIcon(NOTIFY_ICON_MESSAGE.NIM_MODIFY, in nid);
    }

    private unsafe void UpdateIcon()
    {
        if (!_currentIcon.IsNull)
        {
            _ = PInvoke.DestroyIcon(_currentIcon);
            _currentIcon = HICON.Null;
        }

        if (IconSource == null)
        {
            nid.uFlags &= ~NOTIFY_ICON_DATA_FLAGS.NIF_ICON;
            return;
        }

        try
        {
            string iconPath = GetIconPath();
            if (File.Exists(iconPath))
            {
                int sizeW = PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CXSMICON);
                int sizeH = PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CYSMICON);

                var icon = new Icon(iconPath, sizeW, sizeH);
                var hIcon = (HICON)icon.Handle;

                if (!hIcon.IsNull)
                {
                    _currentIcon = hIcon;
                    nid.hIcon = hIcon;
                    nid.uFlags |= NOTIFY_ICON_DATA_FLAGS.NIF_ICON;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading icon: {ex.Message}");
        }
    }

    private string GetIconPath()
    {
        if (IconSource is BitmapImage bitmapImage && bitmapImage.UriSource != null)
        {
            string basePath = bitmapImage.UriSource.Scheme switch
            {
                "ms-appx" or "ms-appx-web" => AppContext.BaseDirectory,
                _ => string.Empty
            };
            return Path.Combine(basePath, bitmapImage.UriSource.LocalPath.TrimStart('/'));
        }
        return string.Empty;
    }

    private void OnActivated(object sender, WindowActivatedEventArgs args)
    {
        if (args.WindowActivationState == WindowActivationState.Deactivated && _isMenuOpen)
        {
            HideMenu();
        }
    }
}
