

namespace ServerAppDesktop.Handlers;

public static class AppHandler
{
    public static IHost ConfigureHost()
    {
        return Host.CreateDefaultBuilder()
            .UseContentRoot(AppContext.BaseDirectory)
            .ConfigureServices((context, services) =>
            {
                _ = context;

                _ = services.AddSingleton<INavigationService, NavigationService>();
                _ = services.AddSingleton<IOOBEService, OOBEService>();
                _ = services.AddSingleton<IProcessService, ProcessService>();
                _ = services.AddSingleton<IPerformanceService, PerformanceService>();
                _ = services.AddSingleton<INetworkService, NetworkService>();
                _ = services.AddSingleton<IServerPropertiesService, ServerPropertiesService>();
                _ = services.AddSingleton<ISystemService, SystemService>();
                _ = services.AddSingleton<ISettingsService, SettingsService>();
                _ = services.AddSingleton<IFilesService, FilesService>();

                _ = services.AddSingleton<IWindowHandler, WindowHandler>();

                _ = services.AddSingleton<MainViewModel>();
                _ = services.AddTransient<OOBEViewModel>();
                _ = services.AddSingleton<TrayViewModel>();
                _ = services.AddSingleton<HomeViewModel>();
                _ = services.AddSingleton<PerformanceViewModel>();
                _ = services.AddSingleton<TerminalViewModel>();
                _ = services.AddSingleton<FilesViewModel>();
                _ = services.AddSingleton<WhatsNewViewModel>();
                _ = services.AddSingleton<SystemInfoViewModel>();
                _ = services.AddSingleton<SettingsViewModel>();
            }).Build();
    }

    public static async Task CheckUpdatesAsync(bool trayOnly)
    {
        MainViewModel vm = App.GetRequiredService<MainViewModel>();
        vm.ReleaseInfo = await UpdateHelper.GetUpdateAsync(DataHelper.GitHubUsername, DataHelper.GitHubRepository, DataHelper.AppVersionTag, DataHelper.UpdateChannel == 1);

        if (vm.ReleaseInfo != null)
        {
            if (!trayOnly)
            {
                _ = MainWindow.Instance.updateDialog.ShowAsync();
            }
            else
            {
                ShowUpdateNotification(vm.ReleaseInfo.Version);
            }
        }
        else
        {
            UpdateHelper.CleanOldUpdates();
        }
    }

    private static void ShowUpdateNotification(string version)
    {
        string title = ResourceHelper.GetString("Update_Notify_Title");
        string message = string.Format(ResourceHelper.GetString("Update_Notify_Msg"), version);
        string btnText = ResourceHelper.GetString("Update_Notify_BtnDownload");

        var toast = new WindowsNotification
        {
            Title = title,
            Message = message,
            NotificationScenario = AppNotificationScenario.Reminder,
            Duration = AppNotificationDuration.Long
        };

        AppNotificationManager.Default.Show(toast.NotificationToBuild
            .AddButton(new AppNotificationButton(btnText)
                .AddArgument("action", "downloadUpdate"))
            .BuildNotification());
    }
}
