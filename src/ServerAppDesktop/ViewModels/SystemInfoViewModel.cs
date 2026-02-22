namespace ServerAppDesktop.ViewModels;

public sealed partial class SystemInfoViewModel : ObservableObject
{
    private readonly ISystemService _systemService;

    [ObservableProperty] private string _cpuName = "";
    [ObservableProperty] private string _gpuName = "";
    [ObservableProperty] private string _memoryInfo = "";
    [ObservableProperty] private string _pcModel = "";
    [ObservableProperty] private string _windowsVersion = "";
    [ObservableProperty] private string _licenseType = "";
    [ObservableProperty] private string _storageType = "";
    [ObservableProperty] private string _storageSize = "";
    [ObservableProperty] private string _activeUser = "";
    [ObservableProperty] private string _activeProcessName = "";
    [ObservableProperty] private string _activeProcessId = "";
    [ObservableProperty] private bool _isLoading;

    public SystemInfoViewModel(ISystemService systemService)
    {
        _systemService = systemService;
        LoadData();
    }

    private void LoadData()
    {
        IsLoading = true;

        Parallel.Invoke(
            () => CpuName = _systemService.CPUName,
            () => GpuName = _systemService.GPUName,
            () => MemoryInfo = $"{_systemService.MemoryGB} GB",
            () => PcModel = _systemService.PCModel,
            () => WindowsVersion = _systemService.WindowsVersion,
            () => LicenseType = _systemService.LicenseType,
            () => StorageType = _systemService.StorageType,
            () => StorageSize = _systemService.StorageSize,
            () => ActiveUser = _systemService.ActiveUser,
            () => ActiveProcessName = _systemService.ActiveProcess,
            () => ActiveProcessId = $"PID {_systemService.ActiveProcessId}"
        );

        IsLoading = false;
    }
}
