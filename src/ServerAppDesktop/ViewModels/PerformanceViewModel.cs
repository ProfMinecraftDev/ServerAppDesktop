
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
        _processService.PlayerCountChanged += (players) =>
        {
            _ = MainWindow.Instance.DispatcherQueue.TryEnqueue(() => PlayersOnline = players.To<string>());
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
        if (IsServerRunning)
        {

            _ = Task.Run(() =>
                MainWindow.Instance?.DispatcherQueue.TryEnqueue(() =>
                {
                    CpuUsage = $"{_performanceService.GetCpuUsagePercentage()}% ({_performanceService.GetCpuUsageInGHz()} GHz)";
                    RamUsage = $"{_performanceService.GetUsedMemoryPercentage()}% ({_performanceService.GetUsedMemory()} MB / {_performanceService.TotalMemory} MB)";
                    NetworkUsage = $"{_performanceService.GetNetworkUploadSpeed()} KB/s ↑ | {_performanceService.GetNetworkDownloadSpeed()} KB/s ↓";
                    DiskUsage = $"{_performanceService.GetDiskWriteSpeed()} KB/s W | {_performanceService.GetDiskReadSpeed()} KB/s R";
                })
            );
        }
    }

    public void Receive(ServerStateChangedMessage message)
    {
        IsServerRunning = message.Value.State == ServerStateType.Running;
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
