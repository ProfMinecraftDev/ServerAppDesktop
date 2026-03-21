namespace ServerAppDesktop.Models;

public sealed class Language(string name, string code)
{
    public string Name { get; set; } = name;
    public string Code { get; set; } = code;
}
