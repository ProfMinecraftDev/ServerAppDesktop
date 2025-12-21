using System;
using System.Runtime.InteropServices;

namespace ServerAppDesktop.Helpers
{
    public static partial class WindowHelper
    {
        [LibraryImport("user32.dll", EntryPoint = "FindWindowW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
        private static partial IntPtr FindWindowW(string lpClassName, string lpWindowName);

        public static IntPtr GetWindowHandle()
        {
            return FindWindowW(null, DataHelper.AppName);
        }
    }
}
