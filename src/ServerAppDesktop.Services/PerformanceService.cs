namespace ServerAppDesktop.Services;

public sealed class PerformanceService : IPerformanceService
{
    private PerformanceCounter? _cpuCounter, _ramCounter, _diskWriteCounter, _diskReadCounter;
    private PerformanceCounter? _networkUploadCounter, _networkDownloadCounter;
    private PerformanceCounter? _pBase, _pPercent;

    public int TotalMemory { get; }
    public bool IsInitialized { get; private set; }

    public PerformanceService()
    {

        GCMemoryInfo gcInfo = GC.GetGCMemoryInfo();
        TotalMemory = (gcInfo.TotalAvailableMemoryBytes / 1024 / 1024).To<int>();


        _ = Task.Run(StartCounters);
    }

    private void StartCounters()
    {
        if (IsInitialized)
        {
            return;
        }

        try
        {
            string netInterface = NetworkHelper.GetNetworkInterfaceName();


            Parallel.Invoke(
                () => _cpuCounter = new("Processor", "% Processor Time", "_Total"),
                () => _ramCounter = new("Memory", "Available MBytes"),
                () => _diskWriteCounter = new("PhysicalDisk", "Disk Write Bytes/sec", "_Total"),
                () => _diskReadCounter = new("PhysicalDisk", "Disk Read Bytes/sec", "_Total"),
                () => _pBase = new("Processor Information", "Processor Frequency", "_Total"),
                () => _pPercent = new("Processor Information", "% of Maximum Frequency", "_Total"),
                () =>
                {
                    if (!string.IsNullOrEmpty(netInterface))
                    {
                        _networkUploadCounter = new("Network Interface", "Bytes Sent/sec", netInterface);
                        _networkDownloadCounter = new("Network Interface", "Bytes Received/sec", netInterface);
                    }
                },
                () =>
                {
                    _ = (_cpuCounter?.NextValue());
                    _ = (_pBase?.NextValue());
                    _ = (_pPercent?.NextValue());
                }
            );

            IsInitialized = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PerfService] Error en carga: {ex.Message}");
        }
    }



    public int GetCpuUsagePercentage()
    {
        return (_cpuCounter?.NextValue() ?? 0).To<int>();
    }

    public float GetCpuUsageInGHz()
    {
        if (_pBase == null || _pPercent == null)
        {
            return 0f;
        }

        try
        {

            float currentGHz = _pBase.NextValue() * (_pPercent.NextValue() / 100f) / 1000f;
            return Math.Round(currentGHz, 2).To<float>();
        }
        catch { return 0f; }
    }

    public int GetUsedMemory()
    {
        if (_ramCounter == null)
        {
            return 0;
        }

        try
        {
            int available = _ramCounter.NextValue().To<int>();
            return Math.Max(0, TotalMemory - available);
        }
        catch { return 0; }
    }

    public int GetUsedMemoryPercentage()
    {
        return TotalMemory <= 0 ? 0 : (GetUsedMemory() / (float)TotalMemory * 100).To<int>();
    }

    public int GetNetworkUploadSpeed()
    {
        return GetSafeValue(_networkUploadCounter);
    }

    public int GetNetworkDownloadSpeed()
    {
        return GetSafeValue(_networkDownloadCounter);
    }

    public int GetDiskWriteSpeed()
    {
        return GetSafeValue(_diskWriteCounter);
    }

    public int GetDiskReadSpeed()
    {
        return GetSafeValue(_diskReadCounter);
    }

    private static int GetSafeValue(PerformanceCounter? counter)
    {
        if (counter == null)
        {
            return 0;
        }

        try
        {

            return (counter.NextValue() / 1024).To<int>();
        }
        catch { return 0; }
    }
}
