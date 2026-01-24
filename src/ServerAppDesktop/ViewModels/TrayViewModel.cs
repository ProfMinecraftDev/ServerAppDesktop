using System;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using ServerAppDesktop.Helpers;
using ServerAppDesktop.Messaging;
using Windows.System;

namespace ServerAppDesktop.ViewModels
{
    public sealed partial class TrayViewModel : ObservableRecipient, IRecipient<ServerStateChangedMessage>
    {
        [ObservableProperty]
        private string _icon = "";

        [RelayCommand]
        private void RestartApp() => AppInstance.Restart("");

        [RelayCommand]
        private void SendFeedback() =>
            _ = Launcher.LaunchUriAsync(new Uri("https://github.com/ProfMinecraftDev/ServerAppDesktop/issues"));

        [RelayCommand]
        private void ExitApp() => Application.Current.Exit();

        [RelayCommand]
        private void ShowWindow()
        {
            if (MainWindow.Instance != null)
                WindowHelper.ShowAndFocus(MainWindow.Instance);
        }

        [RelayCommand]
        private void OpenSettingsFile()
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
        }
    }
}
