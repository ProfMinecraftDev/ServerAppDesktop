namespace ServerAppDesktop.Models;

public enum ServerStateType
{
    Stopped,
    Starting,
    Running,
    Stopping,
    Restarting
}

// El modelo ahora es solo el "contenedor" del estado
public record ServerState(ServerStateType State);