namespace ServerAppDesktop.Services;

public sealed partial class PerformanceService : IPerformanceService, IDisposable
{
#pragma warning disable IDE0044
#nullable disable
    private PdhCloseQuerySafeHandle _query;
#nullable restore
    private PDH_HCOUNTER _cpuCounter, _ramCounter, _diskWCounter, _diskRCounter, _cpuFreqCounter;
    private PDH_HCOUNTER _netUpCounter, _netDownCounter;
    private Timer? _refreshTimer;
#pragma warning restore IDE0044
    private readonly ConcurrentDictionary<PDH_HCOUNTER, double> _cache = new();
    private const PDH_FMT PDH_FMT_DOUBLE = PDH_FMT.PDH_FMT_DOUBLE;

    public int TotalMemory { get; }
    public bool IsInitialized { get; private set; }
    private const int REFRESH = 1000;

    public PerformanceService()
    {
        TotalMemory = (int)(GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / 1024 / 1024);
        InitCounters();
        _refreshTimer = new Timer(Refresh, null, 0, REFRESH);
    }

    private unsafe void InitCounters()
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

        string netIf = NetworkHelper.GetNetworkInterfaceName();
        if (!string.IsNullOrEmpty(netIf))
        {
            string encNet = netIf.Replace("(", "[").Replace(")", "]");
            uint netUpRes = PInvoke.PdhAddEnglishCounter(_query, $@"\Network Interface({encNet})\Bytes Sent/sec", 0, out _netUpCounter);
            uint netDownRes = PInvoke.PdhAddEnglishCounter(_query, $@"\Network Interface({encNet})\Bytes Received/sec", 0, out _netDownCounter);

            if (netUpRes != 0 || netDownRes != 0)
            {
                _netUpCounter = default;
                _netDownCounter = default;
            }
        }

        IsInitialized = true;
    }

    private unsafe void Refresh(object? state)
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
        return (fmtRes == 0 && val.CStatus == 0)
            ? val.Anonymous.doubleValue
            : 0;
    }

    public int GetCpuUsagePercentage()
    {
        return (int)Math.Round(GetVal(_cpuCounter));
    }

    public int GetUsedMemory()
    {
        return Math.Max(0, TotalMemory - (int)Math.Round(GetVal(_ramCounter)));
    }

    public int GetUsedMemoryPercentage()
    {
        return TotalMemory > 0 ? (int)(GetUsedMemory() / (double)TotalMemory * 100) : 0;
    }

    public int GetDiskWriteSpeed()
    {
        return (int)(GetVal(_diskWCounter) / 1024.0);
    }

    public int GetDiskReadSpeed()
    {
        return (int)(GetVal(_diskRCounter) / 1024.0);
    }

    public int GetNetworkUploadSpeed()
    {
        return (int)(GetVal(_netUpCounter) / 1024.0);
    }

    public int GetNetworkDownloadSpeed()
    {
        return (int)(GetVal(_netDownCounter) / 1024.0);
    }

    public float GetCpuUsageInGHz()
    {
        return (float)(GetVal(_cpuFreqCounter) / 1000.0);
    }

    private double GetVal(PDH_HCOUNTER handle)
    {
        return _cache.TryGetValue(handle, out double v) ? v : 0;
    }

    public void Dispose()
    {
        _refreshTimer?.Dispose();
        if (_query is not null && !_query.IsInvalid)
        {
            _query.Dispose();
        }
        GC.SuppressFinalize(this);
    }
}
