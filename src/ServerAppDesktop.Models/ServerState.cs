namespace ServerAppDesktop.Models;

public enum ServerStateType
{
    Stopped,
    Starting,
    Running,
    Stopping,
    Restarting
}


public record ServerState(ServerStateType State);
