namespace ServerAppDesktop.Controls;

public partial class TrayIcon : IDisposable
{
    public void Dispose()
    {
        if (!_disposed)
        {
            if (_window != null && _subclassDelegate != null)
            {
                try
                {
                    var hwnd = (HWND)WindowNative.GetWindowHandle(_window);
                    _ = PInvoke.RemoveWindowSubclass(hwnd, _subclassDelegate, 101);
                    _window.Close();
                    _window = null;
                }
                catch { }
            }

            _ = PInvoke.Shell_NotifyIcon(NOTIFY_ICON_MESSAGE.NIM_DELETE, in nid);

            if (!_currentIcon.IsNull)
            {
                _ = PInvoke.DestroyIcon(_currentIcon);
                _currentIcon = HICON.Null;
            }

            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
