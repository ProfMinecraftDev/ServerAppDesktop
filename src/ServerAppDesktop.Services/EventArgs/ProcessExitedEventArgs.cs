namespace ServerAppDesktop.Services;

public sealed class ProcessExitedEventArgs : EventArgs
{
    public bool IsSuccess { get; init; }

    public int ExitCode { get; init; }

    public DateTime ExitTime { get; init; } = DateTime.Now;

    public ProcessExitedEventArgs(bool isSuccess, int exitCode)
    {
        IsSuccess = isSuccess;
        ExitCode = exitCode;
    }
}
