using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

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

        public static void EnsureSingleInstance()
        {
            DataHelper.AppMutex = new Mutex(true, DataHelper.MutexIdentifier, out bool createdNew);
            if (!createdNew)
            {
                var current = Process.GetCurrentProcess();
                foreach (var process in Process.GetProcessesByName(current.ProcessName))
                {
                    if (process.Id != current.Id)
                    {
                        IntPtr hWnd = process.MainWindowHandle;
                        if (hWnd != IntPtr.Zero)
                        {
                            FocusWindow(hWnd);
                        }
                        break;
                    }
                }
                current.Kill();
            }
        }

        public static void FocusWindow(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero)
                return;

            ShowWindow(hwnd, SW_RESTORE);
            SetForegroundWindow(hwnd);
        }
    }
}