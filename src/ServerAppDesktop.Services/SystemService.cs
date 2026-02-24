namespace ServerAppDesktop.Services;

public sealed class SystemService : ISystemService
{
    public string CPUName { get; set; } = "";
    public string GPUName { get; set; } = "";
    public int MemoryGB { get; set; } = 0;
    public string PCModel { get; set; } = "";
    public string WindowsVersion { get; set; } = "";
    public string LicenseType { get; set; } = "";
    public string ActiveProcess { get; set; } = "";
    public int ActiveProcessId { get; set; } = 0;
    public string StorageType { get; set; } = "";
    public string StorageSize { get; set; } = "";
    public string ActiveUser { get; set; } = "";

    public SystemService()
    {
        Parallel.Invoke(
        GetCpuData, GetGpuData, GetRamData, GetPcModel,
        GetOsData, GetLicenseData, GetProcessData, GetStorageData,
        GetUserData);
    }

    private void GetCpuData()
    {
        const string path = @"HKEY_LOCAL_MACHINE\Hardware\Description\System\CentralProcessor\0";
        CPUName = Registry.GetValue(path, "ProcessorNameString", null)?.ToString()?.Trim() ?? "Unknown CPU";
    }

    private void GetGpuData()
    {
        try
        {
            const string videoKey = @"HKEY_LOCAL_MACHINE\HARDWARE\DEVICEMAP\VIDEO";
            string? devicePath = Registry.GetValue(videoKey, @"\Device\Video0", null)?.ToString();

            if (!string.IsNullOrEmpty(devicePath))
            {
                string keyPath = devicePath.Replace(@"\Registry\Machine\", "");
                string fullPath = $@"HKEY_LOCAL_MACHINE\{keyPath}";

                string? gpuName = Registry.GetValue(fullPath, "DriverDesc", null)?.ToString();

                if (string.IsNullOrEmpty(gpuName))
                {
                    gpuName = Registry.GetValue(fullPath, "Device Description", null)?.ToString();
                }

                GPUName = gpuName ?? "Standard VGA Adapter";
            }
            else
            {
                GPUName = "GPU No Detectada";
            }
        }
        catch
        {
            GPUName = "Unknown GPU";
        }
    }

    private void GetRamData()
    {
        try
        {
            MEMORYSTATUSEX memStatus = new()
            {
                dwLength = (uint)Marshal.SizeOf<MEMORYSTATUSEX>()
            };

            if (PInvoke.GlobalMemoryStatusEx(ref memStatus))
            {
                double totalGB = memStatus.ullTotalPhys / Math.Pow(1024, 3);

                MemoryGB = (int)Math.Ceiling(totalGB);
            }
        }
        catch
        {
            MemoryGB = 0;
        }
    }

    private void GetOsData()
    {
        try
        {
            const string path = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion";
            int build = int.Parse(Registry.GetValue(path, "CurrentBuild", "0")?.ToString() ?? "0");
            string ver = Registry.GetValue(path, "DisplayVersion", "XXHX")?.ToString() ?? "XXHX";
            string rev = Registry.GetValue(path, "UBR", "0")?.ToString() ?? "0";
            Version v = Environment.OSVersion.Version;
            WindowsVersion = $"{(build >= 22000 ? "11" : "10")}, {ver} ({v.Major}.{v.Minor}.{build}.{rev})";
        }
        catch { WindowsVersion = "Unknown Windows"; }
    }

    private void GetPcModel()
    {
        try
        {
            const string biblePath = @"HKEY_LOCAL_MACHINE\HARDWARE\DESCRIPTION\System\BIOS";

            string? systemModel = Registry.GetValue(biblePath, "SystemProductName", null)?.ToString();

            string? boardModel = Registry.GetValue(biblePath, "BaseBoardProduct", null)?.ToString();

            PCModel = string.IsNullOrEmpty(systemModel) ||
                systemModel.Contains("To be filled", StringComparison.OrdinalIgnoreCase) ||
                systemModel.Contains("Default string", StringComparison.OrdinalIgnoreCase)
                ? !string.IsNullOrEmpty(boardModel) ? boardModel : "Custom PC"
                : systemModel;
        }
        catch
        {
            PCModel = "Unknown Model";
        }
    }

    private void GetLicenseData()
    {
        try
        {
            const string path = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion";
            string? editionId = Registry.GetValue(path, "EditionID", "")?.ToString()?.ToUpper();
            string? productId = Registry.GetValue(path, "ProductId", "")?.ToString();

            LicenseType = productId != null && productId.Contains("OEM")
                ? "OEM"
                : editionId != null && editionId.Contains("ENTERPRISE") ? "Volume (KMS/MAK)" : "Retail / Digital";
        }
        catch
        {
            LicenseType = "Unknown";
        }
    }

    private void GetProcessData()
    {
        var current = Process.GetCurrentProcess();
        string? filePath = current.MainModule?.FileName;
        ActiveProcess = Path.GetFileName(filePath) ?? "Unknown Process";
        ActiveProcessId = current.Id;
    }

    private void GetStorageData()
    {
        try
        {
            var scope = new ManagementScope(@"\\.\root\Microsoft\Windows\Storage");
            using var searcher = new ManagementObjectSearcher(scope, new ObjectQuery("SELECT MediaType, BusType FROM MSFT_PhysicalDisk"));
            ManagementObject? disk = searcher.Get().Cast<ManagementObject>().FirstOrDefault();
            if (disk != null)
            {
                ushort media = (ushort)disk["MediaType"];
                ushort bus = (ushort)disk["BusType"];
                StorageType = media == 4 ? (bus == 17 ? "SSD NVMe (M.2)" : "SSD SATA") : media == 3 ? "HDD" : "Unspecified";
            }

            using var driveSearcher = new ManagementObjectSearcher("SELECT Size FROM Win32_DiskDrive WHERE Index = 0");
            long bytes = Convert.ToInt64(driveSearcher.Get().Cast<ManagementObject>().FirstOrDefault()?["Size"]);
            double gb = bytes / Math.Pow(1024, 3);
            StorageSize = gb >= 900 ? $"{Math.Round(gb / 1024.0, 1)} TB" : $"{Math.Round(gb)} GB";
        }
        catch { StorageType = "Unknown"; StorageSize = "Unknown"; }
    }

    private void GetUserData()
    {
        ActiveUser = $"\\\\{Environment.MachineName}\\{Environment.UserName}";
    }
}
