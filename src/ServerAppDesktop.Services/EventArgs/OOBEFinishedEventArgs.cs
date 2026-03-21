namespace ServerAppDesktop.Services;

public sealed class OOBEFinishedEventArgs(bool isSuccess) : EventArgs
{
    public bool IsSuccess { get; init; } = isSuccess;

    public DateTime TimeStamp { get; init; } = DateTime.Now;
}
