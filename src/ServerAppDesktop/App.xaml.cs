using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.Windows.AppLifecycle;
using ServerAppDesktop.Helpers;
using ServerAppDesktop.Services;
using ServerAppDesktop.ViewModels;
using ServerAppDesktop.Views;

namespace ServerAppDesktop
{
    public sealed partial class App : Application
    {
        public static MainWindow? MainWindow { get; private set; } = null;
        public static IHost? Host { get; private set; } = null;

        private readonly bool trayOnly = false;

        public App(bool trayOnly = false)
        {
            this.trayOnly = trayOnly;
            InitializeComponent();
            Host = Microsoft.Extensions.Hosting.Host
                .CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<INavigationService, NavigationService>();

                    services.AddSingleton<MainViewModel>();
                    services.AddTransient<TrayViewModel>();
                })
                .Build();

            var trayIcon = new TrayIcon();
            trayIcon.ForceCreate();
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
            MainWindow = new MainWindow();
            AppInstance.GetCurrent().Activated += (s, e) =>
            {
                MainWindow.DispatcherQueue.TryEnqueue(() =>
                {
                    WindowHelper.ShowAndFocus(MainWindow);
                });
            };

            if (!trayOnly)
            {
                MainWindow.Activate();
            }

            if (MainWindow != null)
            {
                bool isConnected = await NetworkHelper.IsInternetAvailableAsync();
                MainWindow.ViewModel.IsConnectedToInternet = isConnected;

                var mainViewModel = GetRequiredService<MainViewModel>();
                if (isConnected)
                {
                    mainViewModel.ReleaseInfo = await UpdateHelper.GetUpdateAsync(
                        DataHelper.GitHubUsername,
                        DataHelper.GitHubRepository,
                        DataHelper.AppVersionTag,
                        DataHelper.UpdateChannel
                    );

                    if (mainViewModel.ReleaseInfo != null)
                    {
                        _ = MainWindow.updateDialog.ShowAsync();
                    }
                    else
                        UpdateHelper.CleanOldUpdates();
                }

                bool needsToShowOOBE = false;
                MainWindow.contentFrame.Navigate(
                    needsToShowOOBE ? typeof(OOBEView) : typeof(MainView),
                    null,
                    new DrillInNavigationTransitionInfo()
                );
            }
        }

    }
}
