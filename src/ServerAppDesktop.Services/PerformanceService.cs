using System;
using System.Diagnostics;
using ServerAppDesktop.Helpers;

namespace ServerAppDesktop.Services
{
    public sealed class PerformanceService : IPerformanceService
    {
        private readonly PerformanceCounter _cpuCounter;
        private readonly PerformanceCounter _ramCounter;
        private readonly PerformanceCounter _diskWriteCounter;
        private readonly PerformanceCounter _diskReadCounter;
        private readonly PerformanceCounter? _networkUploadCounter;
        private readonly PerformanceCounter? _networkDownloadCounter;
        private readonly PerformanceCounter _pBase;
        private readonly PerformanceCounter _pPercent;

        public PerformanceService()
        {
            string netInterface = NetworkHelper.GetNetworkInterfaceName();

            // Inicializamos TODOS una sola vez
            _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            _ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            _diskWriteCounter = new PerformanceCounter("PhysicalDisk", "Disk Write Bytes/sec", "_Total");
            _diskReadCounter = new PerformanceCounter("PhysicalDisk", "Disk Read Bytes/sec", "_Total");
            _pBase = new PerformanceCounter("Processor Information", "Processor Frequency", "_Total");
            _pPercent = new PerformanceCounter("Processor Information", "% of Maximum Frequency", "_Total");

            // Solo si la interfaz existe evitamos el crash
            if (!string.IsNullOrEmpty(netInterface))
            {
                _networkUploadCounter = new PerformanceCounter("Network Interface", "Bytes Sent/sec", netInterface);
                _networkDownloadCounter = new PerformanceCounter("Network Interface", "Bytes Received/sec", netInterface);
            }

            // Calentamos los contadores (el primer NextValue siempre es basura/0)
            _cpuCounter.NextValue();
            _pBase.NextValue();
            _pPercent.NextValue();
        }

        public int GetCpuUsagePercentage()
        {
            // El primer NextValue() siempre da 0, el segundo ya da la lectura real
            return (int)_cpuCounter.NextValue();
        }

        public float GetCpuUsageInGHz()
        {
            // 1. Necesitamos la frecuencia máxima (Base)
            // Esto lo puedes cachear en el constructor para no pedirlo cada segundo
            float baseFrequency = _pBase.NextValue(); // Ej: 3600 (MHz)

            // 2. Necesitamos el % de rendimiento actual respecto a la base
            float percent = _pPercent.NextValue();

            // 3. Calculamos: (Frecuencia Base * Porcentaje) / 1000 para pasar a GHz
            float currentGHz = (baseFrequency * (percent / 100f)) / 1000f;

            return (float)Math.Round(currentGHz, 2);
        }

        public int GetUsedMemory()
        {
            // El contador de "Memory" nos da lo DISPONIBLE. 
            // Para sacar el USADO: Total - Disponible.
            int total = GetTotalMemory();
            int available = (int)_ramCounter.NextValue();
            return total - available;
        }

        public int GetUsedMemoryPercentage()
        {
            int total = GetTotalMemory();
            int used = GetUsedMemory();
            return (int)((used / (float)total) * 100);
        }

        public int GetTotalMemory()
        {
            // Usamos el Helper de Windows para obtener la RAM física total
            var gcStatus = GC.GetGCMemoryInfo();
            return (int)(gcStatus.TotalAvailableMemoryBytes / 1024 / 1024);
        }

        // Para la red (Bytes -> Kilobits o Kilobytes)
        public int GetNetworkUploadSpeed() => (int)(_networkUploadCounter?.NextValue() / 1024 ?? 0);
        public int GetNetworkDownloadSpeed() => (int)(_networkDownloadCounter?.NextValue() / 1024 ?? 0);

        public int GetDiskWriteSpeed() => (int)(_diskWriteCounter.NextValue() / 1024);
        public int GetDiskReadSpeed() => (int)(_diskReadCounter.NextValue() / 1024);
    }
}
