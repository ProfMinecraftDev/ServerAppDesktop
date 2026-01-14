using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.AppLifecycle;
using ServerAppDesktop.Controls;
using ServerAppDesktop.Helpers;
using ServerAppDesktop.Models;
using ServerAppDesktop.Services;
using Windows.System;

namespace ServerAppDesktop.ViewModels
{
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

        public MainViewModel(INavigationService navService)
        {
            _navService = navService;
            _navService.CanGoBackChanged += (canGoBack) => CanGoBack = canGoBack;

            UpdateHelper.DownloadProgress += (progress, value) =>
            {
                UpdateDownloadProgress = progress;
                DownloadProgressValue = value;
            };
        }

        [RelayCommand]
        private void RestartApp() => AppInstance.Restart("");

        [RelayCommand]
        private void ShowFeedbackDialog(ContentDialog dialog) => _ = dialog.ShowAsync();

        [RelayCommand]
        private void SendFeedback() =>
            _ = Launcher.LaunchUriAsync(new Uri("https://github.com/ProfMinecraftDev/ServerAppDesktop/issues"));

        [RelayCommand]
        private void ShowWarning(ContentDialog dialog) => _ = dialog.ShowAsync();

        [RelayCommand]
        private void ShowAdminWarning(ContentDialog dialog) => _ = dialog.ShowAsync();

        [RelayCommand]
        private async Task DownloadUpdateAsync()
        {
            DownloadingAnUpdate = true;
            bool sucessDownload = await UpdateHelper.DownloadUpdateAsync(ReleaseInfo?.Assets.FirstOrDefault(a => a.Name.EndsWith(".exe")) ?? new Asset());
            if (sucessDownload)
            {
                UpdateDownloadProgress = ResourceHelper.GetString("UpdateInfoBar_PreparedToApply");
                var updateReadyToInstallNotification = new WindowsNotification
                {
                    Title = "Actualización lista para instalar",
                    Messsage = $"La versión {ReleaseInfo?.Version} está lista para instalarse, se instalará al cerrar.",
                    SoundEvent = Microsoft.Windows.AppNotifications.Builder.AppNotificationSoundEvent.IM,
                    NotificationScenario = Microsoft.Windows.AppNotifications.Builder.AppNotificationScenario.Urgent,
                    AppLogoUri = new Uri(Path.Combine(AppContext.BaseDirectory, "Assets", "Update.png")),
                    Duration = Microsoft.Windows.AppNotifications.Builder.AppNotificationDuration.Long,
                    TimeStamp = DateTime.Now
                };
                updateReadyToInstallNotification.ShowNotification();
                await Task.Delay(10000);
                DownloadingAnUpdate = !sucessDownload;
            }
        }

        [RelayCommand]
        private void ExitApp() => Application.Current.Exit();
    }
}
