using WinRT.Interop;

namespace ServerAppDesktop.Helpers;

public static class ServerUIHelper
{
    private const uint SEE_MASK_INVOKEIDLIST = 0x0000000C;

    public static unsafe void ShowFileProperties(string path, Window window)
    {
        if (string.IsNullOrEmpty(path))
            return;

        IntPtr hwnd = WindowNative.GetWindowHandle(window);

        fixed (char* pFile = path)
        fixed (char* pVerb = "properties")
        {
            SHELLEXECUTEINFOW info = new()
            {
                cbSize = (uint)sizeof(SHELLEXECUTEINFOW),
                fMask = SEE_MASK_INVOKEIDLIST,
                lpVerb = pVerb,
                lpFile = pFile,
                nShow = (int)SHOW_WINDOW_CMD.SW_SHOW,
                hwnd = (HWND)hwnd
            };

            if (!PInvoke.ShellExecuteEx(&info))
            {
                int errorCode = Marshal.GetLastWin32Error();
                System.Diagnostics.Debug.WriteLine(string.Format(ResourceHelper.GetString("ShellError"), errorCode));
            }
        }
    }
    public static string GetIconPath(ServerStateType state)
    {
        return state switch
        {
            ServerStateType.Starting or
            ServerStateType.Stopping or
            ServerStateType.Restarting
                => $"{AppContext.BaseDirectory}/Assets/States/ServerRestarting.ico",

            ServerStateType.Running
                => $"{AppContext.BaseDirectory}/Assets/States/ServerRunning.ico",

            ServerStateType.Stopped
                => $"{AppContext.BaseDirectory}/Assets/States/ServerStopped.ico",

            _ => $"{AppContext.BaseDirectory}/Assets/AppIcon.ico",
        };
    }

    public static string GetBadgeIconPath(ServerStateType state)
    {
        return state switch
        {
            ServerStateType.Starting or
            ServerStateType.Stopping or
            ServerStateType.Restarting
                => $"{AppContext.BaseDirectory}/Assets/Badges/Restarting.ico",

            ServerStateType.Running
                => $"{AppContext.BaseDirectory}/Assets/Badges/Running.ico",

            ServerStateType.Stopped
                => $"{AppContext.BaseDirectory}/Assets/Badges/Stopped.ico",

            _ => $"",
        };
    }

    public static string GetStateString(ServerStateType state)
    {
        return state switch
        {
            ServerStateType.Starting => ResourceHelper.GetString("ServerState_Starting"),
            ServerStateType.Running => ResourceHelper.GetString("ServerState_Running"),
            ServerStateType.Restarting => ResourceHelper.GetString("ServerState_Restarting"),
            ServerStateType.Stopping => ResourceHelper.GetString("ServerState_Stopping"),
            ServerStateType.Stopped => ResourceHelper.GetString("ServerState_Stopped"),
            _ => ResourceHelper.GetString("NoneItem")
        };
    }

    public static string GetTooltip(ServerStateType state)
    {
        return state switch
        {
            ServerStateType.Starting => ResourceHelper.GetString("TrayTooltip_ServerStarting"),
            ServerStateType.Running => ResourceHelper.GetString("TrayTooltip_ServerRunning"),
            ServerStateType.Stopping => ResourceHelper.GetString("TrayTooltip_ServerStopping"),
            ServerStateType.Stopped => ResourceHelper.GetString("TrayTooltip_ServerStopped"),
            ServerStateType.Restarting => ResourceHelper.GetString("TrayTooltip_ServerRestarting"),
            _ => "Server App Desktop (Preview)",
        };
    }
}
