using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Animation;
using ServerAppDesktop.Helpers;
using ServerAppDesktop.ViewModels;
using ServerAppDesktop.Views;
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

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            if (MainWindow == null)
            {
                MainWindow = new MainWindow();
                MainWindow.Activate();
            }
            MainWindow.Activate();

            bool isConnected = await NetworkHelper.IsInternetAvailableAsync();
            MainWindow.ViewModel.IsConnectedToInternet = isConnected;

            bool needsToShowOOBE = false;

            if (needsToShowOOBE)
            {
                MainWindow.contentFrame.Navigate(typeof(OOBEView), null, new DrillInNavigationTransitionInfo());
            }
            else
            {
                MainWindow.contentFrame.Navigate(typeof(MainView), null, new DrillInNavigationTransitionInfo());
            }
        }
    }
}
