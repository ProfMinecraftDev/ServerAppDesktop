namespace ServerAppDesktop.Controls;

public partial class TrayIcon
{
    private Window? _window;
    private readonly Frame _frame = new() { Background = new SolidColorBrush(Colors.Transparent) };
    private bool _isMenuOpen;

    private void CreateWindow()
    {
        if (_window != null)
        {
            return;
        }

        _window = new Window { Content = _frame, SystemBackdrop = new DesktopAcrylicBackdrop() };

        _window.AppWindow.SetPresenter(AppWindowPresenterKind.Overlapped);
        _window.AppWindow.Presenter.To<OverlappedPresenter>().IsAlwaysOnTop = true;

        _window.AppWindow.IsShownInSwitchers = false;

        var hwnd = (HWND)WindowNative.GetWindowHandle(_window);

        uint style = PInvoke.GetWindowLong(hwnd, WINDOW_LONG_PTR_INDEX.GWL_STYLE).To<uint>();
        style &= ~(WINDOW_STYLE.WS_CAPTION | WINDOW_STYLE.WS_THICKFRAME | WINDOW_STYLE.WS_SYSMENU).To<uint>();
        style |= WINDOW_STYLE.WS_POPUP.To<uint>();
        _ = PInvoke.SetWindowLong(hwnd, WINDOW_LONG_PTR_INDEX.GWL_STYLE, (int)style);

        int exStyle = PInvoke.GetWindowLong(hwnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE);
        exStyle |= (int)(WINDOW_EX_STYLE.WS_EX_LAYERED | WINDOW_EX_STYLE.WS_EX_TOOLWINDOW | WINDOW_EX_STYLE.WS_EX_TRANSPARENT);
        _ = PInvoke.SetWindowLong(hwnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE, exStyle);

        unsafe
        {
            MARGINS margins = new() { cxLeftWidth = -1, cxRightWidth = -1, cyTopHeight = -1, cyBottomHeight = -1 };
            _ = PInvoke.DwmExtendFrameIntoClientArea(hwnd, &margins);
        }

        _ = PInvoke.SetLayeredWindowAttributes(
            hwnd,
            new COLORREF(0x00000000),
            0,
            LAYERED_WINDOW_ATTRIBUTES_FLAGS.LWA_COLORKEY | LAYERED_WINDOW_ATTRIBUTES_FLAGS.LWA_ALPHA);

        _ = PInvoke.SetWindowPos(hwnd, HWND.Null, 0, 0, 0, 0,
            SET_WINDOW_POS_FLAGS.SWP_FRAMECHANGED | SET_WINDOW_POS_FLAGS.SWP_NOMOVE |
            SET_WINDOW_POS_FLAGS.SWP_NOSIZE | SET_WINDOW_POS_FLAGS.SWP_NOZORDER |
            SET_WINDOW_POS_FLAGS.SWP_HIDEWINDOW);

        CreateTrayIconMenu();
    }

    private MenuFlyout CloneMenuFlyout(MenuFlyout original)
    {
        MenuFlyout clone = new()
        {
            XamlRoot = _frame.XamlRoot,
            AreOpenCloseAnimationsEnabled = original.AreOpenCloseAnimationsEnabled,
            Placement = original.Placement,
            ShowMode = original.ShowMode,
            MenuFlyoutPresenterStyle = original.MenuFlyoutPresenterStyle,
            SystemBackdrop = original.SystemBackdrop,
            AllowFocusOnInteraction = original.AllowFocusOnInteraction,
            AllowFocusWhenDisabled = original.AllowFocusWhenDisabled,
            ElementSoundMode = original.ElementSoundMode,
            LightDismissOverlayMode = original.LightDismissOverlayMode,
            OverlayInputPassThroughElement = original.OverlayInputPassThroughElement,
            ShouldConstrainToRootBounds = original.ShouldConstrainToRootBounds
        };

        foreach (MenuFlyoutItemBase item in original.Items)
        {
            clone.Items.Add(item);
        }

        return clone;
    }

    partial void OnClickRequested()
    {
        XamlRoot?.Content?.DispatcherQueue?.TryEnqueue(() =>
        {
            LeftClickCommand?.Execute(LeftClickCommandParameter);
            Click?.Invoke(this, new RoutedEventArgs());
        });
    }

    private void HideMenu()
    {
        _window?.AppWindow.Hide();
        _isMenuOpen = false;
    }
}
