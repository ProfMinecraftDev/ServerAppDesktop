using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using ServerAppDesktop.Helpers;
using ServerAppDesktop.ViewModels;
using System;

namespace ServerAppDesktop
{
    public sealed partial class App : Application
    {
        public static MainWindow? MainWindow { get; private set; } = null;
        public static IHost? Host { get; private set; } = null;

        public App()
        {
            WindowHelper.EnsureSingleInstance();
            InitializeComponent();
            Host = Microsoft.Extensions.Hosting.Host
                .CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<MainViewModel>();
                })
                .Build();
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
