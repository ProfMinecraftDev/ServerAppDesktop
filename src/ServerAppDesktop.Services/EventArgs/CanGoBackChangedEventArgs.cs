namespace ServerAppDesktop.Services;

public sealed class CanGoBackChangedEventArgs(bool canGoBack) : EventArgs
{
    public bool CanGoBack { get; init; } = canGoBack;
    public DateTime TimeStamp { get; } = DateTime.Now;
}
