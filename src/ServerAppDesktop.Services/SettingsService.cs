namespace ServerAppDesktop.Services;

public sealed class SettingsService : ISettingsService
{
    public bool GetStartWithWindows()
    {
        const string path = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run";
        string appName = DataHelper.UpdateChannel == 0 ? DataHelper.AppName : $"{DataHelper.AppName} (Preview)";
        object? registryValue = Registry.GetValue(path, appName, null);
        return registryValue != null;
    }

    public void Save()
    {
        var options = new JsonSerializerOptions { WriteIndented = true, IndentSize = 4 };
        AppSettingsJsonContext context = new(options);

        string json = JsonSerializer.Serialize(DataHelper.Settings, context.AppSettings);
        string path = Path.Combine(DataHelper.SettingsPath, DataHelper.SettingsFile);

        if (!Directory.Exists(DataHelper.SettingsPath))
            Directory.CreateDirectory(DataHelper.SettingsPath);

        File.WriteAllText(path, json);
    }

    public int GetLanguageIndex()
    {
        string code = DataHelper.Settings?.UI.Language?.ToLowerInvariant() ?? "";
        return code == "es-419" ? 1 : 0;
    }

    public void SetStartWithWindows(bool enable)
    {
        const string path = @"Software\Microsoft\Windows\CurrentVersion\Run";
        string appName = DataHelper.UpdateChannel == 0 ? DataHelper.AppName : $"{DataHelper.AppName} (Preview)";

        string? exePath = Environment.ProcessPath;

        if (string.IsNullOrEmpty(exePath))
            return;

        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(path, true);
        if (key == null)
            return;

        if (enable)
        {
            key.SetValue(appName, $"\"{exePath}\" --tray-only");
        }
        else
        {
            if (key.GetValue(appName) != null)
            {
                key.DeleteValue(appName);
            }
        }
    }
}
