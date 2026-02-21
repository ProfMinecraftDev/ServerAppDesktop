
namespace ServerAppDesktop.ViewModels
{
    public sealed partial class TrayViewModel : ObservableRecipient, IRecipient<ServerStateChangedMessage>
    {
        [ObservableProperty]
        private string _icon = "";

        [ObservableProperty]
        private string _toolTip = "Server App Desktop (Preview)";

        [RelayCommand]
        private static void RestartApp()
        {
            _ = AppInstance.Restart("");
        }

        [RelayCommand]
        private static void SendFeedback()
        {
            _ = Launcher.LaunchUriAsync(new Uri("https://github.com/ProfMinecraftDev/ServerAppDesktop/issues"));
        }

        [RelayCommand]
        private static void ExitApp()
        {
            Environment.Exit(0);
        }

        [RelayCommand]
        private static void ShowWindow()
        {
            if (MainWindow.Instance != null)
            {
                WindowHelper.ShowAndFocus(MainWindow.Instance);
            }
        }

        [RelayCommand]
        private static void OpenSettingsFile()
        {
            string settingsPath = Path.Combine(DataHelper.SettingsPath, DataHelper.SettingsFile);
            _ = Launcher.LaunchUriAsync(new Uri(settingsPath));
        }

        public TrayViewModel()
        {
            Icon = $"{AppContext.BaseDirectory}\\Assets\\AppIcon.ico";
            IsActive = true;
        }

        public void Receive(ServerStateChangedMessage message)
        {
            Icon = ServerUIHelper.GetIconPath(message.Value.State);
            ToolTip = ServerUIHelper.GetTooltip(message.Value.State);
        }
    }
}
