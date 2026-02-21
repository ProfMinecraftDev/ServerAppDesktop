
namespace ServerAppDesktop.Handlers;

public static class CommandLineHandler
{
    public static bool TrayOnly => _trayOnly;

    private static bool _trayOnly = false;
    private static readonly string[] ValidArgs = ["--help", "--version", "--reset-settings", "--tray-only"];

    public static void Handle(string[] args)
    {
        Debug.WriteLine("Args: " + string.Join(" ", args));
        foreach (var arg in args)
        {
            if (!ValidArgs.Any(v => arg.StartsWith(v)))
            {
                _ = PInvoke.MessageBox(
                    HWND.Null,
                    $"Argumento no reconocido: {arg}",
                    "Error",
                    MESSAGEBOX_STYLE.MB_OK | MESSAGEBOX_STYLE.MB_ICONERROR | MESSAGEBOX_STYLE.MB_TOPMOST
                    );
                Environment.Exit(WIN32_ERROR.ERROR_INVALID_PARAMETER.To<int>());
            }

            if (arg is "--tray-only")
                _trayOnly = true;

            if (arg is "--version")
            {
                PInvoke.MessageBox(HWND.Null, $"{DataHelper.AppName} Versión {DataHelper.AppVersion}", "Información de Versión",
                    MESSAGEBOX_STYLE.MB_OK | MESSAGEBOX_STYLE.MB_ICONINFORMATION | MESSAGEBOX_STYLE.MB_TOPMOST);
                Environment.Exit(WIN32_ERROR.ERROR_SUCCESS.To<int>());
            }
        }

        if (args.Any(a => a is "--reset-settings"))
        {
            SettingsHelper.ResetSettings();
            _ = AppInstance.Restart("");
        }
    }

}
