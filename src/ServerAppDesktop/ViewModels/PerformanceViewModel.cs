
namespace ServerAppDesktop.ViewModels;

public sealed partial class PerformanceViewModel : ObservableRecipient, IRecipient<ServerStateChangedMessage>
{
    private readonly IProcessService _processService;
    private readonly IPerformanceService _performanceService;
    private readonly DispatcherTimer _timer;

    [ObservableProperty]
    private bool _isServerRunning;

    [ObservableProperty]
    private string _cpuUsage = ResourceHelper.GetString("DisconnectedString");

    [ObservableProperty]
    private string _ramUsage = ResourceHelper.GetString("DisconnectedString");

    [ObservableProperty]
    private string _networkUsage = ResourceHelper.GetString("DisconnectedString");

    [ObservableProperty]
    private string _diskUsage = ResourceHelper.GetString("DisconnectedString");

    [ObservableProperty]
    private string _playersOnline = ResourceHelper.GetString("DisconnectedString");

    public PerformanceViewModel(IProcessService processService, IPerformanceService performanceService)
    {
        IsActive = true;
        _processService = processService;
        _performanceService = performanceService;
        _processService.PlayerCountChanged += (s, args) =>
        {
            _ = MainWindow.Instance.DispatcherQueue.TryEnqueue(() =>
            {
                PlayersOnline = args.CurrentPlayers.To<string>();
            });
        };
        IsServerRunning = false;
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += OnTick;
    }

    private void OnTick(object? sender, object e)
    {
        if (!IsServerRunning)
        {
            return;
        }

        _ = Task.Run(() =>
        {
            _performanceService.Refresh();

            string cpu = $"{_performanceService.GetCpuUsagePercentage()}% ({_performanceService.GetCpuUsageInGHz()} GHz)";
            string ram = $"{_performanceService.GetUsedMemoryPercentage()}% ({_performanceService.GetUsedMemory()} MB / {_performanceService.TotalMemory} MB)";
            string net = $"{_performanceService.GetNetworkUploadSpeed()} KB/s ↑ | {_performanceService.GetNetworkDownloadSpeed()} KB/s ↓";
            string disk = $"{_performanceService.GetDiskWriteSpeed()} KB/s W | {_performanceService.GetDiskReadSpeed()} KB/s R";

            _ = MainWindow.Instance.DispatcherQueue.TryEnqueue(() =>
            {
                CpuUsage = cpu;
                RamUsage = ram;
                NetworkUsage = net;
                DiskUsage = disk;
            });
        });
    }

    public void Receive(ServerStateChangedMessage message)
    {
        IsServerRunning = message.Value == ServerStateType.Running;
    }

    partial void OnIsServerRunningChanged(bool value)
    {
        if (value)
        {
            _timer.Start();
            PlayersOnline = "0";
        }
        else
        {
            _timer.Stop();
            CpuUsage = ResourceHelper.GetString("DisconnectedString");
            RamUsage = ResourceHelper.GetString("DisconnectedString");
            NetworkUsage = ResourceHelper.GetString("DisconnectedString");
            DiskUsage = ResourceHelper.GetString("DisconnectedString");
            PlayersOnline = ResourceHelper.GetString("DisconnectedString");
        }
    }
}
