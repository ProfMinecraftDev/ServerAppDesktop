namespace ServerAppDesktop;

public sealed class SuccessOperationEventArgs(string message) : EventArgs
{
    public string Message { get; init; } = message;
    public DateTime TimeStamp { get; } = DateTime.Now;
}
