namespace ServerAppDesktop.Services
{
    public interface IPerformanceService
    {
        int GetCpuUsagePercentage();
        float GetCpuUsageInGHz();

        int TotalMemory { get; }
        int GetUsedMemory();
        int GetUsedMemoryPercentage();

        int GetDiskWriteSpeed();
        int GetDiskReadSpeed();

        int GetNetworkUploadSpeed();
        int GetNetworkDownloadSpeed();
    }
}
