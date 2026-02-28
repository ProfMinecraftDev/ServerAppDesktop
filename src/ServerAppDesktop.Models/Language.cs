namespace ServerAppDesktop.Models;

public sealed class Language
{
    public string Name { get; set; }
    public string Code { get; set; }

    public Language(string name, string code)
    {
        Name = name;
        Code = code;
    }
}
