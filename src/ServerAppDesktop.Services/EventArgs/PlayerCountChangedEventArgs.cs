namespace ServerAppDesktop.Services;

public sealed class PlayerCountChangedEventArgs : EventArgs
{
    public int CurrentPlayers { get; init; }
    public int Change { get; init; }
    public string? PlayerName { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.Now;

    public PlayerCountChangedEventArgs(int currentPlayers, int change, string? playerName = null)
    {
        CurrentPlayers = currentPlayers;
        Change = change;
        PlayerName = playerName;
    }
}
