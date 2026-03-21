namespace ServerAppDesktop.Services;

public sealed class ProcessDataReceivedEventArgs(string data, bool isError) : EventArgs
{
    public string Data { get; init; } = data;
    public bool IsError { get; init; } = isError;
    public DateTime TimeStamp { get; init; } = DateTime.Now;
}
