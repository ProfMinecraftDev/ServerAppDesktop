namespace ServerAppDesktop.Services;

public sealed class ProcessDataReceivedEventArgs : EventArgs
{
    public string Data { get; init; }
    public bool IsError { get; init; }
    public DateTime ReceivedAt { get; init; } = DateTime.Now;

    public ProcessDataReceivedEventArgs(string data, bool isError)
    {
        Data = data;
        IsError = isError;
    }
}
