namespace ServerAppDesktop.Helpers;

public sealed class DownloadProgressChangedEventArgs(string text, double progress) : EventArgs
{
    public string Text { get; init; } = text;
    public double Progress { get; init; } = progress;

    public DateTime Timestamp { get; init; } = DateTime.Now;
}
