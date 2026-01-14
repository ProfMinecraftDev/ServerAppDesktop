namespace ServerAppDesktop.Models
{
    public sealed class ServerSettings
    {
        public string Path { get; set; } = "";
        public string Executable { get; set; } = "";
        public int Edition { get; set; } = 0;
        public int RamLimit { get; set; } = 1024;
    }
}
