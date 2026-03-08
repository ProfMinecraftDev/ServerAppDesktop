

namespace ServerAppDesktop;

public static class EntryPoint
{
    [STAThread]
    private static async Task<int> Main(string[] argsRaw)
    {
        if (Environment.OSVersion.Version.Build < 19041)
        {
            ShowError("Error de Compatibilidad", "Requiere Windows 10 Build 19041+.");
            return WIN32_ERROR.ERROR_OLD_WIN_VERSION.To<int>();
        }

        string[] args = [.. argsRaw.Select(s => s.Trim().ToLowerInvariant())];
        ComWrappersSupport.InitializeComWrappers();
        PInvoke.SetCurrentProcessExplicitAppUserModelID(DataHelper.AppUserModelID);

        AppNotificationManager.Default.NotificationInvoked += NotificationHandler.NotificationInvoked;
        AppNotificationManager.Default.Register(DataHelper.AppName, new Uri(Path.Combine(AppContext.BaseDirectory, "Assets", "AppIcon.ico")));

        if (await WindowHelper.IsRedirectedAsync())
            return WIN32_ERROR.ERROR_SUCCESS.To<int>();

        if (!args.IsNullOrEmpty())
            CommandLineHandler.Handle(args);

        try
        {
            Application.Start((p) =>
            {
                var context = new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread());
                SynchronizationContext.SetSynchronizationContext(context);
                new App(CommandLineHandler.TrayOnly);
            });
            return WIN32_ERROR.ERROR_SUCCESS.To<int>();
        }
        catch (Exception ex) { return ex.HResult; }
    }

    private static void ShowError(string title, string msg) =>
        PInvoke.MessageBox(HWND.Null, msg, title, MESSAGEBOX_STYLE.MB_OK | MESSAGEBOX_STYLE.MB_ICONERROR | MESSAGEBOX_STYLE.MB_TOPMOST);
}
