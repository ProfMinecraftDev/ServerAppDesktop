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
                string errorTitle = ResourceHelper.GetString("Cmd_Error_Title");
                string errorMsg = string.Format(ResourceHelper.GetString("Cmd_Error_Unknown"), arg);

                _ = PInvoke.MessageBox(
                    HWND.Null,
                    errorMsg,
                    errorTitle,
                    MESSAGEBOX_STYLE.MB_OK | MESSAGEBOX_STYLE.MB_ICONERROR | MESSAGEBOX_STYLE.MB_TOPMOST
                );
                Environment.Exit(WIN32_ERROR.ERROR_INVALID_PARAMETER.To<int>());
            }

            if (arg is "--version" or "-v")
            {
                string infoTitle = ResourceHelper.GetString("Cmd_Info_Title");
                string infoMsg = string.Format(ResourceHelper.GetString("Cmd_Info_Msg"), DataHelper.AppName, DataHelper.AppVersion);

                PInvoke.MessageBox(HWND.Null, infoMsg, infoTitle,
                    MESSAGEBOX_STYLE.MB_OK | MESSAGEBOX_STYLE.MB_ICONINFORMATION | MESSAGEBOX_STYLE.MB_TOPMOST);
                Environment.Exit(WIN32_ERROR.ERROR_SUCCESS.To<int>());
            }

            if (arg is "--help" or "-h")
            {
                string helpTitle = ResourceHelper.GetString("Cmd_Help_Title");
                string helpMsg = ResourceHelper.GetString("Cmd_Help_Msg");

                PInvoke.MessageBox(HWND.Null, helpMsg, helpTitle,
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
