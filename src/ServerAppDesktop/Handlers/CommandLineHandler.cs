
namespace ServerAppDesktop.Handlers;

public static class CommandLineHandler
{
    public static bool TrayOnly => _trayOnly;

    private static bool _trayOnly = false;
    private static readonly string[] ValidArgs = ["--help", "-h", "--version", "-v", "--reset-settings", "--tray-only"];

    public static void Handle(string[] args)
    {
        foreach (string arg in args)
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

            if (arg is "--version" or "-v")
            {
                PInvoke.MessageBox(HWND.Null, $"{DataHelper.AppName} Versión {DataHelper.AppVersion}", "Información de Versión",
                    MESSAGEBOX_STYLE.MB_OK | MESSAGEBOX_STYLE.MB_ICONINFORMATION | MESSAGEBOX_STYLE.MB_TOPMOST);
                Environment.Exit(WIN32_ERROR.ERROR_SUCCESS.To<int>());
            }

            if (arg is "--help" or "-h")
            {
                string msg = @"
                Ayuda de argumentos:
                --help, -h          : Muestra esta ayuda.
                --version, -v       : Muestra la versión de la app.
                --reset-settings    : Restablece la configuración de la app,
                --tray-only         : La app inicia oculta en la bandeja del sistema.";
                PInvoke.MessageBox(HWND.Null, msg, "Ayuda de argumentos",
                    MESSAGEBOX_STYLE.MB_OK | MESSAGEBOX_STYLE.MB_ICONHAND | MESSAGEBOX_STYLE.MB_TOPMOST);
                Environment.Exit(WIN32_ERROR.ERROR_SUCCESS.To<int>());
            }

            if (arg is "--tray-only")
                _trayOnly = true;
        }

        if (args.Any(a => a is "--reset-settings"))
            SettingsHelper.ResetSettings();
    }

}
