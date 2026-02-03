using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ServerAppDesktop.Helpers;

namespace ServerAppDesktop.Services
{
    public sealed class PerformanceService : IPerformanceService
    {
        private PerformanceCounter? _cpuCounter, _ramCounter, _diskWriteCounter, _diskReadCounter;
        private PerformanceCounter? _networkUploadCounter, _networkDownloadCounter;
        private PerformanceCounter? _pBase, _pPercent;

        public int TotalMemory { get; }
        public bool IsInitialized { get; private set; }

        public PerformanceService()
        {
            // 1. RAM Física (Instantáneo)
            var gcInfo = GC.GetGCMemoryInfo();
            TotalMemory = (int)(gcInfo.TotalAvailableMemoryBytes / 1024 / 1024);

            // 2. Ejecutar carga sin bloquear el hilo principal
            _ = Task.Run(StartCounters);
        }

        private void StartCounters()
        {
            if (IsInitialized)
                return;
            try
            {
                string netInterface = NetworkHelper.GetNetworkInterfaceName();

                // Instanciación en paralelo para ahorrar tiempo (de ~2s a ~0.4s)
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
                    }
                );

                // "Calentamiento": La primera lectura siempre es 0
                _cpuCounter?.NextValue();
                _pBase?.NextValue();
                _pPercent?.NextValue();

                IsInitialized = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PerfService] Error en carga: {ex.Message}");
            }
        }

        // --- MÉTODOS DE LECTURA BLINDADOS ---

        public int GetCpuUsagePercentage() => (int)(_cpuCounter?.NextValue() ?? 0);

        public float GetCpuUsageInGHz()
        {
            if (_pBase == null || _pPercent == null)
                return 0f;
            try
            {
                // Frecuencia actual = (Frecuencia Base * % de uso de frecuencia)
                float currentGHz = (_pBase.NextValue() * (_pPercent.NextValue() / 100f)) / 1000f;
                return (float)Math.Round(currentGHz, 2);
            }
            catch { return 0f; }
        }

        public int GetUsedMemory()
        {
            if (_ramCounter == null)
                return 0;
            try
            {
                int available = (int)_ramCounter.NextValue();
                return Math.Max(0, TotalMemory - available);
            }
            catch { return 0; }
        }

        public int GetUsedMemoryPercentage()
        {
            if (TotalMemory <= 0)
                return 0;
            return (int)((GetUsedMemory() / (float)TotalMemory) * 100);
        }

        public int GetNetworkUploadSpeed() => GetSafeValue(_networkUploadCounter);
        public int GetNetworkDownloadSpeed() => GetSafeValue(_networkDownloadCounter);
        public int GetDiskWriteSpeed() => GetSafeValue(_diskWriteCounter);
        public int GetDiskReadSpeed() => GetSafeValue(_diskReadCounter);

        private int GetSafeValue(PerformanceCounter? counter)
        {
            if (counter == null)
                return 0;
            try
            {
                // Convertimos Bytes/sec a KB/sec
                return (int)(counter.NextValue() / 1024);
            }
            catch { return 0; }
        }
    }
}