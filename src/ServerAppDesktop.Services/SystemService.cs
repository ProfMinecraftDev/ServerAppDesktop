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
        GPUName = GetWmiValue("SELECT Name FROM Win32_VideoController", "Name") ?? "Unknown GPU";
    }

    private void GetRamData()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Capacity FROM Win32_PhysicalMemory");
            long total = searcher.Get().Cast<ManagementObject>().Sum(x => Convert.ToInt64(x["Capacity"]));
            MemoryGB = (int)Math.Round(total / Math.Pow(1024, 3));
        }
        catch { MemoryGB = 0; }
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
            using var searcher = new ManagementObjectSearcher("SELECT Name, Version FROM Win32_ComputerSystemProduct");
            ManagementObject? obj = searcher.Get().Cast<ManagementObject>().FirstOrDefault();

            string name = obj?["Name"]?.ToString() ?? "";

            if (string.IsNullOrEmpty(name) || name.Contains("To be filled", StringComparison.OrdinalIgnoreCase))
            {
                using var boardSearcher = new ManagementObjectSearcher("SELECT Product FROM Win32_BaseBoard");
                name = boardSearcher.Get().Cast<ManagementObject>().FirstOrDefault()?["Product"]?.ToString() ?? "Custom PC";
            }

            PCModel = name;
        }
        catch { PCModel = "Unknown Model"; }
    }

    private void GetLicenseData()
    {
        try
        {
            string query = "SELECT Description FROM SoftwareLicensingProduct WHERE PartialProductKey IS NOT NULL AND ApplicationID = '55c92734-d682-4d71-983e-d6ec3f16059f'";
            string desc = GetWmiValue(query, "Description")?.ToUpper() ?? "";
            LicenseType = desc.Contains("RETAIL") ? "Retail" : desc.Contains("OEM") ? "OEM" : desc.Contains("VOLUME") ? "Volume" : "Original";
        }
        catch { LicenseType = "Unknown"; }
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
        ActiveUser = $"{Environment.UserName} @ {Environment.MachineName}";
    }

    private static string? GetWmiValue(string query, string prop)
    {
        try
        {
            using var searcher = new ManagementObjectSearcher(query);
            return searcher.Get().Cast<ManagementObject>().FirstOrDefault()?[prop]?.ToString();
        }
        catch { return null; }
    }
}
