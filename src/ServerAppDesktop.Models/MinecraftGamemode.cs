namespace ServerAppDesktop.Models;

public sealed class MinecraftGamemode
{
    public string Name { get; set; } = "";
    public string Value { get; set; } = "";
    public MinecraftGamemode(string name, string value)
    {
        Name = name;
        Value = value;
    }
}
