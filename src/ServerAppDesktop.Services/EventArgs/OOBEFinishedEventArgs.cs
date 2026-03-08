namespace ServerAppDesktop.Services;

public sealed class OOBEFinishedEventArgs : EventArgs
{
    public bool IsSuccess { get; init; }

    public DateTime CompletedAt { get; init; } = DateTime.Now;

    public OOBEFinishedEventArgs(bool isSuccess) => IsSuccess = isSuccess;
}
