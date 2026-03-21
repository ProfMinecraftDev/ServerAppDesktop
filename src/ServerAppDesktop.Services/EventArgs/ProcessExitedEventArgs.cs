namespace ServerAppDesktop.Services;

public sealed class ProcessExitedEventArgs(bool isSuccess, int exitCode) : EventArgs
{
    public bool IsSuccess { get; init; } = isSuccess;

    public int ExitCode { get; init; } = exitCode;

    public DateTime TimeStamp { get; init; } = DateTime.Now;
}
