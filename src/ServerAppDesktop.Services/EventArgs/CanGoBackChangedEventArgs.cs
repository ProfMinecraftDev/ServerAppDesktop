namespace ServerAppDesktop.Services;

public sealed class CanGoBackChangedEventArgs : EventArgs
{
    public bool CanGoBack { get; init; }

    public CanGoBackChangedEventArgs(bool canGoBack) => CanGoBack = canGoBack;
}
