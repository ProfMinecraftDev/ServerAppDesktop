namespace ServerAppDesktop.Services;

public interface IProcessService
{
    public event TypedEventHandler<IProcessService, ProcessDataReceivedEventArgs>? DataReceived;

    public event TypedEventHandler<IProcessService, ProcessExitedEventArgs>? ProcessExited;

    public event TypedEventHandler<IProcessService, PlayerCountChangedEventArgs>? PlayerCountChanged;

    public bool IsRunning { get; }

    public Task<bool> StartProcessAsync(string fileName, string arguments, string workingDirectory, int? ramLimit = null);
    public Task<bool> StopProcessAsync();

    public void SendInput(string input);
}
