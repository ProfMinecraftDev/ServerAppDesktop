using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppNotifications;
using ServerAppDesktop.Helpers;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using WinRT;

namespace ServerAppDesktop
{
    public static class EntryPoint
    {
        [STAThread]
        static async Task<int> Main(string[] _args)
        {
            if (Environment.OSVersion.Version.Build < 19041)
            {
                PInvoke.MessageBox(
                    HWND.Null,
                    "Tu versión de Windows es demasiado antigua (requiere 19041+).\nPor favor, actualiza el sistema para utilizar Server App Desktop.",
                    "Error de Compatibilidad",
                    MESSAGEBOX_STYLE.MB_OK | MESSAGEBOX_STYLE.MB_ICONERROR | MESSAGEBOX_STYLE.MB_TOPMOST);
                return 1;
            }
            PInvoke.SetCurrentProcessExplicitAppUserModelID(DataHelper.WindowIdentifier);
            string[] args = [.. _args.Select(s => s.ToLowerInvariant())];

            ComWrappersSupport.InitializeComWrappers();
            AppNotificationManager.Default.NotificationInvoked += App.OnNotificationInvoked;
            AppNotificationManager.Default.Register(DataHelper.AppName, new Uri(Path.Combine(AppContext.BaseDirectory, "Assets", "AppIcon.ico")));

            if (await WindowHelper.IsRedirectedAsync())
                return 0;

            try
            {
                Application.Start((p) =>
                {
                    var context = new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread());
                    SynchronizationContext.SetSynchronizationContext(context);
                    new App(args.Contains("--tray-only") || args.Contains("-to"));
                });
                return 0;
            }
            catch (Exception ex)
            {
                return ex.HResult;
            }
        }
    }
}
