using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ServerAppDesktop.Helpers;
using ServerAppDesktop.Messaging;
using ServerAppDesktop.Models;
using ServerAppDesktop.Services;

namespace ServerAppDesktop.ViewModels
{
    public sealed partial class TerminalViewModel : ObservableRecipient, IRecipient<ServerStateChangedMessage>
    {
        private bool _isRunning = false;
        private readonly IProcessService _processService;

        [ObservableProperty]
        private string _terminalOutput = string.Empty;

        [ObservableProperty]
        private bool _canInput = false;

        [ObservableProperty]
        private bool _canSendCommand = false;

        [ObservableProperty]
        private string _commandInput = string.Empty;

        public TerminalViewModel(IProcessService processService)
        {
            IsActive = true;
            _processService = processService;
            _processService.OutputReceived += (output) =>
            {
                MainWindow.Instance.DispatcherQueue.TryEnqueue(() =>
                {
                    TerminalOutput += output + Environment.NewLine;
                });
            };
            _processService.ErrorReceived += (output) =>
            {
                MainWindow.Instance.DispatcherQueue.TryEnqueue(() =>
                {
                    TerminalOutput += output + Environment.NewLine;
                });
            };
        }

        public void Receive(ServerStateChangedMessage message)
        {
            if (message.Value.State == ServerStateType.Running)
            {
                CanInput = true;
                _isRunning = true;
                CommandInput = "";
            }
            else if (message.Value.State == ServerStateType.Stopped)
            {
                CanInput = false;
                _isRunning = false;
                CommandInput = "";
            }
        }

        partial void OnCommandInputChanged(string value)
        {
            CanSendCommand = !string.IsNullOrWhiteSpace(value) && CanInput;
        }

        [RelayCommand]
        private void SendInput()
        {
            if (_isRunning && CanInput && !string.IsNullOrWhiteSpace(CommandInput))
            {
                _processService.SendInput(CommandInput);
                TerminalOutput += $"> {CommandInput}{Environment.NewLine}";
                CommandInput = string.Empty;
            }
        }

        [RelayCommand]
        private void ClearOutput()
        {
            TerminalOutput = string.Empty;
        }

        [RelayCommand]
        private void CopyOutput()
        {
            if (TerminalOutput != string.Empty)
                MainWindow.Instance.DispatcherQueue.TryEnqueue(() =>
                {
                    ClipboardHelper.SetText(TerminalOutput);
                });
        }
    }
}
