using System;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using ServerAppDesktop.Helpers;
using Windows.System;

namespace ServerAppDesktop.ViewModels
{
    public sealed partial class TrayViewModel : ObservableObject
    {
        [RelayCommand]
        private void RestartApp()
        {
            Process.Start(Environment.ProcessPath ?? throw new InvalidOperationException("La ruta del proceso no se pudo determinar."));
            Environment.Exit(0);
        }

        [RelayCommand]
        private void SendFeedback() =>
            _ = Launcher.LaunchUriAsync(new Uri("https://github.com/ProfMinecraftDev/ServerAppDesktop/issues"));

        [RelayCommand]
        private void ExitApp()
        {
            Application.Current.Exit();
            Environment.Exit(0);
        }

        [RelayCommand]
        private void ShowWindow()
        {
            if (App.MainWindow != null)
                WindowHelper.ShowAndFocus(App.MainWindow);
        }
    }
}
