namespace ServerAppDesktop.Helpers;

public static class SettingsHelper
{
    public static bool ExistsConfigurationFile()
    {
        return File.Exists(Path.Combine(DataHelper.SettingsPath, DataHelper.SettingsFile));
    }

    public static void LoadAndSetBasicSettings()
    {
        string fullPath = Path.Combine(DataHelper.SettingsPath, DataHelper.SettingsFile);

        if (!File.Exists(fullPath))
        {
            DataHelper.Settings = null;
            return;
        }

        try
        {
            string jsonString = File.ReadAllText(fullPath);


            if (string.IsNullOrWhiteSpace(jsonString))
            {
                DataHelper.Settings = null;
                return;
            }

            JsonSerializerOptions options = new()
            { PropertyNameCaseInsensitive = true, WriteIndented = true, IndentSize = 4 };
            AppSettingsJsonContext context = new(options);


            AppSettings? result = JsonSerializer.Deserialize(jsonString, context.AppSettings);



            if (result != null)
            {

                bool isInvalid = string.IsNullOrWhiteSpace(result.Server.Path) ||
                                 string.IsNullOrWhiteSpace(result.Server.Executable);

                DataHelper.Settings = isInvalid ? null : result;
            }
            else
            {
                DataHelper.Settings = null;
            }
        }
        catch
        {
            DataHelper.Settings = null;
        }

        AppSettings? settings = DataHelper.Settings;
        if (settings == null)
        {
            return;
        }
        if (!string.IsNullOrEmpty(DataHelper.Settings?.UI.Language))
        {
            ApplicationLanguages.PrimaryLanguageOverride = DataHelper.Settings.UI.Language;
            var manager = new Microsoft.Windows.ApplicationModel.Resources.ResourceManager();
            ResourceContext context = manager.CreateResourceContext();

            context.QualifierValues["Language"] = DataHelper.Settings.UI.Language;
            var ci = new System.Globalization.CultureInfo(DataHelper.Settings.UI.Language);
            System.Globalization.CultureInfo.CurrentCulture = ci;
            System.Globalization.CultureInfo.CurrentUICulture = ci;
            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = ci;
            System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = ci;
        }
    }

    public static void SaveSettings()
    {
        if (DataHelper.Settings == null)
        {
            return;
        }

        AppSettingsJsonContext context = new(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            IndentSize = 4
        });
        string jsonString = JsonSerializer.Serialize(DataHelper.Settings, context.AppSettings);
        string jsonFilePath = Path.Combine(DataHelper.SettingsPath, DataHelper.SettingsFile);
        if (!string.IsNullOrEmpty(DataHelper.SettingsPath) && !Directory.Exists(DataHelper.SettingsPath))
        {
            _ = Directory.CreateDirectory(DataHelper.SettingsPath);
        }

        File.WriteAllText(jsonFilePath, jsonString);
    }
    public static void ResetSettings()
    {
        try
        {
            if (Directory.Exists(DataHelper.SettingsPath))
            {
                Directory.Delete(DataHelper.SettingsPath, true);
            }

            _ = PInvoke.MessageBox(
                HWND.Null,
                ResourceHelper.GetString("Reset_Success_Msg"),
                ResourceHelper.GetString("Reset_Success_Title"),
                MESSAGEBOX_STYLE.MB_ICONINFORMATION | MESSAGEBOX_STYLE.MB_OK | MESSAGEBOX_STYLE.MB_TOPMOST
            );

            DataHelper.Settings = null;
        }
        catch (Exception ex)
        {
            string errorMsg = string.Format(ResourceHelper.GetString("Reset_Error_Msg"), ex.Message);

            _ = PInvoke.MessageBox(
                HWND.Null,
                errorMsg,
                ResourceHelper.GetString("Reset_Error_Title"),
                MESSAGEBOX_STYLE.MB_ICONERROR | MESSAGEBOX_STYLE.MB_OK | MESSAGEBOX_STYLE.MB_TOPMOST
            );
        }

        string exePath = Environment.ProcessPath ?? string.Empty;
        ProcessStartInfo startInfo = new(exePath)
        {
            UseShellExecute = true,
            Verb = DataHelper.RunAsAdmin ? "runas" : string.Empty
        };
        _ = Process.Start(startInfo);
        Environment.Exit(0);
    }
}
