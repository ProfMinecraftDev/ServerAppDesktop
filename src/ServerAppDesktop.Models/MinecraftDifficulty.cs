namespace ServerAppDesktop.Models;

public sealed class MinecraftDifficulty
{
    public string Name { get; set; } = "";
    public string Value { get; set; } = "";

    public MinecraftDifficulty(string name, string value)
    {
        Name = name;
        Value = value;
    }
}
