
namespace ServerAppDesktop.ViewModels;

public sealed partial class TrayViewModel : ObservableRecipient, IRecipient<ServerStateChangedMessage>
{
    [ObservableProperty]
    private string _icon = "";

    [ObservableProperty]
    private string _toolTip = "Server App Desktop (Preview)";

    [RelayCommand]
    private static void RestartApp()
    {
        string exePath = Environment.ProcessPath ?? string.Empty;
        ProcessStartInfo startInfo = new(exePath)
        {
            UseShellExecute = true,
            Verb = DataHelper.RunAsAdmin ? "runas" : string.Empty
        };
        _ = Process.Start(startInfo);
        Environment.Exit(0);
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
        _ = Task.Run(async () =>
        {
            string settingsPath = Path.Combine(DataHelper.SettingsPath, DataHelper.SettingsFile);
            StorageFile file = await StorageFile.GetFileFromPathAsync(settingsPath);
            _ = Launcher.LaunchFileAsync(file);
        });
    }

    [RelayCommand]
    private static void GoToSourceCodePage()
    {
        _ = Launcher.LaunchUriAsync(new Uri("https://github.com/ProfMinecraftDev/ServerAppDesktop"));
    }

    public TrayViewModel()
    {
        Icon = $"{AppContext.BaseDirectory}\\Assets\\AppIcon.ico";
        IsActive = true;
    }

    public void Receive(ServerStateChangedMessage message)
    {
        Icon = ServerUIHelper.GetIconPath(message.Value);
        ToolTip = ServerUIHelper.GetTooltip(message.Value);
    }
}
