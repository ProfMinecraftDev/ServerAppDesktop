namespace ServerAppDesktop.Services
{
    public interface IProcessService
    {
        event Action<string>? OutputReceived;
        event Action<string>? ErrorReceived;

        event Action<bool, int>? ProcessExited;

        event Action<int>? PlayerCountChanged;

        bool IsRunning { get; }

        Task<bool> StartProcessAsync(string fileName, string arguments, string workingDirectory, int? ramLimit = null);
        Task<bool> StopProcessAsync();

        void SendInput(string input);
    }
}
