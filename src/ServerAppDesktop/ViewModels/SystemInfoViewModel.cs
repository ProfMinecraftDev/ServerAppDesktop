namespace ServerAppDesktop.ViewModels;

public sealed partial class SystemInfoViewModel(ISystemService systemService) : ObservableObject
{
    private readonly ISystemService _systemService = systemService;

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

    public async Task LoadData()
    {
        IsLoading = true;

        CpuName = GpuName = MemoryInfo = PcModel = WindowsVersion =
        LicenseType = StorageType = StorageSize = ActiveUser =
        ActiveProcessName = ActiveProcessId = ResourceHelper.GetString("Process_Loading");

        await Task.Run(() =>
        {
            string cpu = _systemService.CPUName;
            string gpu = _systemService.GPUName;
            string mem = $"{_systemService.MemoryGB} GB";
            string model = _systemService.PCModel;
            string win = _systemService.WindowsVersion;
            string lic = _systemService.LicenseType;
            string sType = _systemService.StorageType;
            string sSize = _systemService.StorageSize;
            string user = _systemService.ActiveUser;
            string pName = _systemService.ActiveProcess;
            string pId = $"PID {_systemService.ActiveProcessId}";

            _ = MainWindow.Instance.DispatcherQueue.TryEnqueue(() =>
            {
                CpuName = cpu;
                GpuName = gpu;
                MemoryInfo = mem;
                PcModel = model;
                WindowsVersion = win;
                LicenseType = lic;
                StorageType = sType;
                StorageSize = sSize;
                ActiveUser = user;
                ActiveProcessName = pName;
                ActiveProcessId = pId;

                IsLoading = false;
            });
        });
    }
}
