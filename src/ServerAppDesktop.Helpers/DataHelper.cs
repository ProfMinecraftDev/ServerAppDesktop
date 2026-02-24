
namespace ServerAppDesktop.Helpers;

public static partial class DataHelper
{
    public static int UpdateChannel { get; } = 1;

    public static string WindowIdentifier => UpdateChannel switch
    {
        0 => "ServerAppDesktop_3e2b8f4a-1c4e-4d3b-9f7a-2f4e5d6c7b8a",
        1 => "ServerAppDesktop_74af9644-d6d0-4762-84ab-c54826171600",
        _ => throw new IndexOutOfRangeException($"Canal inválido: {UpdateChannel}")
    };

    public static string AppUserModelID => UpdateChannel switch
    {
        0 => "ProfMinecraftDev.ServerAppDesktop",
        1 => "ProfMinecraftDev.ServerAppDesktop.Preview",
        _ => throw new IndexOutOfRangeException($"Canal inválido: {UpdateChannel}")
    };

    public static AppSettings? Settings { get; set; }
    public static string AppName { get; } = "Server App Desktop";
    public static string AppVersion { get; } = "1.0.0.3 (Preview)";
    public static string AppVersionTag { get; } = "v1.0.0.3-Preview";

    public static string WindowTitle => UpdateChannel switch
    {
        0 => AppName,
        1 => $"{AppName} (Preview)",
        _ => throw new IndexOutOfRangeException($"Canal inválido: {UpdateChannel}")
    };

    public static string WindowSubtitle { get; private set; } = string.Empty;
    public static string GitHubUsername { get; } = "ProfMinecraftDev";
    public static string GitHubRepository { get; } = "ServerAppDesktop";

    public static string SettingsPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        UpdateChannel == 0 ? AppName : $"{AppName} (Preview)",
        "Settings");

    public static string SettingsFile { get; }
    public static bool RunAsAdmin { get; private set; }

    public static bool DebugMode { get; } =
#if DEBUG
        true;
#else
        false;
#endif

    static DataHelper()
    {
        SettingsFile = !DebugMode ? "appsettings.json" : "appsettings.Debug.json";
        using var identity = WindowsIdentity.GetCurrent();
        WindowsPrincipal principal = new(identity);
        RunAsAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);

        WindowSubtitle = (RunAsAdmin, DebugMode) switch
        {
            (true, true) => ResourceHelper.GetString("AdminDebugModeIndicator"),
            (false, true) => ResourceHelper.GetString("DebugModeIndicator"),
            (true, false) => ResourceHelper.GetString("AdminModeIndicator"),
            _ => string.Empty
        };
    }
}
