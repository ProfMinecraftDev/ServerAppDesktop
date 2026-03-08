namespace ServerAppDesktop.Handlers;

public static class NotificationHandler
{
    public static void HandleNotification(AppNotificationActivatedEventArgs args)
    {
        if (!args.Arguments.TryGetValue("action", out string? action))
        {
            return;
        }

        _ = MainWindow.Instance.DispatcherQueue.TryEnqueue(() =>
        {
            switch (action)
            {
                case "downloadUpdate":
                    App.GetRequiredService<MainViewModel>().DownloadUpdateCommand.Execute(null);
                    break;
                case "restartToInstallUpdate":
                    ProcessHelper.SetEfficiencyMode(false);
                    Environment.Exit(0);
                    break;
                case "activate":
                    WindowHelper.ShowAndFocus(MainWindow.Instance);
                    break;
            }
        });
    }

    public static void NotificationInvoked(AppNotificationManager _, AppNotificationActivatedEventArgs args)
    {
        HandleNotification(args);
    }
}
