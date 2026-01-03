using System.Text.Json.Serialization;

namespace ServerAppDesktop.Models
{
    public class Asset
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("browser_download_url")]
        public string DownloadUrl { get; set; } = "";

        private string _sha256 = "";

        [JsonPropertyName("digest")]
        public string SHA256
        {
            get => _sha256.Replace("sha256:", "");
            set => _sha256 = value ?? "";
        }
    }
}
