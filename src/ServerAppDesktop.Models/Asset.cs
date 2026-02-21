namespace ServerAppDesktop.Models
{
    public sealed class Asset
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("browser_download_url")]
        public string DownloadUrl { get; set; } = "";

        [JsonPropertyName("digest")]
        public string SHA256
        {
            get => field.Replace("sha256:", "");
            set => field = value ?? "";
        } = "";
    }
}
