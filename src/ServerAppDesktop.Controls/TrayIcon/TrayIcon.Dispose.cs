namespace ServerAppDesktop.Controls;

public partial class TrayIcon : IDisposable
{
    private bool _disposed = false;

    ~TrayIcon()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual unsafe void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            ActualThemeChanged -= OnActualThemeChanged;
            Loaded -= OnLoaded;
            TrayDelegateHandler.Invoked -= HandleTrayEvents;
        }

        _ = PInvoke.Shell_NotifyIcon(NOTIFY_ICON_MESSAGE.NIM_DELETE, in nid);

        if (!_currentIcon.IsNull)
        {
            _ = PInvoke.DestroyIcon(_currentIcon);
            _currentIcon = HICON.Null;
        }

        if (_window != null)
        {
            try
            {
                var hwnd = (HWND)WindowNative.GetWindowHandle(_window);
                if (_subclassPtr != null)
                {
                    _ = PInvoke.RemoveWindowSubclass(hwnd, _subclassPtr, 101);
                }

                if (disposing)
                    _window.Close();
                _window = null;
            }
            catch { }
        }

        _disposed = true;
    }
}
