using System.Text.Json.Serialization;

namespace ServerAppDesktop.Models
{
    [JsonSerializable(typeof(AppSettings))]
    [JsonSerializable(typeof(UISettings))]
    [JsonSerializable(typeof(ServerSettings))]
    [JsonSerializable(typeof(StartupSettings))]
    public sealed partial class AppSettingsJsonContext : JsonSerializerContext { }

    public sealed class AppSettings
    {
        [JsonPropertyName("$schema")]
        public string JSONSchema { get; set; } = "";
        public UISettings UI { get; set; } = new();
        public ServerSettings Server { get; set; } = new();
        public StartupSettings Startup { get; set; } = new();
    }
}
