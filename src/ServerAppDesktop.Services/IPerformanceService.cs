namespace ServerAppDesktop.Services;

public interface IPerformanceService
{
    public int GetCpuUsagePercentage();
    public float GetCpuUsageInGHz();

    public int TotalMemory { get; }
    public int GetUsedMemory();
    public int GetUsedMemoryPercentage();

    public int GetDiskWriteSpeed();
    public int GetDiskReadSpeed();

    public int GetNetworkUploadSpeed();
    public int GetNetworkDownloadSpeed();
}
