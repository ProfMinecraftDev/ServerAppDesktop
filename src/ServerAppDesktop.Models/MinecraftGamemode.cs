namespace ServerAppDesktop.Models;

public sealed class MinecraftGamemode(string name, string value)
{
    public string Name { get; set; } = name;
    public string Value { get; set; } = value;
}
