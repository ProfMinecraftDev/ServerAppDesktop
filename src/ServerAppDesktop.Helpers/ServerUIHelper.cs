using System;
using ServerAppDesktop.Models;

namespace ServerAppDesktop.Helpers;

public static class ServerUIHelper
{
    /// <summary>
    /// Obtiene la ruta del icono según el estado del servidor.
    /// </summary>
    public static string GetIconPath(ServerStateType state) => state switch
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

    /// <summary>
    /// Obtiene el texto del Tooltip traducido según el estado.
    /// </summary>
    public static string GetTooltip(ServerStateType state) => state switch
    {
        ServerStateType.Starting => ResourceHelper.GetString("TrayTooltip_ServerStarting"),
        ServerStateType.Running => ResourceHelper.GetString("TrayTooltip_ServerRunning"),
        ServerStateType.Stopping => ResourceHelper.GetString("TrayTooltip_ServerStopping"),
        ServerStateType.Stopped => ResourceHelper.GetString("TrayTooltip_ServerStopped"),
        ServerStateType.Restarting => ResourceHelper.GetString("TrayTooltip_ServerRestarting"),
        _ => "Server App Desktop (Preview)",
    };
}