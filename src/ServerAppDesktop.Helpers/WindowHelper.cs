using System;
using System.Runtime.InteropServices;

namespace ServerAppDesktop.Helpers
{
    public static partial class WindowHelper
    {
        [LibraryImport("user32.dll", SetLastError = true, EntryPoint = "SetForegroundWindow")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool SetForegroundWindow(IntPtr hWnd);

        [LibraryImport("user32.dll", SetLastError = true, EntryPoint = "ShowWindow")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool ShowWindow(IntPtr hWnd, int nCmdShow);
        private const int SW_RESTORE = 9;

        public static void FocusWindow(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero)
                return;

            ShowWindow(hwnd, SW_RESTORE);
            SetForegroundWindow(hwnd);
        }
    }
}