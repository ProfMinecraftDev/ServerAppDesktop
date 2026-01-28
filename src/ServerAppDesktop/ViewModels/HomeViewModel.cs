using System;
using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Windows.AppNotifications.Builder;
using ServerAppDesktop.Controls;
using ServerAppDesktop.Helpers;
using ServerAppDesktop.Messaging;
using ServerAppDesktop.Models;
using ServerAppDesktop.Services;

namespace ServerAppDesktop.ViewModels;

public sealed partial class HomeViewModel : ObservableRecipient, IRecipient<ServerStateChangedMessage>
{
    private readonly TerminalViewModel _terminalViewModel;
    private readonly IOOBEService _oobeService;
    private readonly IProcessService _processService;


    // Flag de configuración (OOBE). Si es false, todo está bloqueado.

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanStartServer))]
    [NotifyPropertyChangedFor(nameof(CanStopServer))]
    [NotifyPropertyChangedFor(nameof(CanRestartServer))]
    [NotifyCanExecuteChangedFor(nameof(StartServerCommand))]
    [NotifyCanExecuteChangedFor(nameof(StopServerCommand))]
    [NotifyCanExecuteChangedFor(nameof(RestartServerCommand))]
    private bool _isConfigured = false;



    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanStartServer))]
    [NotifyPropertyChangedFor(nameof(CanStopServer))]
    [NotifyPropertyChangedFor(nameof(CanRestartServer))]
    private ServerState? _serverState;

    private static readonly string DefaultValue = ResourceHelper.GetString("NoneItem");

    [ObservableProperty] private string _serverEdition = DefaultValue;
    [ObservableProperty] private string _serverVersion = DefaultValue;
    [ObservableProperty] private string _serverPath = DefaultValue;
    [ObservableProperty] private string _serverExecutable = DefaultValue;
    [ObservableProperty] private string _serverIP = DefaultValue;
    [ObservableProperty] private string _serverIPType = "IPv4";
    [ObservableProperty] private string _serverPort = DefaultValue;
    [ObservableProperty] private string _serverPortType = "IPv4";
    [ObservableProperty] private string _activeWorld = DefaultValue;



    public bool CanStartServer => IsConfigured && (ServerState?.State is ServerStateType.Stopped or null);
    public bool CanStopServer => IsConfigured && (ServerState?.State is ServerStateType.Running);
    public bool CanRestartServer => IsConfigured && (ServerState?.State is ServerStateType.Running);

    public HomeViewModel(TerminalViewModel terminalViewModel, IOOBEService oobeService, IProcessService processService)
    {
        IsActive = true;
        _terminalViewModel = terminalViewModel;
        _oobeService = oobeService;
        _processService = processService;

        _processService.ProcessExited += (clean, code) => UpdateState(ServerStateType.Stopped, !clean && code != 0);
        _processService.OutputReceived += (output) =>
        {
            MainWindow.Instance.DispatcherQueue.TryEnqueue(() =>
            {
                _terminalViewModel.TerminalOutput += output + Environment.NewLine;
            });
        };
        _processService.ErrorReceived += (output) =>
        {
            MainWindow.Instance.DispatcherQueue.TryEnqueue(() =>
            {
                _terminalViewModel.TerminalOutput += output + Environment.NewLine;
            });
        };

        _processService.PlayerJoined += (count) =>
        {
            MainWindow.Instance.DispatcherQueue.TryEnqueue(() =>
            {
                _terminalViewModel.TerminalOutput += $"[Servidor] Un jugador se ha unido al servidor. ({count} en línea){Environment.NewLine}";
            });
        };
        _processService.PlayerLeft += (count) =>
        {
            MainWindow.Instance.DispatcherQueue.TryEnqueue(() =>
            {
                _terminalViewModel.TerminalOutput += $"[Servidor] Un jugador ha salido del servidor. ({count} en línea){Environment.NewLine}";
            });
        };

        _oobeService.OOBEFinished += (val) => IsConfigured = val;

        _serverState = new ServerState(ServerStateType.Stopped);
        IsConfigured = DataHelper.Settings != null;

        if (IsConfigured && DataHelper.Settings?.Startup.AutoStartServer == true)
            StartServer();
    }

    [RelayCommand(CanExecute = nameof(CanStartServer))]
    private void StartServer()
    {
        var s = DataHelper.Settings?.Server;
        if (s == null)
            return;

        UpdateState(ServerStateType.Starting);

        // Definimos quién manda realmente
        string fileName = s.Edition == 1 ? "java.exe" : s.Executable;

        // Construimos los argumentos sin el "/c" del CMD
        string args = s.Edition == 1
            ? $" --enable-native-access=ALL-UNNAMED -Xms{s.RamLimit}M -Xmx{s.RamLimit}M -jar \"{s.Executable}\" nogui"
            : ""; // Para Bedrock u otros, los argumentos suelen ir vacíos o personalizados

        bool success = _processService.StartProcess(fileName, args, s.Path);

        if (success)
        {
            UpdateState(ServerStateType.Running);
            Notify("Servidor Iniciado", "El proceso arrancó con éxito.");
        }
        else
        {
            // Si el servicio devuelve false, algo falló a nivel de SO
            UpdateState(ServerStateType.Stopped, true);
        }
    }

    [RelayCommand(CanExecute = nameof(CanStopServer))]
    private async Task StopServerAsync()
    {
        UpdateState(ServerStateType.Stopping);

        bool stopped = await _processService.StopProcessAsync();

        if (stopped)
        {
            UpdateState(ServerStateType.Stopped);
            Notify("Servidor Detenido", "El proceso se cerró correctamente.");
        }
        else
        {
            // No se pudo detener (quizás proceso zombie), notificamos error
            UpdateState(ServerStateType.Stopped, true);
        }
    }

    [RelayCommand(CanExecute = nameof(CanRestartServer))]
    private async Task RestartServerAsync()
    {
        UpdateState(ServerStateType.Restarting);
        await _processService.StopProcessAsync();
        StartServer();
    }

    private void UpdateState(ServerStateType state, bool isError = false)
    {
        var dispatcher = MainWindow.Instance?.DispatcherQueue;
        if (dispatcher == null)
            return;

        // Forzamos que TODO el cambio ocurra en el hilo de UI
        dispatcher.TryEnqueue(() =>
        {
            ServerState = new ServerState(state);

            // Notificamos a los comandos para que habiliten/deshabiliten botones
            StartServerCommand.NotifyCanExecuteChanged();
            StopServerCommand.NotifyCanExecuteChanged();
            RestartServerCommand.NotifyCanExecuteChanged();

            Messenger.Send(new ServerStateChangedMessage(ServerState));

            if (isError)
                Notify("Error", "La operación del servidor falló.", AppNotificationScenario.Urgent);
        });
    }

    private void Notify(string title, string msg, AppNotificationScenario scene = AppNotificationScenario.Default)
    {
        new WindowsNotification
        {
            Title = title,
            Message = msg,
            NotificationScenario = scene
        }.ShowNotification();
    }

    public void Receive(ServerStateChangedMessage m) => ServerState = m.Value;

    partial void OnServerStateChanged(ServerState? value)
    {
        if (value == null || !IsConfigured || MainWindow.Instance.TrayIcon == null)
            return;

        MainWindow.Instance.DispatcherQueue.TryEnqueue(() =>
        {
            try
            {
                // El Switch de GetTooltip ahora corre seguro aquí
                MainWindow.Instance.TrayIcon.ToolTipText = ServerUIHelper.GetTooltip(value.State);
                MainWindow.Instance.SetIcon(ServerUIHelper.GetIconPath(value.State));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error actualizando Tray: {ex.Message}");
            }
        });
    }
}