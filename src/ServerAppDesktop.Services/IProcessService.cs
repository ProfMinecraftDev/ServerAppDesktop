using System;
using System.Threading.Tasks;

namespace ServerAppDesktop.Services
{
    public interface IProcessService
    {
        event Action<string>? OutputReceived;
        event Action<string>? ErrorReceived;

        event Action<bool, int>? ProcessExited;

        bool IsRunning { get; }

        bool StartProcess(string fileName, string arguments, string workingDirectory);
        Task<bool> StopProcessAsync();

        void SendInput(string input);
    }
}