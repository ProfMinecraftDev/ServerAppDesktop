using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ServerAppDesktop.Helpers;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.System;

namespace ServerAppDesktop.ViewModels
{
    public sealed partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _debugMode = DataHelper.DebugMode;

        [ObservableProperty]
        private bool _runAsAdmin = DataHelper.RunAsAdmin;

        [ObservableProperty]
        private bool _canGoBack = false;

        [ObservableProperty]
        private bool _isConnectedToInternet = true;

        [RelayCommand]
        private void RestartApp()
        {
            Process.Start(Environment.ProcessPath ?? throw new InvalidOperationException("La ruta del proceso no se pudo determinar."));
            Environment.Exit(0);
        }

        [RelayCommand]
        private async Task SendFeedback(FrameworkElement content)
        {
            var uri = new Uri("https://github.com/ProfMinecraftDev/ServerAppDesktop/issues");

            var dialog = new ContentDialog
            {
                Title = ResourceHelper.GetString("FeedbackDialog_Title"),
                Content = ResourceHelper.GetString("FeedbackDialog_Description"),
                PrimaryButtonText = ResourceHelper.GetString("FeedbackDialog_SendButton"),
                PrimaryButtonStyle = Application.Current.Resources["AccentButtonStyle"] as Style,
                CloseButtonText = ResourceHelper.GetString("FeedbackDialog_CloseButton"),
                XamlRoot = content.XamlRoot,
                RequestedTheme = content.RequestedTheme
            };

            var result = await dialog.ShowAsync();
            if (result != ContentDialogResult.Primary)
                return;

            _ = Launcher.LaunchUriAsync(uri);
            return;
        }

        [RelayCommand]
        private void ShowWarning(FrameworkElement content)
        {
            var dialog = new ContentDialog
            {
                Title = ResourceHelper.GetString("WarningDialog_Title"),
                Content = ResourceHelper.GetString("WarningDialog_Description"),
                CloseButtonText = ResourceHelper.GetString("WarningDialog_CloseButton"),
                XamlRoot = content.XamlRoot,
                RequestedTheme = content.RequestedTheme
            };

            _ = dialog.ShowAsync();
        }

        [RelayCommand]
        private void ShowAdminWarning(FrameworkElement content)
        {
            var dialog = new ContentDialog
            {
                Title = ResourceHelper.GetString("AdminDialog_Title"),
                Content = ResourceHelper.GetString("AdminDialog_Description"),
                CloseButtonText = ResourceHelper.GetString("AdminDialog_CloseButton"),
                XamlRoot = content.XamlRoot,
                RequestedTheme = content.RequestedTheme
            };

            _ = dialog.ShowAsync();
        }
    }
}
