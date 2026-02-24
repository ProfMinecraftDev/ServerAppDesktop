namespace ServerAppDesktop.Models;

[JsonSerializable(typeof(AppSettings))]
[JsonSerializable(typeof(UISettings))]
[JsonSerializable(typeof(ServerSettings))]
[JsonSerializable(typeof(StartupSettings))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(int))]
public sealed partial class AppSettingsJsonContext : JsonSerializerContext { }

public sealed class AppSettings
{
    [JsonPropertyName("$schema")]
    public string JSONSchema { get; } = "https://raw.githubusercontent.com/ProfMinecraftDev/json-schemas/refs/heads/main/Server%20App%20Desktop%20(Preview)/Settings/appsettings.schema.json";

    public UISettings UI { get; set; } = new();
    public ServerSettings Server { get; set; } = new();
    public StartupSettings Startup { get; set; } = new();
}
