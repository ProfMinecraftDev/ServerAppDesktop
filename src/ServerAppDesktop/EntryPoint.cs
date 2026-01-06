using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppNotifications;
using ServerAppDesktop.Helpers;
using WinRT;

namespace ServerAppDesktop
{
    public static class EntryPoint
    {
        [STAThread]
        static async Task Main(string[] _args)
        {
            string[] args = [.. _args.Select(s => s.ToLowerInvariant())];

            ComWrappersSupport.InitializeComWrappers();

            if (await WindowHelper.IsRedirectedAsync())
            {
                return;
            }

            AppNotificationManager.Default.Register(DataHelper.AppName, new Uri(Path.Combine(AppContext.BaseDirectory, "Assets", "AppIcon.ico")));

            Application.Start((p) =>
            {
                var context = new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread());
                DispatcherQueueSynchronizationContext.SetSynchronizationContext(context);
                new App(args.Contains("--tray-only") || args.Contains("-to"));
            });
        }
    }
}
