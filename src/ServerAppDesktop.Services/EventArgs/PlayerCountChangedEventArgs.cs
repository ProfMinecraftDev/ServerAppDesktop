namespace ServerAppDesktop.Services;

public sealed class PlayerCountChangedEventArgs(int currentPlayers, int change, string? playerName = null) : EventArgs
{
    public int CurrentPlayers { get; init; } = currentPlayers;
    public int Change { get; init; } = change;
    public string? PlayerName { get; init; } = playerName;
    public DateTime Timestamp { get; init; } = DateTime.Now;
}
