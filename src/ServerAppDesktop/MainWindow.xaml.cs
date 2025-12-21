using Microsoft.UI;
using Microsoft.UI.Windowing;
using ServerAppDesktop.Helpers;
using System;
using WinUIEx;

namespace ServerAppDesktop
{
    public sealed partial class MainWindow : WindowEx
    {
        public string WindowTitle => DataHelper.WindowTitle;
        public string WindowSubtitle => DataHelper.WindowSubtitle;

        public MainWindow()
        {
            InitializeComponent();
            ExtendsContentIntoTitleBar = true;

            IntPtr hwnd = WindowHelper.GetWindowHandle();
            if (hwnd != IntPtr.Zero)
            {
                var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
                var appWindow = AppWindow.GetFromWindowId(windowId);
                var titleBar = appWindow.TitleBar;
                titleBar.PreferredTheme = TitleBarTheme.UseDefaultAppMode;
                titleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
                appWindow.SetIcon("Assets/AppIcon.ico");
                appWindow.SetTaskbarIcon("Assets/AppIcon.ico");
                appWindow.SetTitleBarIcon("Assets/AppIcon.ico");
            }
        }
    }
}
