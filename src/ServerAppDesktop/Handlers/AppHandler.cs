

namespace ServerAppDesktop.Handlers;

public static class AppHandler
{
    public static IHost ConfigureHost() =>
        Host.CreateDefaultBuilder()
            .UseContentRoot(AppContext.BaseDirectory)
            .ConfigureServices((_, services) =>
            {
                services.AddSingleton<INavigationService, NavigationService>();
                services.AddSingleton<IOOBEService, OOBEService>();
                services.AddSingleton<IProcessService, ProcessService>();
                services.AddSingleton<IPerformanceService, PerformanceService>();
                services.AddSingleton<INetworkService, NetworkService>();
                services.AddSingleton<IServerPropertiesService, ServerPropertiesService>();

                services.AddSingleton<IWindowHandler, WindowHandler>();

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
            }).Build();

    public static async Task CheckUpdatesAsync(bool trayOnly)
    {
        var vm = App.GetRequiredService<MainViewModel>();
        vm.ReleaseInfo = await UpdateHelper.GetUpdateAsync(DataHelper.GitHubUsername, DataHelper.GitHubRepository, DataHelper.AppVersionTag, DataHelper.UpdateChannel == 1);

        if (vm.ReleaseInfo != null)
        {
            if (!trayOnly)
                _ = MainWindow.Instance.updateDialog.ShowAsync();
            else
                ShowUpdateNotification(vm.ReleaseInfo.Version);
        }
        else
            UpdateHelper.CleanOldUpdates();
    }

    private static void ShowUpdateNotification(string version)
    {
        var toast = new WindowsNotification
        {
            Title = "Nueva actualización",
            Message = $"La versión {version} está lista",
            NotificationScenario = AppNotificationScenario.Reminder
        };
        AppNotificationManager.Default.Show(toast.NotificationToBuild
            .AddButton(new AppNotificationButton("Descargar").AddArgument("action", "downloadUpdate")).BuildNotification());
    }
}
