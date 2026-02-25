namespace ServerAppDesktop.Services;

public sealed partial class PerformanceService : IPerformanceService, IDisposable
{
#nullable disable
    private PdhCloseQuerySafeHandle _query;
#nullable restore
    private PDH_HCOUNTER _cpuCounter, _ramCounter, _diskWCounter, _diskRCounter, _cpuFreqCounter, _cpuMaxFreqCounter;
    private PDH_HCOUNTER _netUpCounter, _netDownCounter;
    private readonly ConcurrentDictionary<PDH_HCOUNTER, double> _cache = new();
    private const PDH_FMT PDH_FMT_DOUBLE = PDH_FMT.PDH_FMT_DOUBLE;

    public int TotalMemory { get; }
    public bool IsInitialized { get; private set; }

    public PerformanceService()
    {
        TotalMemory = (GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / 1024 / 1024).To<int>();
        InitCounters();
    }

    private void InitCounters()
    {
        uint openResult = PInvoke.PdhOpenQuery(null, 0, out _query);
        if (openResult != 0)
        {
            throw new Exception($"Unable to open PDH query: {openResult:X}");
        }

        _ = PInvoke.PdhAddEnglishCounter(_query, @"\Processor(_Total)\% Processor Time", 0, out _cpuCounter);
        _ = PInvoke.PdhAddEnglishCounter(_query, @"\Memory\Available MBytes", 0, out _ramCounter);
        _ = PInvoke.PdhAddEnglishCounter(_query, @"\PhysicalDisk(_Total)\Disk Write Bytes/sec", 0, out _diskWCounter);
        _ = PInvoke.PdhAddEnglishCounter(_query, @"\PhysicalDisk(_Total)\Disk Read Bytes/sec", 0, out _diskRCounter);
        _ = PInvoke.PdhAddEnglishCounter(_query, @"\Processor Information(_Total)\Processor Frequency", 0, out _cpuFreqCounter);
        _ = PInvoke.PdhAddEnglishCounter(_query, @"\Processor Information(_Total)\% of Maximum Frequency", 0, out _cpuMaxFreqCounter);

        string netIf = NetworkHelper.GetNetworkInterfaceName();
        if (!string.IsNullOrEmpty(netIf))
        {
            string encNet = netIf.Replace("(", "[").Replace(")", "]");
            _ = PInvoke.PdhAddEnglishCounter(_query, $@"\Network Interface({encNet})\Bytes Sent/sec", 0, out _netUpCounter);
            _ = PInvoke.PdhAddEnglishCounter(_query, $@"\Network Interface({encNet})\Bytes Received/sec", 0, out _netDownCounter);
        }

        _ = PInvoke.PdhCollectQueryData((PDH_HQUERY)_query.DangerousGetHandle());
        IsInitialized = true;
    }

    public unsafe void Refresh()
    {
        if (!IsInitialized)
        {
            return;
        }

        uint collectRes = PInvoke.PdhCollectQueryData((PDH_HQUERY)_query.DangerousGetHandle());
        if (collectRes != 0)
        {
            return;
        }

        _cache[_cpuCounter] = QueryDouble(_cpuCounter);
        _cache[_ramCounter] = QueryDouble(_ramCounter);
        _cache[_diskWCounter] = QueryDouble(_diskWCounter);
        _cache[_diskRCounter] = QueryDouble(_diskRCounter);
        _cache[_cpuFreqCounter] = QueryDouble(_cpuFreqCounter);
        _cache[_cpuMaxFreqCounter] = QueryDouble(_cpuMaxFreqCounter);

        if (_netUpCounter.Value != null)
        {
            _cache[_netUpCounter] = QueryDouble(_netUpCounter);
        }

        if (_netDownCounter.Value != null)
        {
            _cache[_netDownCounter] = QueryDouble(_netDownCounter);
        }
    }

    private static unsafe double QueryDouble(PDH_HCOUNTER counter)
    {
        PDH_FMT_COUNTERVALUE val;
        uint fmtRes = PInvoke.PdhGetFormattedCounterValue(counter, PDH_FMT_DOUBLE, null, &val);
        return (fmtRes == 0 && val.CStatus == 0) ? val.Anonymous.doubleValue : 0;
    }

    public int GetCpuUsagePercentage()
    {
        return Math.Round(GetVal(_cpuCounter)).To<int>();
    }

    public int GetUsedMemory()
    {
        return Math.Max(0, TotalMemory - Math.Round(GetVal(_ramCounter)).To<int>());
    }

    public int GetUsedMemoryPercentage()
    {
        return TotalMemory > 0 ? (GetUsedMemory() / TotalMemory.To<double>() * 100).To<int>() : 0;
    }

    public int GetDiskWriteSpeed()
    {
        return (GetVal(_diskWCounter) / 1024.0).To<int>();
    }

    public int GetDiskReadSpeed()
    {
        return (GetVal(_diskRCounter) / 1024.0).To<int>();
    }

    public int GetNetworkUploadSpeed()
    {
        return (GetVal(_netUpCounter) / 1024.0).To<int>();
    }

    public int GetNetworkDownloadSpeed()
    {
        return (GetVal(_netDownCounter) / 1024.0).To<int>();
    }

    public float GetCpuUsageInGHz()
    {
        double freq = GetVal(_cpuFreqCounter);
        if (freq <= 0)
        {
            return 0f;
        }

        float currentGHz = (freq * (GetVal(_cpuMaxFreqCounter) / 100.0) / 1000.0).To<float>();
        return Math.Round(currentGHz, 2).To<float>();
    }

    private double GetVal(PDH_HCOUNTER handle)
    {
        return _cache.TryGetValue(handle, out double v) ? v : 0;
    }

    public void Dispose()
    {
        if (_query is not null && !_query.IsInvalid)
        {
            _query.Dispose();
        }
        GC.SuppressFinalize(this);
    }
}
