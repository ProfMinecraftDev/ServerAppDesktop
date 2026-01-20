using System;
using System.IO;
using H.NotifyIcon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.Windows.AppLifecycle;
using Microsoft.Windows.AppNotifications.Builder;
using ServerAppDesktop.Controls;
using ServerAppDesktop.Helpers;
using ServerAppDesktop.Services;
using ServerAppDesktop.ViewModels;
using ServerAppDesktop.Views;

namespace ServerAppDesktop
{
    public sealed partial class App : Application
    {
        public static IHost? Host { get; private set; } = null;
        public static TaskbarIcon? TrayIcon { get; private set; } = null;

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
                    services.AddTransient<IOOBEService, OOBEService>();

                    services.AddSingleton<MainViewModel>();
                    services.AddTransient<OOBEViewModel>();
                    services.AddTransient<TrayViewModel>();
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
            bool needsToShowOOBE = !SettingsHelper.ExistsConfigurationFile();

            if (!needsToShowOOBE)
                SettingsHelper.LoadAndSetSettings(MainWindow.Current);

            AppInstance.GetCurrent().Activated += (s, e) =>
                {
                    MainWindow.Current.DispatcherQueue.TryEnqueue(() =>
                    {
                        WindowHelper.ShowAndFocus(MainWindow.Current);
                    });
                };

            if (!trayOnly)
                MainWindow.Current.Activate();

            if (MainWindow.Current != null)
            {
                bool isConnected = await NetworkHelper.IsInternetAvailableAsync();
                MainWindow.Current.ViewModel.IsConnectedToInternet = isConnected;

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
                        if (!trayOnly)
                            _ = MainWindow.Current.updateDialog.ShowAsync();
                        var newUpdateNotification = new WindowsNotification
                        {
                            Title = "Nueva actualización disponible",
                            Messsage = $"La versión {mainViewModel.ReleaseInfo.Version} te espera",
                            SoundEvent = AppNotificationSoundEvent.IM,
                            NotificationScenario = AppNotificationScenario.Reminder,
                            HeroImagerUri = new Uri(Path.Combine(AppContext.BaseDirectory, "Assets", "HeroImage.png")),
                            AppLogoUri = new Uri(Path.Combine(AppContext.BaseDirectory, "Assets", "AppIcon.ico")),
                            Duration = AppNotificationDuration.Long,
                            TimeStamp = DateTime.Now
                        };
                        newUpdateNotification.ShowNotification();
                    }
                    else
                        UpdateHelper.CleanOldUpdates();
                }

                MainWindow.Current.contentFrame.Navigate(
                    needsToShowOOBE ? typeof(OOBEView) : typeof(MainView),
                    null,
                    new DrillInNavigationTransitionInfo()
                );
            }
        }

    }
}
