namespace ServerAppDesktop.Helpers;

public static class ServerUIHelper
{



    public static string GetIconPath(ServerStateType state)
    {
        return state switch
        {
            ServerStateType.Starting or
            ServerStateType.Stopping or
            ServerStateType.Restarting
                => $"{AppContext.BaseDirectory}/Assets/AppIcon_ServerRestarting.ico",

            ServerStateType.Running
                => $"{AppContext.BaseDirectory}/Assets/AppIcon_ServerRunning.ico",

            ServerStateType.Stopped
                => $"{AppContext.BaseDirectory}/Assets/AppIcon_ServerStopped.ico",

            _ => $"{AppContext.BaseDirectory}/Assets/AppIcon.ico",
        };
    }

    public static string GetStateString(ServerStateType state)
    {
        return state switch
        {
            ServerStateType.Starting => "Iniciando...",
            ServerStateType.Running => "En ejecución",
            ServerStateType.Restarting => "Reiniciando...",
            ServerStateType.Stopping => "Deteniendo...",
            ServerStateType.Stopped => "Detenido",
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
