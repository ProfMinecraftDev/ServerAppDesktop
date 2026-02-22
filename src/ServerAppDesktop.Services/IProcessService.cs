namespace ServerAppDesktop.Services;

public interface IProcessService
{
    public event Action<string>? OutputReceived;
    public event Action<string>? ErrorReceived;

    public event Action<bool, int>? ProcessExited;

    public event Action<int>? PlayerCountChanged;

    public bool IsRunning { get; }

    public Task<bool> StartProcessAsync(string fileName, string arguments, string workingDirectory, int? ramLimit = null);
    public Task<bool> StopProcessAsync();

    public void SendInput(string input);
}
