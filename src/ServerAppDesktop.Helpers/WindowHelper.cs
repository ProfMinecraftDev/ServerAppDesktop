using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace ServerAppDesktop.Helpers
{
    public static partial class WindowHelper
    {
        private const int SW_RESTORE = 9;
        private const int SW_SHOW = 5;

        [LibraryImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool SetForegroundWindow(IntPtr hWnd);

        [LibraryImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [LibraryImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool IsIconic(IntPtr hWnd);

        [LibraryImport("user32.dll")]
        public static partial IntPtr GetForegroundWindow();

        [LibraryImport("user32.dll")]
        public static partial uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [LibraryImport("kernel32.dll")]
        public static partial uint GetCurrentThreadId();

        [LibraryImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool AttachThreadInput(uint idAttach, uint idAttachTo, [MarshalAs(UnmanagedType.Bool)] bool fAttach);

        [LibraryImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool BringWindowToTop(IntPtr hWnd);

        [LibraryImport("user32.dll")]
        public static partial IntPtr SetFocus(IntPtr hWnd);

        private static void ThrowIfFailed(bool result, string apiName)
        {
            if (result)
                return;
            int err = Marshal.GetLastPInvokeError();
            throw new Win32Exception(err, $"{apiName} failed (error {err}).");
        }

        public static void EnsureSingleInstance()
        {
            DataHelper.AppMutex = new Mutex(true, DataHelper.MutexIdentifier, out bool createdNew);
            if (!createdNew)
            {
                var current = Process.GetCurrentProcess();
                foreach (var process in Process.GetProcessesByName(current.ProcessName))
                {
                    if (process.Id == current.Id)
                        continue;

                    process.Refresh();

                    IntPtr hWnd = process.MainWindowHandle;
                    if (hWnd == IntPtr.Zero)
                    {
                        if (process.WaitForInputIdle(500))
                            hWnd = process.MainWindowHandle;
                    }

                    if (hWnd != IntPtr.Zero)
                        FocusWindow(hWnd);

                    DataHelper.AppMutex?.Dispose();
                    Environment.Exit(0);
                }

                DataHelper.AppMutex?.Dispose();
                Environment.Exit(0);
            }
        }

        public static void FocusWindow(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero)
                throw new ArgumentException("hwnd es IntPtr.Zero", nameof(hwnd));

            bool shown = IsIconic(hwnd) ? ShowWindow(hwnd, SW_RESTORE) : ShowWindow(hwnd, SW_SHOW);
            ThrowIfFailed(shown, "ShowWindow");

            if (SetForegroundWindow(hwnd))
                return;

            IntPtr fg = GetForegroundWindow();
            uint fgThread = fg != IntPtr.Zero ? GetWindowThreadProcessId(fg, out _) : 0;
            uint targetThread = GetWindowThreadProcessId(hwnd, out _);
            uint currentThread = GetCurrentThreadId();

            bool attached = false;
            if (fgThread != 0 && targetThread != 0 && fgThread != targetThread)
            {
                attached = AttachThreadInput(currentThread, targetThread, true);
                ThrowIfFailed(attached, "AttachThreadInput (attach)");
            }

            bool brought = BringWindowToTop(hwnd);
            ThrowIfFailed(brought, "BringWindowToTop");

            bool foreground2 = SetForegroundWindow(hwnd);
            ThrowIfFailed(foreground2, "SetForegroundWindow (fallback)");

            SetFocus(hwnd);

            if (attached)
            {
                bool detached = AttachThreadInput(currentThread, targetThread, false);
                ThrowIfFailed(detached, "AttachThreadInput (detach)");
            }
        }
    }
}