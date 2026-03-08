namespace ServerAppDesktop.Helpers;

public sealed class DownloadProgressChangedEventArgs : EventArgs
{
    public string Text { get; init; }
    public double Progress { get; init; }

    public DateTime Timestamp { get; init; } = DateTime.Now;

    public DownloadProgressChangedEventArgs(string text, double progress)
    {
        Text = text;
        Progress = progress;
    }
}
