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

public sealed partial class HomeViewModel : ObservableRecipient
{
    private readonly IOOBEService _oobeService;
    private readonly IProcessService _processService;
    private readonly INetworkService _networkService;

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

    public HomeViewModel(IOOBEService oobeService, IProcessService processService, INetworkService networkService)
    {
        _oobeService = oobeService;
        _processService = processService;
        _networkService = networkService;

        _processService.ProcessExited += (clean, code) => UpdateState(ServerStateType.Stopped, !clean && code != 0, true);

        _oobeService.OOBEFinished += (val) => IsConfigured = val;

        _serverState = new ServerState(ServerStateType.Stopped);
        IsConfigured = DataHelper.Settings != null;
        _networkService = networkService;
    }

    [RelayCommand(CanExecute = nameof(CanStartServer))]
    private async Task StartServerAsync()
    {
        var s = DataHelper.Settings?.Server;
        if (s == null)
            return;

        UpdateState(ServerStateType.Starting);

        // Definimos quién manda realmente
        string fileName = s.Edition == 1 ? "java.exe" : s.Executable;

        // Construimos los argumentos sin el "/c" del CMD
        string[] args = s.Edition == 1
            ? [
                "--enable-native-access=ALL-UNNAMED",
                "-Dfile.encoding=UTF-8",
                $"-Xms{s.RamLimit}M",
                $"-Xmx{s.RamLimit}M",
                "-XX:+UseG1GC",
                "-XX:+ParallelRefProcEnabled",
                "-XX:MaxGCPauseMillis=200",
                "-XX:+UnlockExperimentalVMOptions",
                "-XX:+DisableExplicitGC",
                "-XX:G1NewSizePercent=30",
                "-XX:G1MaxNewSizePercent=40",
                "-XX:G1HeapRegionSize=8M",
                "-XX:G1ReservePercent=20",
                "-XX:G1HeapWastePercent=5",
                "-XX:G1MixedGCCountTarget=4",
                "-XX:InitiatingHeapOccupancyPercent=15",
                "-XX:G1MixedGCLiveThresholdPercent=90",
                "-XX:G1RSetUpdatingPauseTimePercent=5",
                "-XX:SurvivorRatio=32",
                "-XX:+PerfDisableSharedMem",
                "-XX:MaxTenuringThreshold=1",
                $"-jar \"{s.Executable}\"",
                "nogui"
            ]
            : []; // Para Bedrock u otros, los argumentos suelen ir vacíos o personalizados

        bool success = await _processService.StartProcessAsync(fileName, string.Join(" ", args), s.Path);

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
        await StartServerAsync();
    }

    private void UpdateState(ServerStateType state, bool isError = false, bool byProcess = false)
    {
        var dispatcher = MainWindow.Instance?.DispatcherQueue;
        if (dispatcher == null)
            return;

        dispatcher.TryEnqueue(() =>
        {
            // 1. Evitamos actualizaciones innecesarias si el estado es el mismo
            if (ServerState?.State == state)
                return;

            ServerState = new ServerState(state);

            // 3. Informamos al resto de la App vía Messenger
            WeakReferenceMessenger.Default.Send(new ServerStateChangedMessage(ServerState));

            // 4. Lógica de Notificaciones Inteligente
            if (isError)
            {
                Notify("Error Crítico", "El servidor se detuvo de forma inesperada.", AppNotificationScenario.Urgent);
            }
            else if (byProcess && state == ServerStateType.Stopped)
            {
                // Si se cerró "por el proceso" y no fue un error, 
                // probablemente fue un /stop desde dentro del juego.
                Notify("Servidor Cerrado", "El servidor se apagó correctamente desde el proceso.");
            }
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

    partial void OnServerStateChanged(ServerState? value)
    {
        if (value == null || !IsConfigured || MainWindow.Instance.TrayIcon == null)
            return;

        MainWindow.Instance.DispatcherQueue.TryEnqueue(() =>
        {
            try
            {
                // Notificamos a los comandos (UI)
                StartServerCommand.NotifyCanExecuteChanged();
                StopServerCommand.NotifyCanExecuteChanged();
                RestartServerCommand.NotifyCanExecuteChanged();

                // El Switch de GetTooltip ahora corre seguro aquí
                MainWindow.Instance.SetIcon(ServerUIHelper.GetIconPath(value.State));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error actualizando Tray: {ex.Message}");
            }
        });
    }

    partial void OnIsConfiguredChanged(bool value)
    {
        if (value)
        {
            // 1. Despertamos a los chismosos apenas sepamos que está configurado
            _ = App.GetRequiredService<PerformanceViewModel>();
            _ = App.GetRequiredService<TerminalViewModel>();

            // 2. Si el usuario activó el AutoStart, le damos candela al server
            if (DataHelper.Settings?.Startup.AutoStartServer == true)
            {
                _ = StartServerAsync();
            }

            _ = Task.Run(async () =>
            {
                string publicIP = await _networkService.GetPublicIPAsync();

                MainWindow.Instance.DispatcherQueue.TryEnqueue(() =>
                {
                    ServerIP = $"""
                {_networkService.GetLocalIP()} (Local)
                {publicIP} (Pública)
                """;
                });
            });
        }
        else
        {
            // Si por alguna razón vuelve a 'false' (un reset), paramos el estado
            ServerState = new ServerState(ServerStateType.Stopped);
        }
    }
}