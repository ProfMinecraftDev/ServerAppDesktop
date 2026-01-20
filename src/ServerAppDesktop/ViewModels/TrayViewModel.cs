using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using ServerAppDesktop.Helpers;

using Windows.System;

namespace ServerAppDesktop.ViewModels
{
    public sealed partial class TrayViewModel : ObservableObject
    {
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
            if (MainWindow.Current != null)
                WindowHelper.ShowAndFocus(MainWindow.Current);
        }
    }
}
