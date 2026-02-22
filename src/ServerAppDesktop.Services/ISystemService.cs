namespace ServerAppDesktop.Services;

public interface ISystemService
{
    public string CPUName { get; set; }
    public string GPUName { get; set; }
    public int MemoryGB { get; set; }
    public string PCModel { get; set; }
    public string WindowsVersion { get; set; }
    public string LicenseType { get; set; }
    public string ActiveProcess { get; set; }
    public int ActiveProcessId { get; set; }
    public string StorageType { get; set; }
    public string StorageSize { get; set; }
    public string ActiveUser { get; set; }
}
