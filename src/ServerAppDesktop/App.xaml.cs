using System;
using System.IO;
using H.NotifyIcon;
using H.NotifyIcon.EfficiencyMode;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.Windows.AppLifecycle;
using Microsoft.Windows.AppNotifications;
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
            Host = Microsoft.Extensions.Hosting.Host.
                CreateDefaultBuilder().
                UseContentRoot(AppContext.BaseDirectory).
                ConfigureServices((context, services) =>
                {
                    services.AddSingleton<INavigationService, NavigationService>();
                    services.AddSingleton<IOOBEService, OOBEService>();
                    services.AddSingleton<IProcessService, ProcessService>();
                    services.AddSingleton<IPerformanceService, PerformanceService>();

                    services.AddSingleton<MainViewModel>();
                    services.AddTransient<OOBEViewModel>();
                    services.AddSingleton<TrayViewModel>();
                    services.AddSingleton<HomeViewModel>();
                    services.AddSingleton<PerformanceViewModel>();
                    services.AddSingleton<TerminalViewModel>();
                    services.AddSingleton<FilesViewModel>();
                    services.AddSingleton<WhatsNewViewModel>();
                    services.AddSingleton<SystemInfoViewModel>();
                    services.AddSingleton<AboutViewModel>();
                    services.AddSingleton<SettingsViewModel>();
                }).
                Build();
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
            base.OnLaunched(args);

            var keyInstance = AppInstance.FindOrRegisterForKey(DataHelper.WindowIdentifier);
            keyInstance.Activated += (s, e) =>
            {
                MainWindow.Instance.DispatcherQueue.TryEnqueue(() =>
                {
                    WindowHelper.ShowAndFocus(MainWindow.Instance);
                });
            };

            if (SettingsHelper.ExistsConfigurationFile())
                SettingsHelper.LoadAndSetSettings(MainWindow.Instance);
            else
                DataHelper.Settings = null;

            bool needsToShowOOBE = DataHelper.Settings == null;

            if (!trayOnly)
                WindowHelper.ShowAndFocus(MainWindow.Instance);
            else
                EfficiencyModeUtilities.SetEfficiencyMode(true);

            if (MainWindow.Instance != null)
            {
                bool isConnected = await NetworkHelper.IsInternetAvailableAsync();
                MainWindow.Instance.ViewModel.IsConnectedToInternet = isConnected;

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
                            _ = MainWindow.Instance.updateDialog.ShowAsync();
                        else
                        {
                            var newUpdateNotification = new WindowsNotification
                            {
                                Title = "Nueva actualización disponible",
                                Message = $"La versión {mainViewModel.ReleaseInfo.Version} te espera",
                                SoundEvent = AppNotificationSoundEvent.IM,
                                NotificationScenario = AppNotificationScenario.Reminder,
                                HeroImagerUri = new Uri(Path.Combine(AppContext.BaseDirectory, "Assets", "HeroImage.png")),
                                AppLogoUri = new Uri(Path.Combine(AppContext.BaseDirectory, "Assets", "AppIcon.ico")),
                                Duration = AppNotificationDuration.Long,
                                TimeStamp = DateTime.Now
                            };
                            AppNotificationManager.Default.Show(newUpdateNotification.NotificationToBuild.AddButton(new AppNotificationButton("Descargar actualización").AddArgument("action", "downloadUpdate")).BuildNotification());
                        }
                    }
                    else
                        UpdateHelper.CleanOldUpdates();
                }

                MainWindow.Instance.contentFrame.Navigate(
                    needsToShowOOBE ? typeof(OOBEView) : typeof(MainView),
                    null,
                    new DrillInNavigationTransitionInfo()
                );
            }
        }
        public static void OnNotificationInvoked(AppNotificationManager sender, AppNotificationActivatedEventArgs args)
        {
            // 1. Sacar el argumento que pusiste en el botón
            if (args.Arguments.TryGetValue("action", out string? action))
            {
                if (action == "downloadUpdate")
                    MainWindow.Instance.DispatcherQueue.TryEnqueue(() =>
                    {
                        GetRequiredService<MainViewModel>().DownloadUpdateCommand.Execute(null);
                    });
                else if (action == "restartToInstallUpdate")
                {
                    EfficiencyModeUtilities.SetEfficiencyMode(false);
                    Environment.Exit(0);
                }
                else if (action == "activate")
                    MainWindow.Instance.DispatcherQueue.TryEnqueue(() =>
                    {
                        WindowHelper.ShowAndFocus(MainWindow.Instance);

                    });
            }
        }
    }
}
