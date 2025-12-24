using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using ServerAppDesktop.Helpers;
using ServerAppDesktop.ViewModels;
using System;
using System.Diagnostics;
using System.Threading;

namespace ServerAppDesktop
{
    public sealed partial class App : Application
    {
        public static MainWindow? MainWindow { get; private set; }
        public static IHost? Host { get; private set; }
        private static Mutex? _mutex;

        public App()
        {
            InitializeComponent();
            _mutex = new Mutex(true, "ServerAppDesktop_UniqueMutex", out bool createdNew);
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
                            WindowHelper.FocusWindow(hWnd);
                        }
                        break;
                    }
                }
                Environment.Exit(0);
            }
            Host = Microsoft.Extensions.Hosting.Host
                .CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<MainViewModel>();
                }).Build();
        }

        public static T GetRequiredService<T>() where T : notnull
        {
            if (Host == null)
            {
                throw new InvalidOperationException("El Host no está inicializado.");
            }
            return Host.Services.GetRequiredService<T>();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            if (MainWindow == null)
            {
                MainWindow = new MainWindow();
                MainWindow.Activate();
            }
            MainWindow.Activate();
        }
    }
}
