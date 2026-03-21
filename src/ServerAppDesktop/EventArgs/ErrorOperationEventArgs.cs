namespace ServerAppDesktop;

public sealed class ErrorOperationEventArgs(string message, string details = "") : EventArgs
{
    public string Message { get; init; } = message;
    public string Details { get; init; } = details;
    public DateTime TimeStamp { get; } = DateTime.Now;
}
