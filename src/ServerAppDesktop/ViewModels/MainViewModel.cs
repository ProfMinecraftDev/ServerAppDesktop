

namespace ServerAppDesktop.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private readonly INavigationService _navService;

    [ObservableProperty]
    private bool _debugMode = DataHelper.DebugMode;

    [ObservableProperty]
    private bool _runAsAdmin = DataHelper.RunAsAdmin;

    [ObservableProperty]
    private bool _canGoBack = false;

    [ObservableProperty]
    private bool _isConnectedToInternet = true;

    [ObservableProperty]
    private ReleaseInfo? _releaseInfo;

    [ObservableProperty]
    private bool _downloadingAnUpdate = false;

    [ObservableProperty]
    private string _updateDownloadProgress = "";

    [ObservableProperty]
    private double _downloadProgressValue = 0;

    [ObservableProperty]
    private bool _needsToRestart = false;

    public MainViewModel(INavigationService navService)
    {
        _navService = navService;
        _navService.CanGoBackChanged += (_, args) => CanGoBack = args.CanGoBack;

        UpdateHelper.DownloadProgressChanged += (_, e) =>
        {
            UpdateDownloadProgress = e.Text;
            DownloadProgressValue = e.Progress;
        };
    }

    [RelayCommand]
    private static void RestartApp()
    {
        string exePath = Process.GetCurrentProcess().MainModule?.FileName ?? string.Empty;
        ProcessStartInfo startInfo = new(exePath)
        {
            UseShellExecute = true,
            Verb = DataHelper.RunAsAdmin ? "runas" : string.Empty
        };
        _ = Process.Start(startInfo);
        Environment.Exit(0);
    }

    [RelayCommand]
    private static void ShowFeedbackDialog(ContentDialog dialog)
    {
        _ = dialog.ShowAsync();
    }

    [RelayCommand]
    private static void OpenSettingsFile()
    {
        _ = Task.Run(async () =>
        {
            string settingsPath = Path.Combine(DataHelper.SettingsPath, DataHelper.SettingsFile);
            StorageFile file = await StorageFile.GetFileFromPathAsync(settingsPath);
            _ = Launcher.LaunchFileAsync(file);
        });
    }

    [RelayCommand]
    private static void SendFeedback()
    {
        _ = Launcher.LaunchUriAsync(new Uri("https://github.com/ProfMinecraftDev/ServerAppDesktop/issues"));
    }

    [RelayCommand]
    private static void ShowWarning(ContentDialog dialog)
    {
        _ = dialog.ShowAsync();
        WindowHelper.ClearBadge(MainWindow.Instance);
    }

    [RelayCommand]
    private static void ShowAdminWarning(ContentDialog dialog)
    {
        _ = dialog.ShowAsync();
    }

    [RelayCommand]
    private async Task DownloadUpdateAsync()
    {
        DownloadingAnUpdate = true;
        bool sucessDownload = await UpdateHelper.DownloadUpdateAsync(ReleaseInfo?.Assets.FirstOrDefault(a => a.Name.EndsWith(".exe")) ?? new Asset());
        if (sucessDownload)
        {
            UpdateDownloadProgress = ResourceHelper.GetString("UpdateInfoBar_PreparedToApply");
            WindowsNotification updateReadyToInstallNotification = new()
            {
                Title = "Actualización lista para instalar",
                Message = $"La versión {ReleaseInfo?.Version} está lista para instalarse, se instalará al cerrar.",
                SoundEvent = AppNotificationSoundEvent.IM,
                NotificationScenario = AppNotificationScenario.Urgent,
                AppLogoUri = new Uri(Path.Combine(AppContext.BaseDirectory, "Assets", "Update.png")),
                Duration = AppNotificationDuration.Long,
                TimeStamp = DateTime.Now
            };
            AppNotification notification = updateReadyToInstallNotification.NotificationToBuild.AddButton(new AppNotificationButton("Reiniciar ahora").AddArgument("action", "restartToInstallUpdate")).BuildNotification();
            AppNotificationManager.Default.Show(notification);
            await Task.Delay(10000);
            DownloadingAnUpdate = !sucessDownload;
        }
    }

    [RelayCommand]
    private static void ExitApp()
    {
        Environment.Exit(0);
    }
}
