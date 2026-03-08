namespace ServerAppDesktop.ViewModels;

public sealed partial class HomeViewModel : ObservableRecipient, IRecipient<AppSettingsChangedMessage>, IRecipient<ServerPropertiesChangedMessage>
{
    private readonly IOOBEService _oobeService;
    private readonly IProcessService _processService;
    private readonly INetworkService _networkService;
    private readonly IServerPropertiesService _serverPropertiesService;
    private readonly IWindowHandler _windowHandler;

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
    private ServerStateType _serverState;

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



    public bool CanStartServer => IsConfigured && (ServerState is ServerStateType.Stopped);
    public bool CanStopServer => IsConfigured && (ServerState is ServerStateType.Running);
    public bool CanRestartServer => IsConfigured && (ServerState is ServerStateType.Running);

    public HomeViewModel(
        IOOBEService oobeService,
        IProcessService processService,
        INetworkService networkService,
        IServerPropertiesService serverPropertiesService,
        IWindowHandler windowHandler
        )
    {
        IsActive = true;

        _oobeService = oobeService;
        _processService = processService;
        _networkService = networkService;
        _serverPropertiesService = serverPropertiesService;
        _windowHandler = windowHandler;

        _ = App.GetRequiredService<PerformanceViewModel>();
        _ = App.GetRequiredService<TerminalViewModel>();

        _processService.ProcessExited += (_, args) => UpdateState(ServerStateType.Stopped, !args.IsSuccess && args.ExitCode != 0, true);

        _oobeService.OOBEFinished += (_, args) => IsConfigured = args.IsSuccess;
    }

    [RelayCommand(CanExecute = nameof(CanStartServer))]
    private async Task StartServerAsync()
    {
        ServerSettings? s = DataHelper.Settings?.Server;
        if (s == null)
        {
            return;
        }

        UpdateState(ServerStateType.Starting);


        string fileName = s.Edition == 1 ? "java.exe" : s.Executable;


        string[] args = s.Edition == 1
            ? [
                "--enable-native-access=ALL-UNNAMED",
                $"-Duser.dir={s.Path}",
                "-Dfile.encoding=UTF-8",
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
                "--nogui"
            ]
            : [];

        ProcessHelper.SetEfficiencyMode(false);
        ProcessHelper.SetProcessPriorityClass(ProcessPriorityClass.High);
        ProcessHelper.SetProcessQualityOfServiceLevel(QualityOfServiceLevel.High);

        bool success = await _processService.StartProcessAsync(fileName, string.Join(" ", args), s.Path, s.Edition == 0 ? s.RamLimit : null);

        if (success)
        {
            UpdateState(ServerStateType.Running);
            Notify("Servidor Iniciado", "El proceso arrancó con éxito.");
        }
        else
        {
            ProcessHelper.SetProcessPriorityClass(ProcessPriorityClass.Normal);
            ProcessHelper.SetProcessQualityOfServiceLevel(QualityOfServiceLevel.Default);
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

            UpdateState(ServerStateType.Stopped, true);
        }
    }

    [RelayCommand(CanExecute = nameof(CanRestartServer))]
    private async Task RestartServerAsync()
    {
        UpdateState(ServerStateType.Restarting);
        _ = await _processService.StopProcessAsync();
        await StartServerAsync();
    }

    private void UpdateState(ServerStateType state, bool isError = false, bool byProcess = false)
    {
        DispatcherQueue? dispatcher = MainWindow.Instance?.DispatcherQueue;
        if (dispatcher == null)
        {
            return;
        }

        _ = dispatcher.TryEnqueue(() =>
        {

            if (ServerState == state)
            {
                return;
            }

            ServerState = state;

            if (state == ServerStateType.Stopped && _windowHandler.WindowHidden)
                ProcessHelper.SetEfficiencyMode(true);

            if (isError)
            {
                Notify("Error Crítico", "El servidor se detuvo de forma inesperada.", AppNotificationScenario.Urgent);
                WindowHelper.FlashTaskbarIcon(MainWindow.Instance);
            }
            else if (byProcess && state == ServerStateType.Stopped)
            {


                Notify("Servidor Cerrado", "El servidor se apagó correctamente desde el proceso.");
            }
        });
    }

    private static void Notify(string title, string msg, AppNotificationScenario scene = AppNotificationScenario.Default)
    {
        new WindowsNotification
        {
            Title = title,
            Message = msg,
            NotificationScenario = scene
        }.ShowNotification();
    }

    partial void OnServerStateChanged(ServerStateType value)
    {
        if (!IsConfigured || MainWindow.Instance.TrayIcon == null)
            return;

        _ = MainWindow.Instance.DispatcherQueue.TryEnqueue(() =>
        {
            try
            {

                StartServerCommand.NotifyCanExecuteChanged();
                StopServerCommand.NotifyCanExecuteChanged();
                RestartServerCommand.NotifyCanExecuteChanged();

                _ = Messenger.Send(new ServerStateChangedMessage(value));

                _windowHandler.SetBadgeIcon(ServerUIHelper.GetBadgeIconPath(value));
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
            if (DataHelper.Settings == null)
                return;

            _serverPropertiesService.SetPath(DataHelper.Settings.Server.Path);

            if (DataHelper.Settings.Startup.AutoStartServer)
                _ = StartServerAsync();
            else
                ServerState = ServerStateType.Stopped;

            ServerEdition = DataHelper.Settings.Server.Edition == 0 ? "Bedrock" : "Java";
            ServerPath = DataHelper.Settings.Server.Path;
            ServerExecutable = DataHelper.Settings.Server.Executable;
            ServerPort = $"{_serverPropertiesService.GetValue<int>("server-port")}";
            ActiveWorld = _serverPropertiesService.GetValue<string>("level-name") ?? DefaultValue;

            _ = Task.Run(async () =>
            {
                string publicIP = await _networkService.GetPublicIPAsync();
                string localIP = _networkService.GetLocalIP();

                ServerIP = $"""
                {localIP} (Local)
                {publicIP} (Pública)
                """;
            });
        }
        else
        {
            ServerState = ServerStateType.Default;
        }
    }

    public void Receive(AppSettingsChangedMessage message)
    {
        ServerEdition = message.Value.Server.Edition == 0 ? "Bedrock" : "Java";
        ServerPath = message.Value.Server.Path;
        ServerExecutable = message.Value.Server.Executable;
    }

    public void Receive(ServerPropertiesChangedMessage message)
    {
        ServerPort = $"{_serverPropertiesService.GetValue<int>("server-port")}";
        ActiveWorld = _serverPropertiesService.GetValue<string>("level-name") ?? DefaultValue;
    }
}
